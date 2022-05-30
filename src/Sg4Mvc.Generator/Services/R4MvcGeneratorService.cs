using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sg4Mvc.Generator.CodeGen;
using Sg4Mvc.Generator.Extensions;
using Sg4Mvc.Generator.Services.Interfaces;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Sg4Mvc.Generator.Services;

public class Sg4MvcGeneratorService
{
    private readonly IControllerRewriterService _controllerRewriter;
    private readonly IControllerGeneratorService _controllerGenerator;
    private readonly IPageGeneratorService _pageGenerator;
    private readonly IStaticFileGeneratorService _staticFileGenerator;
    private readonly IFilePersistService _filePersistService;
    private readonly Settings _settings;

    public Sg4MvcGeneratorService(
        IControllerRewriterService controllerRewriter,
        IControllerGeneratorService controllerGenerator,
        IPageGeneratorService pageGenerator,
        IStaticFileGeneratorService staticFileGenerator,
        IFilePersistService filePersistService,
        Settings settings)
    {
        _controllerRewriter = controllerRewriter;
        _controllerGenerator = controllerGenerator;
        _pageGenerator = pageGenerator;
        _staticFileGenerator = staticFileGenerator;
        _filePersistService = filePersistService;
        _settings = settings;
    }

    public void Generate(GeneratorExecutionContext context, IList<ControllerDefinition> controllers, IList<PageView> pages)
    {
        var areaControllers = controllers.ToLookup(c => c.Area);

        // Processing controllers, generating partial and derived controller classes for Sg4Mvc
        var generatedControllers = new List<NamespaceDeclarationSyntax>();
        foreach (var namespaceGroup in controllers.Where(c => c.Namespace != null).GroupBy(c => c.Namespace).OrderBy(c => c.Key))
        {
            var namespaceNode = NamespaceDeclaration(ParseName(namespaceGroup.Key));
            foreach (var controller in namespaceGroup.OrderBy(c => c.Name))
            {
                namespaceNode = namespaceNode.AddMembers(
                    _controllerGenerator.GeneratePartialController(controller),
                    _controllerGenerator.GenerateSg4Controller(controller));

                // If SplitIntoMultipleFiles is set, store the generated classes alongside the controller files.
                if (_settings.SplitIntoMultipleFiles)
                {
                    var generatedFilePath = controller.FullyQualifiedGeneratedName.Replace('.', '_') + ".generated.cs";

                    var controllerFile = new CodeFileBuilder(_settings, true)
                        .WithNamespace(namespaceNode);

                    _filePersistService.WriteFile(controllerFile.Build(), generatedFilePath);

                    namespaceNode = NamespaceDeclaration(ParseName(namespaceGroup.Key));
                }
            }

            // If SplitIntoMultipleFiles is NOT set, bundle them all in Sg4Mvc
            if (namespaceNode.Members.Count > 0)
            {
                generatedControllers.Add(namespaceNode);
            }
        }

        var generatedPages = new List<NamespaceDeclarationSyntax>();
        foreach (var namespaceGroup in pages.GroupBy(p => p.Definition?.Namespace ?? _settings.Sg4MvcNamespace).OrderBy(p => p.Key))
        {
            var namespaceNode = NamespaceDeclaration(ParseName(namespaceGroup.Key));
            foreach (var page in namespaceGroup.OrderBy(p => p.Name))
            {
                var viewOnlyPageFile = page.Definition == null;
                if (!viewOnlyPageFile)
                {
                    namespaceNode = namespaceNode.AddMembers(
                        _pageGenerator.GeneratePartialPage(page),
                        _pageGenerator.GenerateSg4Page(page.Definition));
                }
                else
                {
                    namespaceNode = namespaceNode.AddMembers(
                        CreateViewOnlyPageClass(page));
                }

                // If SplitIntoMultipleFiles is set, store the generated classes alongside the controller files.
                if (_settings.SplitIntoMultipleFiles && (_settings.SplitViewOnlyPagesIntoMultipleFiles || !viewOnlyPageFile))
                {
                    var userPageFile = page.Definition.GetFilePath();
                    if (!File.Exists(userPageFile))
                    {
                        var result = new CodeFileBuilder(_settings, false)
                            .WithNamespace(NamespaceDeclaration(ParseName(page.Definition.Namespace))
                                .AddMembers(new ClassBuilder(page.Definition.Name)
                                    .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword)
                                    .WithComment("// Use this file to add custom extensions and helper methods to this page")
                                    .Build()))
                            .Build();
                        _filePersistService.WriteFile(result, userPageFile);
                    }

                    var generatedFilePath = page.Definition.FullyQualifiedGeneratedName
                        .Replace('.', '_') + ".generated.cs";

                    var pageFile = new CodeFileBuilder(_settings, true)
                        .WithNamespace(namespaceNode);

                    _filePersistService.WriteFile(pageFile.Build(), generatedFilePath);
                    namespaceNode = NamespaceDeclaration(ParseName(namespaceGroup.Key));
                }
            }

            // If SplitIntoMultipleFiles is NOT set, bundle them all in Sg4Mvc
            if (namespaceNode.Members.Count > 0)
            {
                generatedPages.Add(namespaceNode);
            }
        }

