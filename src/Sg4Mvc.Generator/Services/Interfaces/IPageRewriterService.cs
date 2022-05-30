using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Sg4Mvc.Generator.Services.Interfaces;

public interface IPageRewriterService
{
    IList<PageDefinition> RewritePages(GeneratorExecutionContext context);
}
