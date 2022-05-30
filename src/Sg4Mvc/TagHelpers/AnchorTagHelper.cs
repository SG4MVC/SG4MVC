using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
#if !CORE1
using Microsoft.AspNetCore.Mvc.Infrastructure;
#endif

namespace Sg4Mvc.TagHelpers
{
    [HtmlTargetElement("a", Attributes = ActionAttribute)]
    public class AnchorTagHelper : TagHelper
    {
        private const String ActionAttribute = "mvc-action";

        private readonly IUrlHelperFactory _urlHelperFactory;
        public AnchorTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }
        /// <summary>
        /// The MVC action call (use SG4MVC syntax i.e. `MVC.Home.Index()`)
        /// </summary>
        [HtmlAttributeName(ActionAttribute)]
        public Object ObjectAction { get; set; }
        [HtmlAttributeName(ActionAttribute)]
        public IActionResult Action { get; set; }
        [HtmlAttributeName(ActionAttribute)]
        public Task<IActionResult> TaskAction { get; set; }
        [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
        public IDictionary<String, String> RouteValues { get; set; } = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase);

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.RemoveAll(ActionAttribute);
            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);

#if !CORE1
            if (ObjectAction is IConvertToActionResult convertToActionResult)
            {
                ObjectAction = convertToActionResult.Convert();
            }
#endif
            RouteValueDictionary routeValues = null;
            switch (ObjectAction)
            {
                case ISg4ActionResult t4ActionResult:
                    routeValues = t4ActionResult.RouteValueDictionary;
                    break;
                case Task task:
                    var result = task.GetActionResult();
                    if (result is ISg4ActionResult taskActionResult)
                    {
                        routeValues = taskActionResult.RouteValueDictionary;
                    }

                    break;
            }

            if (routeValues != null)
            {
                foreach (var set in RouteValues)
                    routeValues[set.Key] = set.Value;

                var url = urlHelper.RouteUrl(routeValues);
                output.Attributes.SetAttribute("href", url);
            }
        }
    }
}
