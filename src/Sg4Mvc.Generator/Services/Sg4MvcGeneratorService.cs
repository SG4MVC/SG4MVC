using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sg4Mvc.Generator.CodeGen;
using Sg4Mvc.Generator.Controllers;
using Sg4Mvc.Generator.Controllers.Interfaces;
using Sg4Mvc.Generator.Extensions;
using Sg4Mvc.Generator.Pages;
using Sg4Mvc.Generator.Pages.Interfaces;
using Sg4Mvc.Generator.Services.Interfaces;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Sg4Mvc.Generator.Services;

public class Sg4MvcGeneratorService
{
    private readonly IControllerGeneratorService _controllerGenerator;
    private readonly IPageGeneratorService _pageGenerator;
    private readonly IStaticFileGeneratorService _staticFileGenerator;
    private readonly Settings _settings;
    private readonly SourceProductionContext _context;

    public Sg4MvcGeneratorService(
        IControllerGeneratorService controllerGenerator,
        IPageGeneratorService pageGenerator,
        IStaticFileGeneratorService staticFileGenerator,
        Settings settings,
        SourceProductionContext context
    )
    {
        _controllerGenerator = controllerGenerator;
        _pageGenerator = pageGenerator;
        _staticFileGenerator = staticFileGenerator;
        _settings = settings;
        _context = context;
    }

    public void Generate(String workingDirectory,
        IList<ControllerDefinition> controllers,
        IList<PageView> pages)
    {
        var areaControllers = controllers.ToLookup(c => c.Area);

        // Processing controllers, generating partial and derived controller classes for Sg4Mvc
        var namespaceGroups = controllers
            .Where(c => c.Namespace != null)
            .GroupBy(c => c.Namespace)
            .OrderBy(c => c.Key);

        using (new PerformanceLogger("Total Controllers"))
        {
            foreach (var namespaceGroup in namespaceGroups)
            {
                using (new PerformanceLogger($"Total Namespace: {namespaceGroup.Key}"))
                {
                    var namespaceNode = NamespaceDeclaration(ParseName(namespaceGroup.Key));
                    foreach (var controller in namespaceGroup.OrderBy(c => c.Name))
                    {
                        var partialController = _controllerGenerator.GeneratePartialController(controller);
                        var sg4Controller = _controllerGenerator.GenerateSg4Controller(controller);
                        namespaceNode = namespaceNode.AddMembers(
                            partialController,
                            sg4Controller);

                        var generatedFilePath =
                            controller.FullyQualifiedGeneratedName.Replace('.', '_') + ".generated.cs";

                        var controllerFile = new CodeFileBuilder(_settings, true)
                            .WithNamespace(namespaceNode);

                        var builtControllerFile = controllerFile.Build();

                        _context.WriteFile(builtControllerFile, generatedFilePath);

                        namespaceNode = NamespaceDeclaration(ParseName(namespaceGroup.Key));

                        Logging.ReportProgress($"Processed Controller: {controller.FullyQualifiedGeneratedName}");
                    }
                }
            }
        }

        var generatedPages = new List<NamespaceDeclarationSyntax>();
        foreach (var namespaceGroup in pages
                     .GroupBy(p => p.Definition?.Namespace ?? _settings.Sg4MvcNamespace)
                     .OrderBy(p => p.Key))
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
                if (!viewOnlyPageFile)
                {
                    var userPageFile = page.Definition.GetFilePath();

                    if (!File.Exists(userPageFile))
                    {
                        var result = new CodeFileBuilder(_settings, false)
                            .WithNamespace(NamespaceDeclaration(ParseName(page.Definition.Namespace))
                                .AddMembers(new ClassBuilder(page.Definition.Name)
                                    .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword)
                                    .WithComment(
                                        "// Use this file to add custom extensions and helper methods to this page")
                                    .Build()))
                            .Build();

                        _context.WriteFile(result, userPageFile);
                    }

                    var generatedFilePath = page.Definition.FullyQualifiedGeneratedName
                        .Replace('.', '_') + ".generated.cs";

                    var pageFile = new CodeFileBuilder(_settings, true)
                        .WithNamespace(namespaceNode);

                    _context.WriteFile(pageFile.Build(), generatedFilePath);

                    namespaceNode = NamespaceDeclaration(ParseName(namespaceGroup.Key));
                }
            }

            // If it's a view only page, bundle it in Sg4Mvc.cs
            if (namespaceNode.Members.Count > 0)
            {
                generatedPages.Add(namespaceNode);
            }
        }

        Logging.ReportProgress("Total Pages");

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
            .AddMembers(CreateViewOnlyControllerClasses(controllers).ToArray<MemberDeclarationSyntax>())
            .AddMembers(CreateAreaClasses(areaControllers).ToArray<MemberDeclarationSyntax>())
            .AddMembers(CreatePagePathClasses(pages, out var topLevelPagePaths).ToArray<MemberDeclarationSyntax>());

        Logging.ReportProgress($"Create {nameof(sg4Namespace)}");

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

        new CodeFileBuilder(_settings, true)
            .WithMembers(mvcStaticClass.Build())
            .WriteToFile(_context, mvcStaticClass.Name + ".cs");

        Logging.ReportProgress($"Create {nameof(mvcStaticClass)}");

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

        new CodeFileBuilder(_settings, true)
            .WithMembers(mvcPagesStaticClass.Build())
            .WriteToFile(_context, mvcPagesStaticClass.Name + ".cs");

        Logging.ReportProgress($"Create {nameof(mvcPagesStaticClass)}");

        // Generate a list of all static files from the wwwroot path
        MemberDeclarationSyntax staticFileNode;
        using (new PerformanceLogger("Total _staticFileGenerator.GenerateStaticFiles"))
        {
            staticFileNode = _staticFileGenerator.GenerateStaticFiles(workingDirectory);
        }

        var sg4MvcFile = new CodeFileBuilder(_settings, true)
            .WithMembers(
                sg4Namespace,
                staticFileNode)
            .WithNamespaces(generatedPages);

        Logging.ReportProgress($"Create {nameof(sg4MvcFile)}");

        var sg4MvcFileCompiled = sg4MvcFile.Build();

        Logging.ReportProgress($"Build {nameof(sg4MvcFile)}");

        _context.WriteFile(sg4MvcFileCompiled, Constants.Sg4MvcGeneratedFileName);

        Logging.ReportProgress($"WriteFile {nameof(sg4MvcFile)}");
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

        page.Definition = new PageDefinition(_settings.Sg4MvcNamespace,
            className,
            false,
            null,
            new List<String> { generatedPath })
        {
            FullyQualifiedGeneratedName = $"{_settings.Sg4MvcNamespace}.{className}"
        };

        var pageClass = new ClassBuilder(className)
            .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword)
            .WithBaseTypes("ISg4ActionResult");

        _pageGenerator.AddSg4ActionMethods(pageClass, page.PagePath);

        if (_settings.GeneratePageViewsClass)
        {
            _pageGenerator.WithViewsClass(pageClass, new List<PageView> { page });
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

    public List<ClassDeclarationSyntax> CreatePagePathClasses(IList<PageView> pages,
        out IDictionary<String, String> topLevelPagePaths)
    {
        if (!pages.Any())
        {
            topLevelPagePaths = null;
            return new List<ClassDeclarationSyntax>(0);
        }

        var splitter = "_";
        while (pages.Any(p => p.Segments.Any(s => s.Contains(splitter))))
        {
            splitter += "_";
        }

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

        return pathClasses.Values
            .Select(c => c.Build())
            .ToList();
    }
}
