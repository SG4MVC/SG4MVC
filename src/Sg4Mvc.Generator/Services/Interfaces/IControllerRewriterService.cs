using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Sg4Mvc.Generator.Services.Interfaces;

public interface IControllerRewriterService
{
    IList<ControllerDefinition> RewriteControllers(GeneratorExecutionContext context);
}
