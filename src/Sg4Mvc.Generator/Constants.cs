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

    private const String ActionResultNamespace = ProjectName + "_Mvc_";

    internal const String ActionResultClass = ActionResultNamespace + "ActionResult";
    internal const String JsonResultClass = ActionResultNamespace + "JsonResult";
    internal const String ContentResultClass = ActionResultNamespace + "ContentResult";
    internal const String FileResultClass = ActionResultNamespace + "FileResult";
    internal const String RedirectResultClass = ActionResultNamespace + "RedirectResult";
    internal const String RedirectToActionResultClass = ActionResultNamespace + "RedirectToActionResult";
    internal const String RedirectToRouteResultClass = ActionResultNamespace + "RedirectToRouteResult";

    private const String PageActionResultNamespace = ProjectName + "_RazorPages_";

    internal const String PageActionResultClass = PageActionResultNamespace + "ActionResult";
    internal const String PageJsonResultClass = PageActionResultNamespace + "JsonResult";
    internal const String PageContentResultClass = PageActionResultNamespace + "ContentResult";
    internal const String PageFileResultClass = PageActionResultNamespace + "FileResult";
    internal const String PageRedirectResultClass = PageActionResultNamespace + "RedirectResult";
    internal const String PageRedirectToActionResultClass = PageActionResultNamespace + "RedirectToActionResult";
    internal const String PageRedirectToRouteResultClass = PageActionResultNamespace + "RedirectToRouteResult";
}
