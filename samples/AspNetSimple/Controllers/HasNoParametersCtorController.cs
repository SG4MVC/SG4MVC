using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetSimple.Controllers;

[Authorize]
public partial class HasNoParametersCtorController : Controller
{
    // Declares explicit parameterless ctor
    public HasNoParametersCtorController()
    {
        
    }

    public virtual IActionResult Index() => throw new NotImplementedException();
}
