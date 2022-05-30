using System;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Mvc;

public interface ISg4MvcActionResult : ISg4ActionResult
{
    String Controller { get; set; }
    String Action { get; set; }
    new String Protocol { get; set; }
    new RouteValueDictionary RouteValueDictionary { get; set; }
}
