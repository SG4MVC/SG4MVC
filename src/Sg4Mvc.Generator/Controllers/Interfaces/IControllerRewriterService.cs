using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Sg4Mvc.Generator.Controllers.Interfaces;

public interface IControllerRewriterService
{
    IList<ControllerDefinition> RewriteControllers(GeneratorExecutionContext context);
}
