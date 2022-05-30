using System;
using System.Threading.Tasks;
using AspNetSimple.Models;
using Microsoft.AspNetCore.Mvc;

namespace AspNetSimple.Controllers
{
    public partial class WebApiController : ControllerBase
    {
        public virtual Product ApiCall() => throw new NotImplementedException();
        public virtual Product ApiCallWithParams(Int32 id) => throw new NotImplementedException();

        public virtual Task<Product> TaskApiCall() => throw new NotImplementedException();
        public virtual Task<Product> TaskApiCallWithParams(Int32 id) => throw new NotImplementedException();

        public virtual ActionResult<Product> ApiCallTyped() => throw new NotImplementedException();
        public virtual ActionResult<Product> ApiCallTypedWithParams(Int32 id) => throw new NotImplementedException();
        public virtual ActionResult<Product> ApiCallTypedWithPrefixedParams([Bind(Prefix = "foo")] Int32 id) => throw new NotImplementedException();

        public virtual Task<ActionResult<Product>> TaskApiCallTyped() => throw new NotImplementedException();
        public virtual Task<ActionResult<Product>> TaskApiCallTypedWithParams(Int32 id) => throw new NotImplementedException();
        public virtual Task<ActionResult<Product>> TaskApiCallTypedWithPrefixedParams([Bind(Prefix = "foo")] Int32 id) => throw new NotImplementedException();
    }
}
