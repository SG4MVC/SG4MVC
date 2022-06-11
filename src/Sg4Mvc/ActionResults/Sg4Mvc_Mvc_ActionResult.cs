using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Sg4Mvc.ActionResults;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

public partial class Sg4Mvc_Mvc_ActionResult : ActionResult, ISg4MvcActionResult
{
    public Sg4Mvc_Mvc_ActionResult(String area, String controller, String action, String protocol = null)
    {
        this.InitMVCT4Result(area, controller, action, protocol);
    }

    public String Controller { get; set; }

    public String Action { get; set; }

    public String Protocol { get; set; }

    public RouteValueDictionary RouteValueDictionary { get; set; }
}
