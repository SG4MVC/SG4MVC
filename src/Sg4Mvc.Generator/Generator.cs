using System;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Sg4Mvc.Generator.Locators;
using Sg4Mvc.Generator.Services;

namespace Sg4Mvc.Generator;

[Generator]
public class Generator : ISourceGenerator
{
    internal static String GetVersion()
    {
        var assembly = typeof(Generator).Assembly;

        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version.ToString();

        return version;
    }

    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var settings = new Settings();
        var filePersistService = new FilePersistService(context);
        var controllerGeneratorService = new ControllerGeneratorService(settings);
        var controllerRewriterService = new ControllerRewriterService(controllerGeneratorService);
        var fileLocator = new PhysicalFileLocator();

        var viewLocators = new IViewLocator[]
        {
            new FeatureFolderRazorViewLocator(fileLocator, settings),
            new DefaultRazorViewLocator(fileLocator, settings)
        };

        var pageViewLocators = new IPageViewLocator[]
        {
            new DefaultRazorPageViewLocator(fileLocator, settings)
        };

        var staticFileLocators = new IStaticFileLocator[]
        {
            new DefaultStaticFileLocator(fileLocator, settings)
        };

        var command = new GenerateCommand(
            controllerRewriter: controllerRewriterService,
            new PageRewriterService(filePersistService, settings),
            viewLocators,
            pageViewLocators,
            new Sg4MvcGeneratorService(
                controllerRewriterService,
                controllerGeneratorService,
                new PageGeneratorService(settings),
                new StaticFileGeneratorService(staticFileLocators, settings),
                filePersistService,
                settings),
            settings);

        command.Run(context);
    }
}
