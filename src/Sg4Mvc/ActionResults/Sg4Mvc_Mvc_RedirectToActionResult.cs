﻿using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Sg4Mvc.ActionResults;

public partial class Sg4Mvc_Mvc_RedirectToActionResult : RedirectToActionResult, ISg4MvcActionResult
{
    public Sg4Mvc_Mvc_RedirectToActionResult(String area, String controller, String action, String protocol = null) : base(" ", " ", " ")
    {
        this.InitMVCT4Result(area, controller, action, protocol);
    }

    public String Controller { get; set; }

    public String Action { get; set; }

    public String Protocol { get; set; }

    public RouteValueDictionary RouteValueDictionary { get; set; }
}