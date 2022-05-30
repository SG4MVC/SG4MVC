using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
#if !CORE1
using Microsoft.AspNetCore.Mvc.Infrastructure;
#endif

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    public static class HtmlHelperExtensions
    {
        #region ActionLink
        public static IHtmlContent ActionLink(this IHtmlHelper htmlHelper, String linkText, IActionResult result, Object htmlAttributes = null, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.ActionLink(linkText, result.GetSg4ActionResult(), htmlAttributes, protocol, hostName, fragment);
        }

        public static IHtmlContent ActionLink(this IHtmlHelper htmlHelper, String linkText, IActionResult result, IDictionary<String, Object> htmlAttributes, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.ActionLink(linkText, result.GetSg4ActionResult(), htmlAttributes, protocol, hostName, fragment);
        }

        public static IHtmlContent ActionLink<TAction>(this IHtmlHelper htmlHelper, String linkText, Task<TAction> result, Object htmlAttributes = null, String protocol = null, String hostName = null, String fragment = null)
            where TAction : IActionResult
        {
            return htmlHelper.ActionLink(linkText, result.Result, htmlAttributes, protocol, hostName, fragment);
        }

        public static IHtmlContent ActionLink<TAction>(this IHtmlHelper htmlHelper, String linkText, Task<TAction> result, IDictionary<String, Object> htmlAttributes, String protocol = null, String hostName = null, String fragment = null)
            where TAction : IActionResult
        {
            return htmlHelper.ActionLink(linkText, result.Result, htmlAttributes, protocol, hostName, fragment);
        }

        public static IHtmlContent ActionLink(this IHtmlHelper htmlHelper, String linkText, Task result, Object htmlAttributes = null, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.ActionLink(linkText, result.GetActionResult(), htmlAttributes, protocol, hostName, fragment);
        }

        public static IHtmlContent ActionLink(this IHtmlHelper htmlHelper, String linkText, Task result, IDictionary<String, Object> htmlAttributes, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.ActionLink(linkText, result.GetActionResult(), htmlAttributes, protocol, hostName, fragment);
        }

        public static IHtmlContent ActionLink(this IHtmlHelper htmlHelper, String linkText, ISg4ActionResult result, Object htmlAttributes = null, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.RouteLink(linkText, null, result, htmlAttributes, protocol, hostName, fragment);
        }

        public static IHtmlContent ActionLink(this IHtmlHelper htmlHelper, String linkText, ISg4ActionResult result, IDictionary<String, Object> htmlAttributes, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.RouteLink(linkText, null, result, htmlAttributes, protocol, hostName, fragment);
        }

#if !CORE1
        public static IHtmlContent ActionLink(this IHtmlHelper htmlHelper, String linkText, IConvertToActionResult result, Object htmlAttributes = null, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.ActionLink(linkText, result.Convert(), htmlAttributes, protocol, hostName, fragment);
        }

        public static IHtmlContent ActionLink(this IHtmlHelper htmlHelper, String linkText, IConvertToActionResult result, IDictionary<String, Object> htmlAttributes, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.ActionLink(linkText, result.Convert(), htmlAttributes, protocol, hostName, fragment);
        }
#endif
        #endregion

        #region RouteLink
        public static IHtmlContent RouteLink(this IHtmlHelper htmlHelper, String linkText, IActionResult result, Object htmlAttributes)
        {
            return htmlHelper.RouteLink(linkText, null, result, htmlAttributes, null, null, null);
        }

        public static IHtmlContent RouteLink(this IHtmlHelper htmlHelper, String linkText, String routeName, IActionResult result, Object htmlAttributes, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.RouteLink(linkText, routeName, result.GetSg4ActionResult(), htmlAttributes, protocol, hostName, fragment);
        }

        public static IHtmlContent RouteLink(this IHtmlHelper htmlHelper, String linkText, IActionResult result, IDictionary<String, Object> htmlAttributes)
        {
            return htmlHelper.RouteLink(linkText, null, result, htmlAttributes, null, null, null);
        }

        public static IHtmlContent RouteLink(this IHtmlHelper htmlHelper, String linkText, String routeName, IActionResult result, IDictionary<String, Object> htmlAttributes, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.RouteLink(linkText, routeName, result.GetSg4ActionResult(), htmlAttributes, protocol, hostName, fragment);
        }

        public static IHtmlContent RouteLink<TAction>(this IHtmlHelper htmlHelper, String linkText, Task<TAction> result, Object htmlAttributes)
            where TAction : IActionResult
        {
            return htmlHelper.RouteLink(linkText, result.Result, htmlAttributes);
        }

        public static IHtmlContent RouteLink<TAction>(this IHtmlHelper htmlHelper, String linkText, String routeName, Task<TAction> result, Object htmlAttributes, String protocol = null, String hostName = null, String fragment = null)
            where TAction : IActionResult
        {
            return htmlHelper.RouteLink(linkText, routeName, result.Result, htmlAttributes, protocol, hostName, fragment);
        }

        public static IHtmlContent RouteLink<TAction>(this IHtmlHelper htmlHelper, String linkText, Task<TAction> result, IDictionary<String, Object> htmlAttributes)
            where TAction : IActionResult
        {
            return htmlHelper.RouteLink(linkText, result.Result, htmlAttributes);
        }

        public static IHtmlContent RouteLink<TAction>(this IHtmlHelper htmlHelper, String linkText, String routeName, Task<TAction> result, IDictionary<String, Object> htmlAttributes, String protocol = null, String hostName = null, String fragment = null)
            where TAction : IActionResult
        {
            return htmlHelper.RouteLink(linkText, routeName, result.Result, htmlAttributes, protocol, hostName, fragment);
        }

        public static IHtmlContent RouteLink(this IHtmlHelper htmlHelper, String linkText, Task result, Object htmlAttributes)
        {
            return htmlHelper.RouteLink(linkText, result.GetActionResult(), htmlAttributes);
        }

        public static IHtmlContent RouteLink(this IHtmlHelper htmlHelper, String linkText, String routeName, Task result, Object htmlAttributes, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.RouteLink(linkText, routeName, result.GetActionResult(), htmlAttributes, protocol, hostName, fragment);
        }

        public static IHtmlContent RouteLink(this IHtmlHelper htmlHelper, String linkText, Task result, IDictionary<String, Object> htmlAttributes)
        {
            return htmlHelper.RouteLink(linkText, result.GetActionResult(), htmlAttributes);
        }

        public static IHtmlContent RouteLink(this IHtmlHelper htmlHelper, String linkText, String routeName, Task result, IDictionary<String, Object> htmlAttributes, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.RouteLink(linkText, routeName, result.GetActionResult(), htmlAttributes, protocol, hostName, fragment);
        }

        public static IHtmlContent RouteLink(this IHtmlHelper htmlHelper, String linkText, String routeName, ISg4ActionResult result, Object htmlAttributes, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.RouteLink(linkText, routeName, result, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes), protocol, hostName, fragment);
        }

        public static IHtmlContent RouteLink(this IHtmlHelper htmlHelper, String linkText, String routeName, ISg4ActionResult result, IDictionary<String, Object> htmlAttributes, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.RouteLink(linkText, routeName, protocol ?? result.Protocol, hostName, fragment, result.RouteValueDictionary, htmlAttributes);
        }

