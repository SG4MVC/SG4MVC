using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Sg4Mvc.Generator.Pages.Interfaces;

public interface IPageRewriterService
{
    IList<PageDefinition> RewritePages(GeneratorExecutionContext context);
}
