using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Sg4Mvc.Generator.Controllers;
using Sg4Mvc.Generator.Extensions;
using Sg4Mvc.Generator.Pages;
using Sg4Mvc.Generator.Services;

namespace Sg4Mvc.Generator;

[Generator(LanguageNames.CSharp)]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var controllers = ControllerTargetFilter.GetCompilationDetails(context);
        var pages = PageTargetFilter.GetCompilationDetails(context);
        var additionalTexts = AdditionalTextsTargetFilter.GetCompilationDetails(context);

        // Narrow extract: only re-evaluates when ASP.NET Core framework assemblies change.
        var frameworkMethodNames = context.CompilationProvider
            .Select(static (c, _) => SyntaxNodeHelpers.GetFrameworkMethodNames(c));

        // Narrow extract: only re-evaluates when [assembly: GenerateSg4Mvc] is added/removed.
        var hasGenerateAttribute = context.CompilationProvider
            .Select(static (c, _) => c.Assembly.GetAttributes()
                .Any(a => a.AttributeClass?.Name == "GenerateSg4MvcAttribute"));

        // Track sg4mvc.json through the AdditionalTexts pipeline — Roslyn caches by content.
        var settingsFile = additionalTexts
            .Where(static t => t.Path.EndsWith("sg4mvc.json", StringComparison.OrdinalIgnoreCase))
            .Collect()
            .Select(static (files, _) => files.Length > 0 ? files[0] : null);

        var settings = settingsFile
            .Select(static (file, ct) => Settings.Load(file, ct));

        var workingDirectory = context.AnalyzerConfigOptionsProvider
            .Select(static (provider, _) => provider.GetWorkingDirectory());

        // Exclude sg4mvc.json from the view texts pipeline.
        var viewTexts = additionalTexts
            .Where(static t => !t.Path.EndsWith("sg4mvc.json", StringComparison.OrdinalIgnoreCase));

        var combined = controllers.Collect()
            .Combine(pages.Collect())
            .Combine(viewTexts.Collect())
            .Combine(frameworkMethodNames)
            .Combine(settings)
            .Combine(workingDirectory)
            .Combine(hasGenerateAttribute);

        context.RegisterSourceOutput(combined, PerformSourceOutput);
    }

    private void PerformSourceOutput(SourceProductionContext spc,
        ((((((ImmutableArray<ControllerDefinition> Controllers,
              ImmutableArray<PageDefinition> Pages) ControllersAndPages,
              ImmutableArray<AdditionalText> ViewTexts) WithViews,
              FrameworkMethodNames FrameworkMethodNames) WithNames,
              Settings Settings) WithSettings,
              String WorkingDirectory) WithWorkDir,
              Boolean HasGenerateAttribute) source)
    {
        var ((((((controllers, pages), viewTexts), frameworkMethodNames), settings), workingDirectory), hasGenerateAttribute) = source;

        if (!hasGenerateAttribute)
            return;

        Execute(controllers, pages, viewTexts, frameworkMethodNames, settings, workingDirectory, spc);
    }

    public static void Execute(
        ImmutableArray<ControllerDefinition> controllers,
        ImmutableArray<PageDefinition> pages,
        ImmutableArray<AdditionalText> additionalTexts,
        FrameworkMethodNames frameworkMethodNames,
        Settings settings,
        String workingDirectory,
        SourceProductionContext context)
    {
        Logging.Reset();
        Logging.LogDirectory = workingDirectory;
        Logging.ReportProgress("Execute");

        var (controllerDefinitions, pageViews) = DataGroupingService.Prep(
            workingDirectory,
            controllers.ToList(),
            pages.ToList(),
            additionalTexts,
            settings);
        Logging.ReportProgress("DataGroupingService.Prep");

        var generatorService = GeneratorServiceFactory.Create(context, settings, frameworkMethodNames);
        generatorService.Generate(workingDirectory, controllerDefinitions, pageViews);

        Logging.WriteFile();
    }
}

