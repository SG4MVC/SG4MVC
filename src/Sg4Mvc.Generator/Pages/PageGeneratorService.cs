using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sg4Mvc.Generator.CodeGen;
using Sg4Mvc.Generator.Extensions;
using Sg4Mvc.Generator.Pages.Interfaces;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Sg4Mvc.Generator.Pages;

public class PageGeneratorService : IPageGeneratorService
{
    private const String ViewNamesClassName = "_ViewNamesClass";

    private readonly Settings _settings;

    public PageGeneratorService(Settings settings)
    {
        _settings = settings;
    }

    public ClassDeclarationSyntax GeneratePartialPage(PageView pageView)
    {
        var page = pageView.Definition;

        // build controller partial class node
        var genControllerClass = new ClassBuilder(page.Symbol.Name)               // public partial {controllerClass} : ISg4ActionResult
            .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword)
            .WithTypeParameters(page.Symbol.TypeParameters.Select(tp => tp.Name).ToArray()) // optional <T1, T2, �>
            .WithBaseTypes("ISg4ActionResult");

        // add a default constructor if there are some but none are zero length
        var gotCustomConstructors = page.Symbol.Constructors
            .Where(c => c.DeclaredAccessibility == Accessibility.Public)
            .Where(SyntaxNodeHelpers.IsNotSg4MvcGenerated)
            .Where(c => !c.IsImplicitlyDeclared)
            .Any();
        if (!gotCustomConstructors)
        /* [GeneratedCode, DebuggerNonUserCode]
         * public ctor() { }
         */
        {
            genControllerClass.WithConstructor(c => c
                .WithModifiers(SyntaxKind.PublicKeyword)
                .WithGeneratedNonUserCodeAttributes());
        }

        /* [GeneratedCode, DebuggerNonUserCode]
         * public ctor(Dummy d) {}
         */
        genControllerClass.WithConstructor(c => c
            .WithModifiers(SyntaxKind.ProtectedKeyword)
            .WithGeneratedNonUserCodeAttributes()
            .WithParameter("d", Constants.DummyClass));

        AddRedirectMethods(genControllerClass);
        AddSg4ActionMethods(genControllerClass, pageView.PagePath);
        AddParameterlessMethods(genControllerClass, page.Symbol, page.IsSecure);

        //var actionsExpression = _settings.HelpersPrefix + "." + page.Name;
        var handlerMethods = SyntaxNodeHelpers.GetPublicNonGeneratedPageMethods(page.Symbol).ToArray();
        var handlerNames = handlerMethods.Select(m => m.Name)
            .Select(GetHandler)
            .Where(n => !String.IsNullOrWhiteSpace(n))
            .Distinct()
            .ToArray();
        genControllerClass
            //.WithExpressionProperty("Actions", page.Symbol.Name, actionsExpression, SyntaxKind.PublicKeyword)
            .WithStringField("Name", pageView.PagePath, SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword)
            .WithStringField("NameConst", pageView.PagePath, SyntaxKind.PublicKeyword, SyntaxKind.ConstKeyword)
            .WithStaticFieldBackedProperty("HandlerNames", "HandlerNamesClass", SyntaxKind.PublicKeyword)
            /* [GeneratedCode, DebuggerNonUserCode]
             * public class ActionNamesClass
             * {
             *  public readonly string {action} = "{action}";
             * }
             */
            .WithChildClass("HandlerNamesClass", ac => ac
                .WithModifiers(SyntaxKind.PublicKeyword)
                .WithGeneratedNonUserCodeAttributes()
                .ForEach(handlerNames, (c, m) => c
                    .WithStringField(m, m, SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword)))
            /* [GeneratedCode, DebuggerNonUserCode]
             * public class ActionNameConstants
             * {
             *  public const string {action} = "{action}";
             * }
             */
            .WithChildClass("HandlerNameConstants", ac => ac
                .WithModifiers(SyntaxKind.PublicKeyword)
                .WithGeneratedNonUserCodeAttributes()
                .ForEach(handlerNames, (c, m) => c
                    .WithStringField(m, m, SyntaxKind.PublicKeyword, SyntaxKind.ConstKeyword)));

