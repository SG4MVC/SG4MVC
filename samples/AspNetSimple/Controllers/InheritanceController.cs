using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace AspNetSimple.Controllers;

public partial class InheritanceController(
    IDoesThing doesThing)
    : InheritanceBaseController(doesThing)
{
    public virtual IActionResult Index()
    {
        Url.Action(MVC.Inheritance.Index());

        // We need to use the parameters
        doesThing.DoThing();

        return View();
    }
}