#if !CORE1
        public static IHtmlContent RouteLink(this IHtmlHelper htmlHelper, String linkText, IConvertToActionResult result, Object htmlAttributes)
        {
            return htmlHelper.RouteLink(linkText, result.Convert(), htmlAttributes);
        }

        public static IHtmlContent RouteLink(this IHtmlHelper htmlHelper, String linkText, String routeName, IConvertToActionResult result, Object htmlAttributes, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.RouteLink(linkText, routeName, result.Convert(), htmlAttributes, protocol, hostName, fragment);
        }

        public static IHtmlContent RouteLink(this IHtmlHelper htmlHelper, String linkText, IConvertToActionResult result, IDictionary<String, Object> htmlAttributes)
        {
            return htmlHelper.RouteLink(linkText, result.Convert(), htmlAttributes);
        }

        public static IHtmlContent RouteLink(this IHtmlHelper htmlHelper, String linkText, String routeName, IConvertToActionResult result, IDictionary<String, Object> htmlAttributes, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.RouteLink(linkText, routeName, result.Convert(), htmlAttributes, protocol, hostName, fragment);
        }
#endif
        #endregion

        #region AutoNamedRouteLink
        public static IHtmlContent AutoNamedRouteLink(this IHtmlHelper htmlHelper, String linkText, IActionResult result, Object htmlAttributes = null, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.AutoNamedRouteLink(linkText, result, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes), protocol, hostName, fragment);
        }

        public static IHtmlContent AutoNamedRouteLink(this IHtmlHelper htmlHelper, String linkText, IActionResult result, IDictionary<String, Object> htmlAttributes, String protocol = null, String hostName = null, String fragment = null)
        {
            String routeName = AutoRouteNameFromActionResult(result);
            return htmlHelper.RouteLink(linkText, routeName, protocol ?? result.GetSg4ActionResult().Protocol, hostName, fragment, result.GetRouteValueDictionary(), htmlAttributes);
        }

        public static IHtmlContent AutoNamedRouteLink<TAction>(this IHtmlHelper htmlHelper, String linkText, Task<TAction> result, Object htmlAttributes = null, String protocol = null, String hostName = null, String fragment = null)
            where TAction : IActionResult
        {
            return htmlHelper.AutoNamedRouteLink(linkText, result.Result, htmlAttributes, protocol, hostName, fragment);
        }

        public static IHtmlContent AutoNamedRouteLink<TAction>(this IHtmlHelper htmlHelper, String linkText, Task<TAction> result, IDictionary<String, Object> htmlAttributes, String protocol = null, String hostName = null, String fragment = null)
            where TAction : IActionResult
        {
            return htmlHelper.AutoNamedRouteLink(linkText, result.Result, htmlAttributes, protocol, hostName, fragment);
        }

        public static IHtmlContent AutoNamedRouteLink(this IHtmlHelper htmlHelper, String linkText, Task result, Object htmlAttributes = null, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.AutoNamedRouteLink(linkText, result.GetActionResult(), htmlAttributes, protocol, hostName, fragment);
        }

        public static IHtmlContent AutoNamedRouteLink(this IHtmlHelper htmlHelper, String linkText, Task result, IDictionary<String, Object> htmlAttributes, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.AutoNamedRouteLink(linkText, result.GetActionResult(), htmlAttributes, protocol, hostName, fragment);
        }