        /* [GeneratedCode]
         * static readonly HandlerParamsClass_OnPost s_OnPostParams = new HandlerParamsClass_OnPost();
         * [GeneratedCode, DebuggerNonUserCode]
         * public HandlerParamsClass_OnPost OnPostParams => s_OnPostParams;
         * [GeneratedCode, DebuggerNonUserCode]
         * public class HandlerParamsClass_OnPost
         * {
         *  public readonly string param1 = "param1";
         *  public readonly string param2 = "param2";
         * }
         */
        if (_settings.GenerateParamsForActionMethods)
        {
            genControllerClass
                .ForEach(handlerMethods.Where(m => m.Parameters.Any()), (c, m) => c
                    .WithStaticFieldBackedProperty(m.Name + _settings.ParamsPropertySuffix, $"HandlerParamsClass_{m.Name}", SyntaxKind.PublicKeyword)
                    .WithChildClass($"HandlerParamsClass_{m.Name}", ac => ac
                        .WithModifiers(SyntaxKind.PublicKeyword)
                        .WithGeneratedNonUserCodeAttributes()
                        .ForEach(m.Parameters, (c, p) => c
                            .WithStringField(p.Name, p.GetRouteName(), SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword))));
        }

        if (_settings.GeneratePageViewsClass)
        {
            WithViewsClass(genControllerClass, new List<PageView> { pageView });
        }