        // Sg4Mvc namespace used for the areas and Dummy class
        var sg4Namespace = NamespaceDeclaration(ParseName(_settings.Sg4MvcNamespace))
            // add the dummy class uses in the derived controller partial class
            /* [GeneratedCode, DebuggerNonUserCode]
             * public class Dummy
             * {
             *  private Dummy() {}
             *  public static Dummy Instance = new Dummy();
             * }
             */
            .AddMembers(new ClassBuilder(Constants.DummyClass)
                .WithModifiers(SyntaxKind.PublicKeyword)
                .WithGeneratedNonUserCodeAttributes()
                .WithConstructor(c => c
                    .WithModifiers(SyntaxKind.PrivateKeyword))
                .WithField(Constants.DummyClassInstance, Constants.DummyClass, Constants.DummyClass, SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                .Build())
            .AddMembers(CreateViewOnlyControllerClasses(controllers).ToArray<MemberDeclarationSyntax>())
            .AddMembers(CreateAreaClasses(areaControllers).ToArray<MemberDeclarationSyntax>())
            .AddMembers(CreatePagePathClasses(pages, out var topLevelPagePaths).ToArray<MemberDeclarationSyntax>());

        // create static MVC class and add the area and controller fields
        var mvcStaticClass = new ClassBuilder(_settings.HelpersPrefix)
            .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.PartialKeyword)
            .WithGeneratedNonUserCodeAttributes();
        foreach (var area in areaControllers.Where(a => !String.IsNullOrEmpty(a.Key)).OrderBy(a => a.Key))
        {
            mvcStaticClass.WithStaticFieldBackedProperty(area.First().AreaKey, $"{_settings.Sg4MvcNamespace}.{area.Key}AreaClass", SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword);
        }
        foreach (var controller in areaControllers[String.Empty].OrderBy(c => c.Namespace == null).ThenBy(c => c.Name))
        {
            mvcStaticClass.WithField(
                controller.Name,
                controller.FullyQualifiedGeneratedName,
                controller.FullyQualifiedSg4ClassName ?? controller.FullyQualifiedGeneratedName,
                SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword);
        }

