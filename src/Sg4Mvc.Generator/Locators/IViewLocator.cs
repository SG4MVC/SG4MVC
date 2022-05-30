using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Sg4Mvc.Generator.Locators;

public interface IViewLocator
{
    IEnumerable<View> Find(GeneratorExecutionContext context);
}
