using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sg4Mvc.Generator.Controllers;
using Sg4Mvc.Generator.Extensions;

namespace Sg4Mvc.Generator.Pages;

public static class PageTargetFilter
{
    public static IncrementalValuesProvider<PageDefinition> GetCompilationDetails(
        IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, ct) => IsSyntaxTargetForGeneration(s, ct),
                transform: static (ctx, ct) => GetSemanticTargetForGeneration(ctx, ct))
            .Where(static m => m is not null)!
            .Select(PageDefinitionTransform.Perform);
    }

    private static Boolean IsSyntaxTargetForGeneration(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return syntaxNode is ClassDeclarationSyntax classDeclarationSyntax
               && classDeclarationSyntax.Modifiers.Any(SyntaxKind.PublicKeyword)
               && !classDeclarationSyntax.Modifiers.Any(SyntaxKind.AbstractKeyword);
    }

    private static CompilationDetails GetSemanticTargetForGeneration(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var typeDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var namedTypeSymbol = context.SemanticModel.GetDeclaredSymbol(typeDeclarationSyntax);

        if (namedTypeSymbol is not null
            && namedTypeSymbol.InheritsFrom(FullTypeNames.PageModel)
            && !namedTypeSymbol.GetAttributes().Any(a => a.AttributeClass.InheritsFrom(FullTypeNames.Sg4MvcExcludeAttribute)))
        {

            return new CompilationDetails(typeDeclarationSyntax, namedTypeSymbol);
        }

        return null;
    }
}