#if !CORE1
        public static IHtmlContent AutoNamedRouteLink(this IHtmlHelper htmlHelper, String linkText, IConvertToActionResult result, Object htmlAttributes = null, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.AutoNamedRouteLink(linkText, result.Convert(), htmlAttributes, protocol, hostName, fragment);
        }

        public static IHtmlContent AutoNamedRouteLink(this IHtmlHelper htmlHelper, String linkText, IConvertToActionResult result, IDictionary<String, Object> htmlAttributes, String protocol = null, String hostName = null, String fragment = null)
        {
            return htmlHelper.AutoNamedRouteLink(linkText, result.Convert(), htmlAttributes, protocol, hostName, fragment);
        }
#endif
        #endregion

        #region BeginForm
        public static MvcForm BeginForm(this IHtmlHelper htmlHelper, IActionResult result, FormMethod formMethod, Object htmlAttributes)
        {
            return BeginForm(htmlHelper, result, formMethod, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcForm BeginForm(this IHtmlHelper htmlHelper, IActionResult result, FormMethod formMethod = FormMethod.Post, IDictionary<String, Object> htmlAttributes = null)
        {
            return BeginForm(htmlHelper, result.GetSg4ActionResult(), formMethod, htmlAttributes);
        }

        public static MvcForm BeginForm<TAction>(this IHtmlHelper htmlHelper, Task<TAction> result, FormMethod formMethod, Object htmlAttributes)
            where TAction : IActionResult
        {
            return BeginForm(htmlHelper, result.Result, formMethod, htmlAttributes);
        }

        public static MvcForm BeginForm<TAction>(this IHtmlHelper htmlHelper, Task<TAction> result, FormMethod formMethod = FormMethod.Post, IDictionary<String, Object> htmlAttributes = null)
            where TAction : IActionResult
        {
            return BeginForm(htmlHelper, result.Result, formMethod, htmlAttributes);
        }

        public static MvcForm BeginForm(this IHtmlHelper htmlHelper, Task result, FormMethod formMethod, Object htmlAttributes)
        {
            return BeginForm(htmlHelper, result.GetActionResult(), formMethod, htmlAttributes);
        }

        public static MvcForm BeginForm(this IHtmlHelper htmlHelper, Task result, FormMethod formMethod = FormMethod.Post, IDictionary<String, Object> htmlAttributes = null)
        {
            return BeginForm(htmlHelper, result.GetActionResult(), formMethod, htmlAttributes);
        }

        public static MvcForm BeginForm(this IHtmlHelper htmlHelper, ISg4ActionResult result, FormMethod formMethod = FormMethod.Post, IDictionary<String, Object> htmlAttributes = null)
        {
            return htmlHelper.BeginRouteForm(null, result, formMethod, htmlAttributes);
        }

#if !CORE1
        public static MvcForm BeginForm(this IHtmlHelper htmlHelper, IConvertToActionResult result, FormMethod formMethod, Object htmlAttributes)
        {
            return BeginForm(htmlHelper, result.Convert(), formMethod, htmlAttributes);
        }

        public static MvcForm BeginForm(this IHtmlHelper htmlHelper, IConvertToActionResult result, FormMethod formMethod = FormMethod.Post, IDictionary<String, Object> htmlAttributes = null)
        {
            return BeginForm(htmlHelper, result.Convert(), formMethod, htmlAttributes);
        }
#endif
        #endregion

        #region BeginRouteForm
        public static MvcForm BeginRouteForm(this IHtmlHelper htmlHelper, IActionResult result)
        {
            return htmlHelper.BeginRouteForm(null, result, FormMethod.Post, null);
        }

        public static MvcForm BeginRouteForm(this IHtmlHelper htmlHelper, String routeName, IActionResult result, FormMethod formMethod, Object htmlAttributes)
        {
            return htmlHelper.BeginRouteForm(routeName, result, formMethod, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcForm BeginRouteForm(this IHtmlHelper htmlHelper, String routeName, IActionResult result, FormMethod formMethod = FormMethod.Post, IDictionary<String, Object> htmlAttributes = null)
        {
            return htmlHelper.BeginRouteForm(routeName, result.GetSg4ActionResult(), formMethod, htmlAttributes);
        }

        public static MvcForm BeginRouteForm<TAction>(this IHtmlHelper htmlHelper, Task<TAction> result)
            where TAction : IActionResult
        {
            return htmlHelper.BeginRouteForm(result.Result);
        }

        public static MvcForm BeginRouteForm<TAction>(this IHtmlHelper htmlHelper, String routeName, Task<TAction> result, FormMethod formMethod, Object htmlAttributes)
            where TAction : IActionResult
        {
            return htmlHelper.BeginRouteForm(routeName, result.Result, formMethod, htmlAttributes);
        }

        public static MvcForm BeginRouteForm<TAction>(this IHtmlHelper htmlHelper, String routeName, Task<TAction> result, FormMethod formMethod = FormMethod.Post, IDictionary<String, Object> htmlAttributes = null)
            where TAction : IActionResult
        {
            return htmlHelper.BeginRouteForm(routeName, result.Result, formMethod, htmlAttributes);
        }

        public static MvcForm BeginRouteForm(this IHtmlHelper htmlHelper, Task result)
        {
            return htmlHelper.BeginRouteForm(result.GetActionResult());
        }

        public static MvcForm BeginRouteForm(this IHtmlHelper htmlHelper, String routeName, Task result, FormMethod formMethod, Object htmlAttributes)
        {
            return htmlHelper.BeginRouteForm(routeName, result.GetActionResult(), formMethod, htmlAttributes);
        }

        public static MvcForm BeginRouteForm(this IHtmlHelper htmlHelper, String routeName, Task result, FormMethod formMethod = FormMethod.Post, IDictionary<String, Object> htmlAttributes = null)
        {
            return htmlHelper.BeginRouteForm(routeName, result.GetActionResult(), formMethod, htmlAttributes);
        }

        public static MvcForm BeginRouteForm(this IHtmlHelper htmlHelper, String routeName, ISg4ActionResult result, FormMethod formMethod = FormMethod.Post, IDictionary<String, Object> htmlAttributes = null)
        {
            return htmlHelper.BeginRouteForm(routeName, result.RouteValueDictionary, formMethod, null, htmlAttributes);
        }

#if !CORE1
        public static MvcForm BeginRouteForm(this IHtmlHelper htmlHelper, IConvertToActionResult result)
        {
            return htmlHelper.BeginRouteForm(result.Convert());
        }

        public static MvcForm BeginRouteForm(this IHtmlHelper htmlHelper, String routeName, IConvertToActionResult result, FormMethod formMethod, Object htmlAttributes)
        {
            return htmlHelper.BeginRouteForm(routeName, result.Convert(), formMethod, htmlAttributes);
        }

        public static MvcForm BeginRouteForm(this IHtmlHelper htmlHelper, String routeName, IConvertToActionResult result, FormMethod formMethod = FormMethod.Post, IDictionary<String, Object> htmlAttributes = null)
        {
            return htmlHelper.BeginRouteForm(routeName, result.Convert(), formMethod, htmlAttributes);
        }
#endif
        #endregion

        private static String AutoRouteNameFromActionResult(IActionResult result)
        {
            if (result is ISg4MvcActionResult sg4MvcRes)
            {
                String actionName = sg4MvcRes.Action;
                String ctrlName = sg4MvcRes.Controller;
                // get area from route values
                sg4MvcRes.RouteValueDictionary.TryGetValue("area", out Object areaName);
                sg4MvcRes.RouteValueDictionary.Remove("area");
                // compose route name
                String routeName = ComposeAutoRouteName(areaName as String, ctrlName, actionName);
                return routeName;
            }
            else if (result is ISg4PageActionResult sg4pageRes)
            {
                String pageName = sg4pageRes.PageName;
                // compose route name
                String routeName = ComposeAutoRouteName(pageName);
                return routeName;
            }
            else
            {
                // fall back to defaults
                return null;
            }
        }

        public static String ComposeAutoRouteName(String areaName, String controllerName, String actionName)
        {
            if (controllerName == null)
            {
                throw new ArgumentNullException(nameof(controllerName), "Controller name cannot be null");
            }

            if (actionName == null)
            {
                throw new ArgumentNullException(nameof(actionName), "Action name cannot be null");
            }

            if (String.IsNullOrWhiteSpace(areaName))
            {
                areaName = "__AUTONAMEDROUTE_DEFAULT__";
            }

            return String.Join("_", areaName, controllerName, actionName).ToLowerInvariant();
        }

        public static String ComposeAutoRouteName(String pageName)
        {
            if (pageName == null)
            {
                throw new ArgumentNullException(nameof(pageName), "Page name cannot be null");
            }

            return pageName.Replace('/', '_').ToLowerInvariant();
        }
    }
}
