using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AspNetSimple.Controllers;

public partial class HasParametersCtorController : Controller
{
    // Declares explicit ctor with parameters
    public HasParametersCtorController(ILogger logger)
    {
        var x = logger;
    }

    public virtual IActionResult Index() => throw new NotImplementedException();
}