        return genControllerClass.Build();
    }

    private static String GetHandler(String action)
    {
        var name = action.Substring(2); // trimming "On"
        if (name.EndsWith("Async"))
        {
            name = name.Substring(0, name.Length - "Async".Length);
        }

        if (name.StartsWith("Get"))
        {
            name = name.Substring(3);
        }
        else if (name.StartsWith("Post"))
        {
            name = name.Substring(4);
        }
        else if (name.StartsWith("Delete"))
        {
            name = name.Substring(6);
        }
        else if (name.StartsWith("Put"))
        {
            name = name.Substring(3);
        }

        if (name.Length == 0)
        {
            return null;
        }

        return name;
    }

    public ClassDeclarationSyntax GenerateSg4Page(PageDefinition page)
    {
        var className = GetSg4MvcControllerClassName(page.Symbol);
        page.FullyQualifiedSg4ClassName = $"{page.Namespace}.{className}";

        /* [GeneratedCode, DebuggerNonUserCode]
         * public partial class Sg4Mvc_{Controller} : {Controller}
         * {
         *  public Sg4Mvc_{Controller}() : base(Dummy.Instance) {}
         * }
         */
        var sg4ControllerClass = new ClassBuilder(className)
            .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword)
            .WithGeneratedNonUserCodeAttributes()
            .WithBaseTypes(page.Symbol.ContainingNamespace + "." + page.Symbol.Name)
            .WithConstructor(c => c
                .WithBaseConstructorCall(IdentifierName(Constants.DummyClass + "." + Constants.DummyClassInstance))
                .WithModifiers(SyntaxKind.PublicKeyword));
        AddMethodOverrides(sg4ControllerClass, page.Symbol, page.IsSecure);
        return sg4ControllerClass.Build();
    }

    private void AddRedirectMethods(ClassBuilder genControllerClass)
    {
        genControllerClass
            /* [GeneratedCode, DebuggerNonUserCode]
             * protected RedirectToRouteResult RedirectToAction(IActionResult result)
             * {
             *  var callInfo = result.GetSg4ActionResult();
             *  return RedirectToRoute(callInfo.RouteValueDictionary);
             * }
             */
            .WithMethod("RedirectToAction", "RedirectToRouteResult", m => m
                .WithModifiers(SyntaxKind.ProtectedKeyword)
                .WithGeneratedNonUserCodeAttributes()
                .WithParameter("result", "IActionResult")
                .WithBody(b => b
                    .VariableFromMethodCall("callInfo", "result", "GetSg4ActionResult")
                    .ReturnMethodCall(null, "RedirectToRoute", "callInfo.RouteValueDictionary")))

            /* [GeneratedCode, DebuggerNonUserCode]
             * protected RedirectToRouteResult RedirectToAction(Task<IActionResult> taskResult)
             * {
             *  return RedirectToAction(taskResult.Result);
             * }
            */
            .WithMethod("RedirectToAction", "RedirectToRouteResult", m => m
                .WithModifiers(SyntaxKind.ProtectedKeyword)
                .WithGeneratedNonUserCodeAttributes()
                .WithParameter("taskResult", "Task<IActionResult>")
                .WithBody(b => b
                    .ReturnMethodCall(null, "RedirectToAction", "taskResult.Result")))

            /* [GeneratedCode, DebuggerNonUserCode]
             * protected RedirectToRouteResult RedirectToActionPermanent(IActionResult result)
             * {
             *  var callInfo = result.GetSg4ActionResult();
             *  return RedirectToRoutePermanent(callInfo.RouteValueDictionary);
             * }
             */
            .WithMethod("RedirectToActionPermanent", "RedirectToRouteResult", m => m
                .WithModifiers(SyntaxKind.ProtectedKeyword)
                .WithGeneratedNonUserCodeAttributes()
                .WithParameter("result", "IActionResult")
                .WithBody(b => b
                    .VariableFromMethodCall("callInfo", "result", "GetSg4ActionResult")
                    .ReturnMethodCall(null, "RedirectToRoutePermanent", "callInfo.RouteValueDictionary")))

            /* [GeneratedCode, DebuggerNonUserCode]
             * protected RedirectToRouteResult RedirectToActionPermanent(Task<IActionResult> taskResult)
             * {
             *  return RedirectToActionPermanent(taskResult.Result);
             * }
            */
            .WithMethod("RedirectToActionPermanent", "RedirectToRouteResult", m => m
                .WithModifiers(SyntaxKind.ProtectedKeyword)
                .WithGeneratedNonUserCodeAttributes()
                .WithParameter("taskResult", "Task<IActionResult>")
                .WithBody(b => b
                    .ReturnMethodCall(null, "RedirectToActionPermanent", "taskResult.Result")))

            /* [GeneratedCode, DebuggerNonUserCode]
             * protected RedirectToRouteResult RedirectToPage(IActionResult result)
             * {
             *  var callInfo = result.GetSg4ActionResult();
             *  return RedirectToRoute(callInfo.RouteValueDictionary);
             * }
             */
            .WithMethod("RedirectToPage", "RedirectToRouteResult", m => m
                .WithModifiers(SyntaxKind.ProtectedKeyword)
                .WithGeneratedNonUserCodeAttributes()
                .WithParameter("result", "IActionResult")
                .WithBody(b => b
                    .VariableFromMethodCall("callInfo", "result", "GetSg4ActionResult")
                    .ReturnMethodCall(null, "RedirectToRoute", "callInfo.RouteValueDictionary")))

            /* [GeneratedCode, DebuggerNonUserCode]
             * protected RedirectToRouteResult RedirectToPage(Task<IActionResult> taskResult)
             * {
             *  return RedirectToPage(taskResult.Result);
             * }
            */
            .WithMethod("RedirectToPage", "RedirectToRouteResult", m => m
                .WithModifiers(SyntaxKind.ProtectedKeyword)
                .WithGeneratedNonUserCodeAttributes()
                .WithParameter("taskResult", "Task<IActionResult>")
                .WithBody(b => b
                    .ReturnMethodCall(null, "RedirectToPage", "taskResult.Result")))

            /* [GeneratedCode, DebuggerNonUserCode]
             * protected RedirectToRouteResult RedirectToPagePermanent(IActionResult result)
             * {
             *  var callInfo = result.GetSg4ActionResult();
             *  return RedirectToRoutePermanent(callInfo.RouteValueDictionary);
             * }
             */
            .WithMethod("RedirectToPagePermanent", "RedirectToRouteResult", m => m
                .WithModifiers(SyntaxKind.ProtectedKeyword)
                .WithGeneratedNonUserCodeAttributes()
                .WithParameter("result", "IActionResult")
                .WithBody(b => b
                    .VariableFromMethodCall("callInfo", "result", "GetSg4ActionResult")
                    .ReturnMethodCall(null, "RedirectToRoutePermanent", "callInfo.RouteValueDictionary")))

            /* [GeneratedCode, DebuggerNonUserCode]
             * protected RedirectToRouteResult RedirectToPagePermanent(Task<IActionResult> taskResult)
             * {
             *  return RedirectToPagePermanent(taskResult.Result);
             * }
            */
            .WithMethod("RedirectToPagePermanent", "RedirectToRouteResult", m => m
                .WithModifiers(SyntaxKind.ProtectedKeyword)
                .WithGeneratedNonUserCodeAttributes()
                .WithParameter("taskResult", "Task<IActionResult>")
                .WithBody(b => b
                    .ReturnMethodCall(null, "RedirectToPagePermanent", "taskResult.Result")));
    }

    public void AddSg4ActionMethods(ClassBuilder genControllerClass, String pagePath)
    {
        var routeField = "m_RouteValueDictionary";
        var routeValues = new Dictionary<String, Object>
        {
            ["Page"] = pagePath,
        };

        genControllerClass = genControllerClass
            .WithExpressionProperty("ISg4ActionResult.Protocol", "string", null)
            .WithRouteValueField(routeField, routeValues)
            .WithExpressionProperty("ISg4ActionResult.RouteValueDictionary", "RouteValueDictionary", routeField);
    }

    private void AddParameterlessMethods(ClassBuilder genControllerClass, ITypeSymbol mvcSymbol, Boolean isControllerSecure)
    {
        var methods = mvcSymbol.GetPublicNonGeneratedControllerMethods()
            .GroupBy(m => m.Name)
            .Where(g => !g.Any(m => m.Parameters.Length == 0));
        foreach (var method in methods)
        {
            var handlerKey = GetHandler(method.Key);
            if (handlerKey != null)
            {
                handlerKey = "HandlerNames." + handlerKey;
            }
            else
            {
                handlerKey = "null";
            }

            genControllerClass
                /* [GeneratedCode, DebuggerNonUserCode]
                 * public virtual IActionResult {method.Key}()
                 * {
                 *  return new Sg4Mvc_RazorPages_ActionResult(Name, HandlerNames.{Handler});
                 * }
                 */
                .WithMethod(method.Key, "IActionResult", m => m
                    .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.VirtualKeyword)
                    .WithNonHandlerAttribute()
                    .WithGeneratedNonUserCodeAttributes()
                    .WithBody(b => b
                        .ReturnNewObject(Constants.PageActionResultClass,
                            isControllerSecure || method.Any(mg => mg.GetAttributes().Any(a => a.AttributeClass.InheritsFrom(FullTypeNames.RequireHttpsAttribute)))
                                ? new Object[] { "Name", handlerKey, SimpleLiteral.String("https") }
                                : new Object[] { "Name", handlerKey }
                        )));
        }
    }

    private void AddMethodOverrides(ClassBuilder classBuilder, ITypeSymbol mvcSymbol, Boolean isControllerSecure)
    {
        const String overrideMethodSuffix = "Override";
        foreach (var method in mvcSymbol.GetPublicNonGeneratedControllerMethods())
        {
            var methodReturnType = method.ReturnType;
            Boolean isTaskResult = false, isGenericTaskResult = false;
            if (methodReturnType.InheritsFrom<Task>())
            {
                isTaskResult = true;
                var taskReturnType = methodReturnType as INamedTypeSymbol;
                if (taskReturnType.TypeArguments.Length > 0)
                {
                    methodReturnType = taskReturnType.TypeArguments[0];
                    isGenericTaskResult = true;
                }
            }

            var callInfoType = Constants.PageActionResultClass;
            if (methodReturnType.InheritsFrom(FullTypeNames.JsonResult))
            {
                callInfoType = Constants.PageJsonResultClass;
            }
            else if (methodReturnType.InheritsFrom(FullTypeNames.ContentResult))
            {
                callInfoType = Constants.PageContentResultClass;
            }
            else if (methodReturnType.InheritsFrom(FullTypeNames.FileResult))
            {
                callInfoType = Constants.PageFileResultClass;
            }
            else if (methodReturnType.InheritsFrom(FullTypeNames.RedirectResult))
            {
                callInfoType = Constants.PageRedirectResultClass;
            }
            else if (methodReturnType.InheritsFrom(FullTypeNames.RedirectToActionResult))
            {
                callInfoType = Constants.PageRedirectToActionResultClass;
            }
            else if (methodReturnType.InheritsFrom(FullTypeNames.RedirectToRouteResult))
            {
                callInfoType = Constants.PageRedirectToRouteResultClass;
            }
            else if (methodReturnType.InheritsFrom(FullTypeNames.IConvertToActionResult))
            {
                callInfoType = Constants.PageActionResultClass;
            }
            else if ((!isTaskResult || isGenericTaskResult) && !methodReturnType.InheritsFrom(FullTypeNames.IActionResult))
            {
                // Not a return type we support right now. Returning
                continue;
            }

            var handlerKey = GetHandler(method.Name);
            if (handlerKey != null)
            {
                handlerKey = "HandlerNames." + handlerKey;
            }
            else
            {
                handlerKey = "null";
            }

            classBuilder
                /* [NonHandler]
                 * partial void {action}Override({ActionResultType} callInfo, [� params]);
                 */
                .WithMethod(method.Name + overrideMethodSuffix, null, m => m
                    .WithModifiers(SyntaxKind.PartialKeyword)
                    .WithNonHandlerAttribute()
                    .WithParameter("callInfo", callInfoType)
                    .ForEach(method.Parameters, (m2, p) => m2
                        .WithParameter(p.Name, p.Type.ToString()))
                    .WithNoBody())
                /* [NonHandler]
                 * public overrive {ActionResultType} {action}([� params])
                 * {
                 *  var callInfo = new Sg4Mvc_RazorPages_ActionResult(Name, HandlerNames.{Handler});
                 *  ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "paramName", paramName);
                 *  {Action}Override(callInfo, {parameters});
                 *  return callInfo;
                 * }
                 */
                .WithMethod(method.Name, method.ReturnType.ToString(), m => m
                    .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.OverrideKeyword)
                    .WithNonHandlerAttribute()
                    .ForEach(method.Parameters, (m2, p) => m2
                        .WithParameter(p.Name, p.Type.ToString()))
                    .WithBody(b => b
                        .VariableFromNewObject("callInfo", callInfoType,
                            isControllerSecure || method.GetAttributes().Any(a => a.AttributeClass.InheritsFrom(FullTypeNames.RequireHttpsAttribute))
                                ? new Object[] { "Name", handlerKey, SimpleLiteral.String("https") }
                                : new Object[] { "Name", handlerKey }
                        )
                        .ForEach(method.Parameters, (cb, p) => cb
                            .MethodCall("ModelUnbinderHelpers", "AddRouteValues", "callInfo.RouteValueDictionary", SimpleLiteral.String(p.GetRouteName()), p.Name))
                        .MethodCall(null, method.Name + overrideMethodSuffix, new[] { "callInfo" }.Concat(method.Parameters.Select(p => p.Name)).ToArray())
                        .Statement(rb => isTaskResult
                            ? rb.ReturnMethodCall(typeof(Task).FullName, "FromResult" + (isGenericTaskResult ? "<" + methodReturnType + ">" : null), "callInfo")
                            : rb.ReturnVariable("callInfo"))
                    ));
        }
    }

    internal static String GetSg4MvcControllerClassName(INamedTypeSymbol controllerClass)
    {
        return $"Sg4Mvc_{controllerClass.Name}";
    }

    public ClassBuilder WithViewsClass(ClassBuilder classBuilder, List<PageView> viewFiles)
    {
        var viewEditorTemplates = viewFiles.Where(c => c.TemplateKind == "EditorTemplates" || c.TemplateKind == "DisplayTemplates");
        var subpathViews = viewFiles.Where(c => c.TemplateKind != null && c.TemplateKind != "EditorTemplates" && c.TemplateKind != "DisplayTemplates")
            .OrderBy(v => v.TemplateKind);

        /* public class ViewsClass
         * {
         * [...] */
        classBuilder.WithChildClass("ViewsClass", cb => cb
            .WithModifiers(SyntaxKind.PublicKeyword)
            .WithGeneratedNonUserCodeAttributes()
            // static readonly _ViewNamesClass s_ViewNames = new _ViewNamesClass();
            // public _ViewNamesClass ViewNames => s_ViewNames;
            .WithStaticFieldBackedProperty("ViewNames", ViewNamesClassName, SyntaxKind.PublicKeyword)
            /* public class _ViewNamesClass
             * {
             *  public readonly string {view} = "{view}";
             * }
             */
            .WithChildClass(ViewNamesClassName, vnc => vnc
                .WithModifiers(SyntaxKind.PublicKeyword)
                .ForEach(viewFiles.Where(c => c.TemplateKind == null), (vc, v) => vc
                    .WithStringField(v.Name.SanitiseFieldName(), v.Name, SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword)))
            .ForEach(viewFiles.Where(c => c.TemplateKind == null), (c, v) => c
                // public readonly string {view} = "~/Views/{controller}/{view}.cshtml";
                .WithStringField(v.Name.SanitiseFieldName(), v.RelativePath.ToString(), SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword))
            .ForEach(viewEditorTemplates.GroupBy(v => v.TemplateKind), (c, g) => c
                // static readonly _DisplayTemplatesClass s_DisplayTemplates = new _DisplayTemplatesClass();
                // public _DisplayTemplatesClass DisplayTemplates => s_DisplayTemplates;
                .WithStaticFieldBackedProperty(g.Key, $"_{g.Key}Class", SyntaxKind.PublicKeyword)
                /* public partial _DisplayTemplatesClass
                 * {
                 *  public readonly string {view} = "{view}";
                 * }
                 */
                .WithChildClass($"_{g.Key}Class", tc => tc
                    .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword)
                    .ForEach(g, (tcc, v) => tcc
                        .WithStringField(v.Name.SanitiseFieldName(), v.Name, SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword))))
            .ForEach(subpathViews.GroupBy(v => v.TemplateKind), (c, g) => c
                // static readonly _{viewFolder}Class s_{viewFolder} = new _{viewFolder}Class();
                // public _{viewFolder}Class {viewFolder} => s_{viewFolder};
                .WithStaticFieldBackedProperty(g.Key, $"_{g.Key}Class", SyntaxKind.PublicKeyword)
                /* public class _{viewFolder}Class
                 * {
                 * [...] */
                .WithChildClass($"_{g.Key}Class", tc => tc
                    .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword)
                    // static readonly _ViewNamesClass s_ViewNames = new _ViewNamesClass();
                    // public _ViewNamesClass ViewNames => s_ViewNames;
                    .WithStaticFieldBackedProperty("ViewNames", ViewNamesClassName, SyntaxKind.PublicKeyword)
                    /* public class _ViewNamesClass
                     * {
                     *  public readonly string {view} = "{view}";
                     * }
                     */
                    .WithChildClass(ViewNamesClassName, vnc => vnc
                        .WithModifiers(SyntaxKind.PublicKeyword)
                        .ForEach(g, (vc, v) => vc
                            .WithStringField(v.Name.SanitiseFieldName(), v.Name, SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword)))
                    .ForEach(g, (vc, v) => vc
                        // public string {view} = "~/Views/{controller}/{viewFolder}/{view}.cshtml";
                        .WithStringField(v.Name.SanitiseFieldName(), v.RelativePath.ToString(), SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword)))));

        return classBuilder
            .WithStaticFieldBackedProperty("Views", "ViewsClass", SyntaxKind.PublicKeyword);
    }
}
