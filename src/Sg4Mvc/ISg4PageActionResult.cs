using System;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Mvc;

public interface ISg4PageActionResult : ISg4ActionResult
{
    String PageName { get; set; }
    String PageHandler { get; set; }
    new String Protocol { get; set; }
    new RouteValueDictionary RouteValueDictionary { get; set; }
}
