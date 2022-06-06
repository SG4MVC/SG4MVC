using System;
using System.Diagnostics;
using AspNetSimple.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AspNetSimple.Controllers;

public partial class HomeController : Controller
{
    public virtual IActionResult Index()
    {
        Url.Action(MVC.Home.Index());

        return View();
    }

    public virtual IActionResult About()
    {
        ViewData["Message"] = "Your application description page.";

        return View();
    }

    public virtual IActionResult Contact()
    {
        ViewData["Message"] = "Your contact page.";

        return View();
    }

    public virtual IActionResult Error([Bind(Prefix = "errorCode")] Int32 statusCode = 500)
    {
        var model = new ErrorViewModel
        {
            StatusCode = statusCode,
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
        };

        return View(model);
    }

    public virtual IActionResult UsesDifferentView()
    {
        return View(MVC.Home.Views.Contact);
    }

    public virtual IActionResult ReferencesActionName(Int32 namedParam)
    {
        var x = MVC.Home.Actions.Actions.ActionNames.ReferencesActionName;

        return Ok();
    }

    public virtual IActionResult ReferencesShared()
    {
        return View(MVC.Shared.Views.Error);
    }
}
//public partial class SharedController : Controller { }
