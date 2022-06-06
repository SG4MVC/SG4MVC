using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
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

        var combined = controllers.Collect()
            .Combine(pages.Collect())
            .Combine(additionalTexts.Collect())
            .Combine(context.CompilationProvider)
            .Combine(context.AnalyzerConfigOptionsProvider);

        context.RegisterSourceOutput(combined, PerformSourceOutput);
    }

    private void PerformSourceOutput(SourceProductionContext spc,
        ((((ImmutableArray<ControllerDefinition> Controllers,
            ImmutableArray<PageDefinition> Pages) ControllersAndPages,
            ImmutableArray<AdditionalText> AdditionalTexts) ControllersPagesAndViews,
            Compilation Compilation) Everything,
            AnalyzerConfigOptionsProvider ConfigOptionsProvider) source)
    {
        var ((((controllers, pages), additionalTexts), compilation), configOptionsProvider) = source;

        if (compilation.Assembly.GetAttributes()
            .Any(a => a.AttributeClass.Name == "GenerateSg4MvcAttribute"))
        {
            var sw = Stopwatch.StartNew();

            Execute(controllers, pages, additionalTexts, compilation, configOptionsProvider, spc);

            sw.StopAndReport(spc, "Total");
        }
    }

    public static void Execute(
        ImmutableArray<ControllerDefinition> controllers,
        ImmutableArray<PageDefinition> pages,
        ImmutableArray<AdditionalText> additionalTexts,
        Compilation compilation,
        AnalyzerConfigOptionsProvider analyzerConfigOptionsProvider,
        SourceProductionContext context)
    {

        var sw = Stopwatch.StartNew();

        var workingDirectory = analyzerConfigOptionsProvider.GetWorkingDirectory();
        sw.ReportAndRestart(context, "GetWorkingDirectory");

        // Prep the project Compilation object, and process the Controller public methods list
        SyntaxNodeHelpers.PopulateControllerClassMethodNames(compilation);
        sw.ReportAndRestart(context, "PopulateControllerClassMethodNames");

        var (controllerDefinitions, pageViews) = DataGroupingService.Prep(
            workingDirectory,
            controllers.ToList(),
            pages.ToList(),
            additionalTexts);
        sw.ReportAndRestart(context, "DataGroupingService.Prep");

        // Generate the files
        var generatorService = GeneratorServiceFactory.Create(context);
        generatorService.Generate(workingDirectory, controllerDefinitions, pageViews);
        sw.ReportAndRestart(context, "generatorService.Generate");
    }
}
