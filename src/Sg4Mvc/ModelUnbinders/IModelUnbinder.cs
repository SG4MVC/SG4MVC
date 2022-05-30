using System;
using Microsoft.AspNetCore.Routing;

namespace Sg4Mvc.ModelUnbinders;

public interface IModelUnbinder
{
    void UnbindModel(RouteValueDictionary routeValueDictionary, String routeName, Object routeValue);
}

public interface IModelUnbinder<in T> where T : class
{
    void UnbindModel(RouteValueDictionary routeValueDictionary, String routeName, T routeValue);
}
