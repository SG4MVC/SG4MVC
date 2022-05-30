using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Mvc;

public static class Sg4Extensions
{
    public static ISg4ActionResult GetSg4ActionResult(this IActionResult result)
    {
        if (result is not ISg4ActionResult actionResult)
        {
            throw new InvalidOperationException("SG4MVC was called incorrectly.");
        }

        return actionResult;
    }

    public static ISg4ActionResult GetSg4ActionResult<TActionResult>(this Task<TActionResult> taskResult)
        where TActionResult : IActionResult
    {
        return GetSg4ActionResult(taskResult.Result);
    }

    public static ISg4ActionResult GetSg4ActionResult(this Task taskResult)
    {
        return GetSg4ActionResult(GetActionResult(taskResult));
    }

    internal static IActionResult GetActionResult(this Task task)
    {
        switch (task)
        {
            case Task<IActionResult> taskIActionResult:
                return taskIActionResult.Result;
            case Task<ActionResult> taskIActionResult:
                return taskIActionResult.Result;
            case Task<ContentResult> taskIActionResult:
                return taskIActionResult.Result;
            case Task<FileResult> taskIActionResult:
                return taskIActionResult.Result;
            case Task<FileStreamResult> taskIActionResult:
                return taskIActionResult.Result;
            case Task<PhysicalFileResult> taskIActionResult:
                return taskIActionResult.Result;
            case Task<VirtualFileResult> taskIActionResult:
                return taskIActionResult.Result;
            case Task<JsonResult> taskIActionResult:
                return taskIActionResult.Result;
            case Task<RedirectResult> taskIActionResult:
                return taskIActionResult.Result;
            case Task<RedirectToActionResult> taskIActionResult:
                return taskIActionResult.Result;
            case Task<RedirectToRouteResult> taskIActionResult:
                return taskIActionResult.Result;
            case Task<LocalRedirectResult> taskIActionResult:
                return taskIActionResult.Result;
            default:
                if (task.GetType().GetTypeInfo().IsGenericType)
                {
                    var resultProp = task.GetType().GetTypeInfo().GetProperty("Result");
                    if (resultProp != null)
                    {
                        var result = resultProp.GetValue(task);
                        if (result is IActionResult actionResult)
                        {
                            return actionResult;
                        }
                    }
                }
                return null;
        }
    }

    public static RouteValueDictionary GetRouteValueDictionary(this IActionResult result)
    {
        return result.GetSg4ActionResult().RouteValueDictionary;
    }

    public static void InitMVCT4Result(this ISg4MvcActionResult result, String area, String controller, String action, String protocol = null)
    {
        result.Controller = controller;
        result.Action = action;
        result.Protocol = protocol;
        result.RouteValueDictionary = new RouteValueDictionary
        {
            { "Area", area ?? String.Empty },
            { "Controller", controller },
            { "Action", action }
        };
    }

    public static void InitMVCT4Result(this ISg4PageActionResult result, String pageName, String pageHandler, String protocol = null)
    {
        result.PageName = pageName;
        result.PageHandler = pageHandler;
        result.Protocol = protocol;
        result.RouteValueDictionary = new RouteValueDictionary
        {
            { "Page", pageName },
            { "Handler", pageHandler },
        };
    }

    public static IActionResult AddRouteValues(this IActionResult result, Object routeValues)
    {
        return result.AddRouteValues(new RouteValueDictionary(routeValues));
    }

    public static IActionResult AddRouteValues(this IActionResult result, RouteValueDictionary routeValues)
    {
        RouteValueDictionary currentRouteValues = result.GetRouteValueDictionary();

        // Add all the extra values
        foreach (var pair in routeValues)
        {
            ModelUnbinderHelpers.AddRouteValues(currentRouteValues, pair.Key, pair.Value);
        }

        return result;
    }

    public static IActionResult AddRouteValue(this IActionResult result, String name, Object value)
    {
        RouteValueDictionary routeValues = result.GetRouteValueDictionary();
        ModelUnbinderHelpers.AddRouteValues(routeValues, name, value);
        return result;
    }
}