        var mvcPagesStaticClass = new ClassBuilder(_settings.PageHelpersPrefix)
            .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.PartialKeyword)
            .WithGeneratedNonUserCodeAttributes();
        if (topLevelPagePaths != null)
        {
            foreach (var set in topLevelPagePaths)
            {
                mvcPagesStaticClass.WithStaticFieldBackedProperty(set.Key, set.Value, SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword);
            }
        }

        foreach (var page in pages.Where(p => p.Segments.Length == 0))
        {
            mvcPagesStaticClass.WithField(
                page.Name,
                page.Definition.FullyQualifiedGeneratedName,
                page.Definition.FullyQualifiedSg4ClassName ?? page.Definition.FullyQualifiedGeneratedName,
                SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword);
        }

        // Generate a list of all static files from the wwwroot path
        var staticFileNode = _staticFileGenerator.GenerateStaticFiles(context.GetWorkingDirectory());

        var sg4MvcFile = new CodeFileBuilder(_settings, true)
            .WithMembers(
                mvcStaticClass.Build(),
                mvcPagesStaticClass.Build(),
                sg4Namespace,
                staticFileNode,
                Sg4MvcHelpersClass(),
                ActionResultClass(),
                JsonResultClass(),
                ContentResultClass(),
                FileResultClass(),
                RedirectResultClass(),
                RedirectToActionResultClass(),
                RedirectToRouteResultClass())
            .WithMembers(true,
                PageActionResultClass(),
                PageJsonResultClass(),
                PageContentResultClass(),
                PageFileResultClass(),
                PageRedirectResultClass(),
                PageRedirectToActionResultClass(),
                PageRedirectToRouteResultClass())
            .WithNamespaces(generatedControllers)
            .WithNamespaces(generatedPages);

        Console.WriteLine("Generating " + Path.DirectorySeparatorChar + Constants.Sg4MvcGeneratedFileName);

        _filePersistService.WriteFile(sg4MvcFile.Build(), Constants.Sg4MvcGeneratedFileName);
    }

    public IEnumerable<ClassDeclarationSyntax> CreateViewOnlyControllerClasses(IList<ControllerDefinition> controllers)
    {
        foreach (var controller in controllers.Where(c => c.Namespace == null).OrderBy(c => c.Area).ThenBy(c => c.Name))
        {
            var className = !String.IsNullOrEmpty(controller.Area)
                ? $"{controller.Area}Area_{controller.Name}Controller"
                : $"{controller.Name}Controller";
            controller.FullyQualifiedGeneratedName = $"{_settings.Sg4MvcNamespace}.{className}";

            var controllerClass = new ClassBuilder(className)
                .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword)
                .WithGeneratedNonUserCodeAttributes();
            _controllerGenerator.WithViewsClass(controllerClass, controller.Views);
            yield return controllerClass.Build();
        }
    }

    public ClassDeclarationSyntax CreateViewOnlyPageClass(PageView page)
    {
        var generatedPath = page.FilePath + ".cs";
        var className = String.Join("_", page.Segments.Concat(new[] { page.Name })) + "Model";
        page.Definition = new PageDefinition(_settings.Sg4MvcNamespace, className, false, null, new List<String> { generatedPath });
        page.Definition.FullyQualifiedGeneratedName = $"{_settings.Sg4MvcNamespace}.{className}";

        var pageClass = new ClassBuilder(className)
            .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword)
            .WithBaseTypes("ISg4ActionResult");
        _pageGenerator.AddSg4ActionMethods(pageClass, page.PagePath);
        if (_settings.GeneratePageViewsClass)
        {
            _pageGenerator.WithViewsClass(pageClass, new[] { page });
        }

        return pageClass.Build();
    }

    public IEnumerable<ClassDeclarationSyntax> CreateAreaClasses(ILookup<String, ControllerDefinition> areaControllers)
    {
        foreach (var area in areaControllers.Where(a => !String.IsNullOrEmpty(a.Key)).OrderBy(a => a.Key))
        {
            var areaClass = new ClassBuilder(area.Key + "AreaClass")
                .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword)
                .WithGeneratedNonUserCodeAttributes()
                .WithStringField("Name", area.Key, SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword)
                .ForEach(area.OrderBy(c => c.Namespace == null).ThenBy(c => c.Name), (cb, c) => cb
                    .WithField(
                        c.Name,
                        c.FullyQualifiedGeneratedName,
                        c.FullyQualifiedSg4ClassName ?? c.FullyQualifiedGeneratedName,
                        SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword));
            yield return areaClass.Build();
        }
    }

    public IEnumerable<ClassDeclarationSyntax> CreatePagePathClasses(IList<PageView> pages, out IDictionary<String, String> topLevelPagePaths)
    {
        if (!pages.Any())
        {
            topLevelPagePaths = null;
            return new ClassDeclarationSyntax[0];
        }

        var splitter = "_";
        while (pages.Any(p => p.Segments.Any(s => s.Contains(splitter))))
            splitter += "_";

        var pagePaths = pages
            .Where(p => p.Segments.Length > 0)
            .SelectMany(p => Enumerable.Range(1, p.Segments.Length)
                .Select(i => String.Join(splitter, p.Segments.Take(i))))
            .Distinct()
            .OrderBy(k => k)
            .ToList();
        var pageGroups = pages
            .ToLookup(p => String.Join(splitter, p.Segments));

        var pathClasses = new Dictionary<String, ClassBuilder>();

        foreach (var key in pagePaths)
        {
            var pathClass = new ClassBuilder(key + "PathClass")
                .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword)
                .WithGeneratedNonUserCodeAttributes()
                .ForEach(pageGroups[key].OrderBy(p => p.Name), (cb, p) => cb
                    .WithField(
                        p.Name,
                        p.Definition.FullyQualifiedGeneratedName,
                        p.Definition.FullyQualifiedSg4ClassName ?? p.Definition.FullyQualifiedGeneratedName,
                        SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword));
            pathClasses.Add(key, pathClass);
        }

        foreach (var key in pagePaths.Where(k => k.IndexOf(splitter) > 0))
        {
            var parentKey = key.Substring(0, key.LastIndexOf(splitter));
            pathClasses[parentKey]
                .WithStaticFieldBackedProperty(key.Substring(parentKey.Length + splitter.Length), $"{_settings.Sg4MvcNamespace}.{key}PathClass", SyntaxKind.PublicKeyword);
        }

        topLevelPagePaths = pagePaths.Where(k => k.IndexOf(splitter) == -1)
            .ToDictionary(k => k, k => $"{_settings.Sg4MvcNamespace}.{k}PathClass");

        return pathClasses.Values.Select(c => c.Build());
    }

    private ClassDeclarationSyntax IActionResultDerivedClass(String className, String baseClassName, Action<ConstructorMethodBuilder> constructorParts = null)
    {
        var result = new ClassBuilder(className)                                    // internal partial class {className}
            .WithGeneratedNonUserCodeAttributes()
            .WithModifiers(SyntaxKind.InternalKeyword, SyntaxKind.PartialKeyword)
            .WithBaseTypes(baseClassName, "ISg4MvcActionResult")                     // : {baseClassName}, ISg4MvcActionResult
            .WithConstructor(c => c
                .WithOther(constructorParts)
                .WithModifiers(SyntaxKind.PublicKeyword)                        // public ctor(
                .WithParameter("area", "string")                                //  string area,
                .WithParameter("controller", "string")                          //  string controller,
                .WithParameter("action", "string")                              //  string action,
                .WithParameter("protocol", "string", defaultsToNull: true)      //  string protocol = null)
                .WithBody(b => b                                                    // this.InitMVCT4Result(area, controller, action, protocol);
                    .MethodCall("this", "InitMVCT4Result", "area", "controller", "action", "protocol")))
            .WithProperty("Controller", "string")                                   // public string Controller { get; set; }
            .WithProperty("Action", "string")                                       // public string Action { get; set; }
            .WithProperty("Protocol", "string")                                     // public string Protocol { get; set; }
            .WithProperty("RouteValueDictionary", "RouteValueDictionary");          // public RouteValueDictionary RouteValueDictionary { get; set; }

        return result.Build();
    }

    private ClassDeclarationSyntax IActionResultDerivedPageClass(String className, String baseClassName, Action<ConstructorMethodBuilder> constructorParts = null)
    {
        var result = new ClassBuilder(className)                                    // internal partial class {className}
            .WithGeneratedNonUserCodeAttributes()
            .WithModifiers(SyntaxKind.InternalKeyword, SyntaxKind.PartialKeyword)
            .WithBaseTypes(baseClassName, "ISg4PageActionResult")                    // : {baseClassName}, ISg4ActionResult
            .WithConstructor(c => c
                .WithOther(constructorParts)
                .WithModifiers(SyntaxKind.PublicKeyword)                        // public ctor(
                .WithParameter("pageName", "string")                            //  string pageName,
                .WithParameter("pageHandler", "string")                         //  string pageHandler,
                .WithParameter("protocol", "string", defaultsToNull: true)      //  string protocol = null)
                .WithBody(b => b                                                    // this.InitMVCT4Result(pageName, pageHandler, protocol);
                    .MethodCall("this", "InitMVCT4Result", "pageName", "pageHandler", "protocol")))
            .WithProperty("PageName", "string")                                     // public string PageName { get; set; }
            .WithProperty("PageHandler", "string")                                  // public string PageHandler { get; set; }
            .WithProperty("Protocol", "string")                                     // public string Protocol { get; set; }
            .WithProperty("RouteValueDictionary", "RouteValueDictionary");          // public RouteValueDictionary RouteValueDictionary { get; set; }

        return result.Build();
    }

    public ClassDeclarationSyntax Sg4MvcHelpersClass()
        => new ClassBuilder(Constants.Sg4MvcHelpersClass)
            .WithModifiers(SyntaxKind.InternalKeyword, SyntaxKind.StaticKeyword)
            .WithGeneratedNonUserCodeAttributes()
            .WithMethod(Constants.Sg4MvcHelpers_ProcessVirtualPath + "Default", "string", m => m
                .WithModifiers(SyntaxKind.PrivateKeyword, SyntaxKind.StaticKeyword)
                .WithParameter("virtualPath", "string")
                .WithExpresisonBody(IdentifierName("virtualPath")))
            .WithValueField(Constants.Sg4MvcHelpers_ProcessVirtualPath, "Func<string, string>", Constants.Sg4MvcHelpers_ProcessVirtualPath + "Default", SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
            .Build();

    public ClassDeclarationSyntax ActionResultClass()
        => IActionResultDerivedClass(Constants.ActionResultClass, "ActionResult");

    public ClassDeclarationSyntax JsonResultClass()
        => IActionResultDerivedClass(Constants.JsonResultClass, "JsonResult",
            c => c.WithBaseConstructorCall(SimpleLiteral.Null));                           // ctor : base(null)

    public ClassDeclarationSyntax ContentResultClass()
        => IActionResultDerivedClass(Constants.ContentResultClass, "ContentResult");

    public ClassDeclarationSyntax FileResultClass()
        => IActionResultDerivedClass(Constants.FileResultClass, "FileResult",
            c => c.WithBaseConstructorCall(SimpleLiteral.Null));                           // ctor : base(null)

    public ClassDeclarationSyntax RedirectResultClass()
        => IActionResultDerivedClass(Constants.RedirectResultClass, "RedirectResult",
            c => c.WithBaseConstructorCall(SimpleLiteral.Space));                          // ctor : base(" ")

    public ClassDeclarationSyntax RedirectToActionResultClass()
        => IActionResultDerivedClass(Constants.RedirectToActionResultClass, "RedirectToActionResult",
            c => c.WithBaseConstructorCall(SimpleLiteral.Space, SimpleLiteral.Space, SimpleLiteral.Space));  // ctor : base(" ", " ", " ")

    public ClassDeclarationSyntax RedirectToRouteResultClass()
        => IActionResultDerivedClass(Constants.RedirectToRouteResultClass, "RedirectToRouteResult",
            c => c.WithBaseConstructorCall(SimpleLiteral.Null));                           // ctor : base(null)

    public ClassDeclarationSyntax PageActionResultClass()
        => IActionResultDerivedPageClass(Constants.PageActionResultClass, "ActionResult");

    public ClassDeclarationSyntax PageJsonResultClass()
        => IActionResultDerivedPageClass(Constants.PageJsonResultClass, "JsonResult",
            c => c.WithBaseConstructorCall(SimpleLiteral.Null));                           // ctor : base(null)

    public ClassDeclarationSyntax PageContentResultClass()
        => IActionResultDerivedClass(Constants.PageContentResultClass, "ContentResult");

    public ClassDeclarationSyntax PageFileResultClass()
        => IActionResultDerivedPageClass(Constants.PageFileResultClass, "FileResult",
            c => c.WithBaseConstructorCall(SimpleLiteral.Null));                           // ctor : base(null)

    public ClassDeclarationSyntax PageRedirectResultClass()
        => IActionResultDerivedPageClass(Constants.PageRedirectResultClass, "RedirectResult",
            c => c.WithBaseConstructorCall(SimpleLiteral.Space));                          // ctor : base(" ")

    public ClassDeclarationSyntax PageRedirectToActionResultClass()
        => IActionResultDerivedPageClass(Constants.PageRedirectToActionResultClass, "RedirectToActionResult",
            c => c.WithBaseConstructorCall(SimpleLiteral.Space, SimpleLiteral.Space, SimpleLiteral.Space));  // ctor : base(" ", " ", " ")

    public ClassDeclarationSyntax PageRedirectToRouteResultClass()
        => IActionResultDerivedPageClass(Constants.PageRedirectToRouteResultClass, "RedirectToRouteResult",
            c => c.WithBaseConstructorCall(SimpleLiteral.Null));                           // ctor : base(null)
}
