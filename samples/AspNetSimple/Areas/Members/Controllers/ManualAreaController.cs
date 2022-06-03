using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AspNetSimple.Areas.Members.Controllers;

[Area("Members")]
public partial class ManualAreaController : Controller
{
    public virtual IActionResult Index() => throw new NotImplementedException();
    public virtual ActionResult Index2() => throw new NotImplementedException();
    public virtual JsonResult Index3() => throw new NotImplementedException();
    public virtual Task<IActionResult> AsyncIndex() => throw new NotImplementedException();
    public virtual Task<ActionResult> AsyncIndex2() => throw new NotImplementedException();
    public virtual Task<JsonResult> AsyncIndex3() => throw new NotImplementedException();
}
