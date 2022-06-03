using System;
using System.Threading.Tasks;
using AspNetSimple.Models;
using Microsoft.AspNetCore.Mvc;
using SampleModels;

namespace AspNetSimple.Controllers;

public partial class TestsController : DebugControllerBase
{
    public virtual IActionResult Index() => throw new NotImplementedException();
    public virtual ActionResult ActionResult() => throw new NotImplementedException();
    public virtual JsonResult JsonResult() => throw new NotImplementedException();
    public virtual FileResult FileResult() => throw new NotImplementedException();
    public virtual RedirectResult RedirectResult() => throw new NotImplementedException();
    public virtual RedirectToActionResult RedirectToActionResult() => throw new NotImplementedException();
    public virtual RedirectToRouteResult RedirectToRouteResult() => throw new NotImplementedException();

    public virtual Task<IActionResult> TaskIndex() => throw new NotImplementedException();
    public virtual Task<ActionResult> TaskActionResult() => throw new NotImplementedException();
    public virtual Task<JsonResult> TaskJsonResult() => throw new NotImplementedException();
    public virtual Task<FileResult> TaskFileResult() => throw new NotImplementedException();
    public virtual Task<RedirectResult> TaskRedirectResult() => throw new NotImplementedException();
    public virtual Task<RedirectToActionResult> TaskRedirectToActionResult() => throw new NotImplementedException();
    public virtual Task<RedirectToRouteResult> TaskRedirectToRouteResult() => throw new NotImplementedException();

    [RequireHttps]
    public virtual IActionResult RequiresHttps() => throw new NotImplementedException();
    [NonAction]
    public IActionResult NonAction() => throw new NotImplementedException();
    [NonAction]
    public IActionResult NonActionWithParams(Int32 id) => throw new NotImplementedException();

    [Sg4MvcExclude]
    public IActionResult Sg4MvcExcluded() => throw new NotImplementedException();

    [Sg4MvcExclude]
    public IActionResult Sg4MvcExcludedWithParams(Int32 id) => throw new NotImplementedException();

    public virtual IActionResult Parameters(Int32 id, String name) => throw new NotImplementedException();
    public virtual IActionResult ParametersWithDefault(Int32 id = 5, String name = "test") => throw new NotImplementedException();
    public virtual IActionResult PrefixedParameters([Bind(Prefix = "foo")] Int32 id, [Bind(Prefix = "far")] String name) => throw new NotImplementedException();
    public virtual IActionResult PrefixedParametersWithDefault([Bind(Prefix = "foo")] Int32 id = 5, [Bind(Prefix = "far")] String name = "test") => throw new NotImplementedException();

    public virtual Product ApiCall() => throw new NotImplementedException();
    public virtual Product ApiCallWithParams(Int32 id) => throw new NotImplementedException();
    public virtual Product ApiCallWithPrefixedParams(Int32 id) => throw new NotImplementedException();

    public virtual Task<Product> TaskApiCall() => throw new NotImplementedException();
    public virtual Task<Product> TaskApiCallWithParams(Int32 id) => throw new NotImplementedException();
    public virtual Task<Product> TaskApiCallWithPrefixedParams([Bind(Prefix = "foo")] Int32 id) => throw new NotImplementedException();

    public virtual ActionResult<Product> ApiCallTyped() => throw new NotImplementedException();
    public virtual ActionResult<Product> ApiCallTypedWithParams(Int32 id) => throw new NotImplementedException();
    public virtual ActionResult<Product> ApiCallTypedWithPrefixedParams([Bind(Prefix = "foo")] Int32 id) => throw new NotImplementedException();

    public virtual Task<ActionResult<Product>> TaskApiCallTyped() => throw new NotImplementedException();
    public virtual Task<ActionResult<Product>> TaskApiCallTypedWithParams(Int32 id) => throw new NotImplementedException();
    public virtual Task<ActionResult<Product>> TaskApiCallTypedWithPrefixedParams([Bind(Prefix = "foo")] Int32 id) => throw new NotImplementedException();

    public virtual IActionResult LocalViewModel(ErrorViewModel model) => throw new NotImplementedException();
    public virtual IActionResult ExternalViewModel(TestViewModel model) => throw new NotImplementedException();
    public virtual IActionResult PrefixedViewModel([Bind(Prefix = "viewModel")] TestViewModel model) => throw new NotImplementedException();

    public override IActionResult OverrideMe() => throw new NotImplementedException();
}
