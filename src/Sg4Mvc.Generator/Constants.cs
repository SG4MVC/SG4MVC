using System;

namespace Sg4Mvc.Generator;

internal static class Constants
{
    internal const String ProjectName = "Sg4Mvc";
    internal const String Version = "1.0";

    internal const String Sg4MvcGeneratedFileName = "Sg4Mvc.generated.cs";

    internal const String DummyClass = "Dummy";
    internal const String DummyClassInstance = "Instance";

    internal const String Sg4MvcHelpersClass = "Sg4MvcHelpers";
    internal const String Sg4MvcHelpers_ProcessVirtualPath = "ProcessVirtualPath";

    private const String ActionResultNamespace = "_Microsoft_AspNetCore_Mvc_";
    internal const String ActionResultClass = ProjectName + ActionResultNamespace + "ActionResult";
    internal const String JsonResultClass = ProjectName + ActionResultNamespace + "JsonResult";
    internal const String ContentResultClass = ProjectName + ActionResultNamespace + "ContentResult";
    internal const String FileResultClass = ProjectName + ActionResultNamespace + "FileResult";
    internal const String RedirectResultClass = ProjectName + ActionResultNamespace + "RedirectResult";
    internal const String RedirectToActionResultClass = ProjectName + ActionResultNamespace + "RedirectToActionResult";
    internal const String RedirectToRouteResultClass = ProjectName + ActionResultNamespace + "RedirectToRouteResult";

    private const String PageActionResultNamespace = "_Microsoft_AspNetCore_Mvc_RazorPages_";
    internal const String PageActionResultClass = ProjectName + PageActionResultNamespace + "ActionResult";
    internal const String PageJsonResultClass = ProjectName + PageActionResultNamespace + "JsonResult";
    internal const String PageContentResultClass = ProjectName + PageActionResultNamespace + "ContentResult";
    internal const String PageFileResultClass = ProjectName + PageActionResultNamespace + "FileResult";
    internal const String PageRedirectResultClass = ProjectName + PageActionResultNamespace + "RedirectResult";
    internal const String PageRedirectToActionResultClass = ProjectName + PageActionResultNamespace + "RedirectToActionResult";
    internal const String PageRedirectToRouteResultClass = ProjectName + PageActionResultNamespace + "RedirectToRouteResult";
}
