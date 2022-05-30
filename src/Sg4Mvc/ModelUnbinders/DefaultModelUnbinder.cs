using System;
using Microsoft.AspNetCore.Routing;

namespace Sg4Mvc.ModelUnbinders;

public class DefaultModelUnbinder : IModelUnbinder
{
    public void UnbindModel(RouteValueDictionary routeValueDictionary, String routeName, Object routeValue)
    {
        routeValueDictionary[routeName] = routeValue;
    }
}
