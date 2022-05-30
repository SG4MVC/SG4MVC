using System;

namespace Sg4Mvc.ModelUnbinders;

public interface IModelUnbinderProvider
{
    IModelUnbinder GetUnbinder(Type routeValueType);
}
