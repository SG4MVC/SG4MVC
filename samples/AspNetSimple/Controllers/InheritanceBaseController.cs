using Microsoft.AspNetCore.Mvc;

namespace AspNetSimple.Controllers;

public abstract class InheritanceBaseController : Controller
{
    protected InheritanceBaseController(IDoesThing doesThing)
    {
        doesThing.DoThing();
    }
}
