using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AspNetSimple.Areas.SecondVeryLarge.Controllers
{
    [Area(nameof(SecondVeryLarge))]
    public partial class SecondController : Controller
    {
        public virtual IActionResult Index() => throw new NotImplementedException();
        public virtual ActionResult Index2() => throw new NotImplementedException();
        public virtual JsonResult Index3() => throw new NotImplementedException();
        public virtual Task<IActionResult> AsyncIndex() => throw new NotImplementedException();
        public virtual Task<ActionResult> AsyncIndex2() => throw new NotImplementedException();
        public virtual Task<JsonResult> AsyncIndex3() => throw new NotImplementedException();
        public virtual IActionResult Index(Guid guid) => throw new NotImplementedException();
        public virtual ActionResult Index2(Guid guid) => throw new NotImplementedException();
        public virtual JsonResult Index3(Guid guid) => throw new NotImplementedException();
        public virtual Task<IActionResult> AsyncIndex(Guid guid) => throw new NotImplementedException();
        public virtual Task<ActionResult> AsyncIndex2(Guid guid) => throw new NotImplementedException();
        public virtual Task<JsonResult> AsyncIndex3(Guid guid) => throw new NotImplementedException();
        public virtual IActionResult Index(Int32 num) => throw new NotImplementedException();
        public virtual ActionResult Index2(Int32 num) => throw new NotImplementedException();
        public virtual JsonResult Index3(Int32 num) => throw new NotImplementedException();
        public virtual Task<IActionResult> AsyncIndex(Int32 num) => throw new NotImplementedException();
        public virtual Task<ActionResult> AsyncIndex2(Int32 num) => throw new NotImplementedException();
        public virtual Task<JsonResult> AsyncIndex3(Int32 num) => throw new NotImplementedException();
        public virtual IActionResult Index(String str) => throw new NotImplementedException();
        public virtual ActionResult Index2(String str) => throw new NotImplementedException();
        public virtual JsonResult Index3(String str) => throw new NotImplementedException();
        public virtual Task<IActionResult> AsyncIndex(String str) => throw new NotImplementedException();
        public virtual Task<ActionResult> AsyncIndex2(String str) => throw new NotImplementedException();
        public virtual Task<JsonResult> AsyncIndex3(String str) => throw new NotImplementedException();
    }
}
