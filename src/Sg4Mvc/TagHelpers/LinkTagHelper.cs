using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Sg4Mvc.TagHelpers;

[HtmlTargetElement("link", Attributes = HrefAttribute, TagStructure = TagStructure.WithoutEndTag)]
public class LinkTagHelper : UrlResolutionTagHelper
{
    private const String HrefAttribute = "mvc-href";

    public LinkTagHelper(IUrlHelperFactory urlHelperFactory, HtmlEncoder htmlEncoder)
        : base(urlHelperFactory, htmlEncoder)
    { }

    [HtmlAttributeName(HrefAttribute)]
    public String Source { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("href", Source);
        ProcessUrlAttribute("href", output);
    }
}
