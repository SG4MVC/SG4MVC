using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Sg4Mvc.Generator.Locators;

public interface IPageViewLocator
{
    IEnumerable<PageView> Find(GeneratorExecutionContext context);
}
