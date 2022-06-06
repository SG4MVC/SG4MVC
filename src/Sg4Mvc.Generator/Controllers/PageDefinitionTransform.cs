using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sg4Mvc.Generator.Extensions;
using Sg4Mvc.Generator.Pages;

namespace Sg4Mvc.Generator.Controllers;

public static class PageDefinitionTransform
{
    public static PageDefinition Perform(
        CompilationDetails compilationDetails,
        CancellationToken ct
    )
    {
        ct.ThrowIfCancellationRequested();

        var (classDeclarationSyntax, namedTypeSymbol) = compilationDetails;

        var namespaceSyntax = classDeclarationSyntax.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>();

        var fullNamespace = namespaceSyntax.Name
            .ToFullString()
            .Trim();

        var isSecure = namedTypeSymbol.GetAttributes()
            .Any(a => a.AttributeClass.InheritsFrom(FullTypeNames.RequireHttpsAttribute));

        return new PageDefinition(
            fullNamespace,
            namedTypeSymbol.Name.TrimEnd("Model"),
            isSecure,
            namedTypeSymbol,
            new List<String>
            {
                compilationDetails.ClassDeclarationSyntax.SyntaxTree.FilePath
            });
    }
}
