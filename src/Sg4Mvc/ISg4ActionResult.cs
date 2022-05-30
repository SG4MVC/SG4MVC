using System;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Mvc;

public interface ISg4ActionResult
{
    String Protocol { get; }
    RouteValueDictionary RouteValueDictionary { get; }
}
