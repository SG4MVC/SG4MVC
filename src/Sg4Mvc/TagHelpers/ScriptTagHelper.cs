using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Sg4Mvc.TagHelpers;

[HtmlTargetElement("script", Attributes = SourceAttribute)]
public class ScriptTagHelper : UrlResolutionTagHelper
{
    private const String SourceAttribute = "mvc-src";

    public ScriptTagHelper(IUrlHelperFactory urlHelperFactory, HtmlEncoder htmlEncoder)
        : base(urlHelperFactory, htmlEncoder)
    { }

    [HtmlAttributeName(SourceAttribute)]
    public String Source { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("src", Source);
        ProcessUrlAttribute("src", output);
    }
}
