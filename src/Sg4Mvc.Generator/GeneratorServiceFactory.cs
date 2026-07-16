using Microsoft.CodeAnalysis;
using Sg4Mvc.Generator.Controllers;
using Sg4Mvc.Generator.Locators;
using Sg4Mvc.Generator.Pages;
using Sg4Mvc.Generator.Services;

namespace Sg4Mvc.Generator;

public static class GeneratorServiceFactory
{
    public static Sg4MvcGeneratorService Create(SourceProductionContext context, Settings settings, FrameworkMethodNames frameworkMethodNames)
    {
        var controllerGeneratorService = new ControllerGeneratorService(settings, frameworkMethodNames);

        var fileLocator = new PhysicalFileLocator();

        var staticFileLocator = new DefaultStaticFileLocator(fileLocator, settings);

        return new Sg4MvcGeneratorService(
            controllerGeneratorService,
            new PageGeneratorService(settings, frameworkMethodNames),
            new StaticFileGeneratorService(staticFileLocator, settings),
            settings,
            context);
    }
}
