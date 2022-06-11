using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Sg4Mvc.ActionResults;

public partial class Sg4Mvc_RazorPages_FileResult : FileResult, ISg4PageActionResult
{
    public Sg4Mvc_RazorPages_FileResult(String pageName, String pageHandler, String protocol = null) : base(null)
    {
        this.InitMVCT4Result(pageName, pageHandler, protocol);
    }

    public String PageName { get; set; }

    public String PageHandler { get; set; }

    public String Protocol { get; set; }

    public RouteValueDictionary RouteValueDictionary { get; set; }
}