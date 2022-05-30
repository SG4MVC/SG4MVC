using System;
using System.Threading.Tasks;
#if !CORE1
using Microsoft.AspNetCore.Mvc.Infrastructure;
#endif

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    public static class UrlHelperExtensions
    {
        public static String Action(this IUrlHelper urlHelper, IActionResult result, String protocol = null, String hostName = null, String fragment = null)
        {
            return urlHelper.Action(result.GetSg4ActionResult(), protocol, hostName, fragment);
        }

        public static String Action<TAction>(this IUrlHelper urlHelper, Task<TAction> taskResult, String protocol = null, String hostName = null, String fragment = null)
            where TAction : IActionResult
        {
            return urlHelper.Action(taskResult.Result, protocol, hostName, fragment);
        }

        public static String Action(this IUrlHelper urlHelper, Task taskResult, String protocol = null, String hostName = null, String fragment = null)
        {
            return urlHelper.Action(taskResult.GetActionResult(), protocol, hostName, fragment);
        }

        public static String Action(this IUrlHelper urlHelper, ISg4ActionResult result, String protocol = null, String hostName = null, String fragment = null)
        {
            return urlHelper.RouteUrl(null, result, protocol, hostName, fragment);
        }

#if !CORE1
        public static String Action(this IUrlHelper urlHelper, IConvertToActionResult result, String protocol = null, String hostName = null, String fragment = null)
        {
            return urlHelper.Action(result.Convert(), protocol, hostName, fragment);
        }
#endif

        public static String ActionAbsolute(this IUrlHelper urlHelper, IActionResult result)
        {
            return urlHelper.ActionAbsolute(result.GetSg4ActionResult());
        }

        public static String ActionAbsolute<TAction>(this IUrlHelper urlHelper, Task<TAction> taskResult)
            where TAction : IActionResult
        {
            return urlHelper.ActionAbsolute(taskResult.Result);
        }

        public static String ActionAbsolute(this IUrlHelper urlHelper, Task taskResult)
        {
            return urlHelper.ActionAbsolute(taskResult.GetActionResult());
        }

        public static String ActionAbsolute(this IUrlHelper urlHelper, ISg4ActionResult result)
        {
            var request = urlHelper.ActionContext.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}{urlHelper.RouteUrl(result.RouteValueDictionary)}";
        }

#if !CORE1
        public static String ActionAbsolute(this IUrlHelper urlHelper, IConvertToActionResult result)
        {
            return urlHelper.ActionAbsolute(result.Convert());
        }
#endif

        public static String RouteUrl(this IUrlHelper urlHelper, IActionResult result)
        {
            return urlHelper.RouteUrl(null, result, null, null);
        }

        public static String RouteUrl(this IUrlHelper urlHelper, String routeName, IActionResult result, String protocol = null, String hostName = null, String fragment = null)
        {
            return urlHelper.RouteUrl(routeName, result.GetSg4ActionResult(), protocol, hostName, fragment);
        }

        public static String RouteUrl<TAction>(this IUrlHelper urlHelper, Task<TAction> taskResult)
            where TAction : IActionResult
        {
            return urlHelper.RouteUrl(null, taskResult.Result, null, null);
        }

        public static String RouteUrl<TAction>(this IUrlHelper urlHelper, String routeName, Task<TAction> taskResult, String protocol = null, String hostName = null, String fragment = null)
            where TAction : IActionResult
        {
            return urlHelper.RouteUrl(routeName, taskResult.Result, protocol, hostName, fragment);
        }

        public static String RouteUrl<TAction>(this IUrlHelper urlHelper, Task taskResult)
        {
            return urlHelper.RouteUrl(null, taskResult.GetActionResult(), null, null);
        }

        public static String RouteUrl<TAction>(this IUrlHelper urlHelper, String routeName, Task taskResult, String protocol = null, String hostName = null, String fragment = null)
        {
            return urlHelper.RouteUrl(routeName, taskResult.GetActionResult(), protocol, hostName, fragment);
        }

        public static String RouteUrl(this IUrlHelper urlHelper, String routeName, ISg4ActionResult result, String protocol = null, String hostName = null, String fragment = null)
        {
            return urlHelper.RouteUrl(routeName, result.RouteValueDictionary, protocol ?? result.Protocol, hostName, fragment);
        }

#if !CORE1
        public static String RouteUrl<TAction>(this IUrlHelper urlHelper, IConvertToActionResult result)
        {
            return urlHelper.RouteUrl(null, result.Convert(), null, null);
        }

        public static String RouteUrl<TAction>(this IUrlHelper urlHelper, String routeName, IConvertToActionResult result, String protocol = null, String hostName = null, String fragment = null)
        {
            return urlHelper.RouteUrl(routeName, result.Convert(), protocol, hostName, fragment);
        }
#endif
    }
}
