using Microsoft.CodeAnalysis;

namespace Sg4Mvc.Generator.Controllers;

public static class AdditionalTextsTargetFilter
{
    public static IncrementalValuesProvider<AdditionalText> GetCompilationDetails(
        IncrementalGeneratorInitializationContext context)
    {
        return context.AdditionalTextsProvider
            .Where(static t => t.Path.EndsWith(".cshtml") || t.Path.Contains("wwwroot"));
    }
}
