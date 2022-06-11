using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Sg4Mvc.ActionResults;

public partial class Sg4Mvc_RazorPages_ActionResult : ActionResult, ISg4PageActionResult
{
    public Sg4Mvc_RazorPages_ActionResult(String pageName, String pageHandler, String protocol = null)
    {
        this.InitMVCT4Result(pageName, pageHandler, protocol);
    }

    public String PageName { get; set; }

    public String PageHandler { get; set; }

    public String Protocol { get; set; }

    public RouteValueDictionary RouteValueDictionary { get; set; }
}
