using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sg4Mvc.Generator.Extensions;

namespace Sg4Mvc.Generator.Controllers;

public static class ControllerDefinitionTransform
{
    public static ControllerDefinition Perform(
        CompilationDetails compilationDetails,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var (classDeclarationSyntax, namedTypeSymbol) = compilationDetails;

        var fullNamespace = classDeclarationSyntax
            .FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>()
            .Name
            .ToFullString()
            .Trim();

        var isSecure = namedTypeSymbol.GetAttributes()
            .Any(a => a.AttributeClass.InheritsFrom(FullTypeNames.RequireHttpsAttribute));

        var cAreaName = namedTypeSymbol.GetControllerArea();

        return new ControllerDefinition
        {
            Namespace = fullNamespace,
            Name = namedTypeSymbol.Name.TrimEnd("Controller"),
            Area = cAreaName,
            IsSecure = isSecure,
            Symbol = namedTypeSymbol,
        };
    }
}
