using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sg4Mvc.Generator.Extensions;

namespace Sg4Mvc.Generator.Controllers;

public static class ControllerDefinitionTransform
{
    public static ControllerDefinition Perform(CompilationDetails compilationDetails, CancellationToken ct)
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

        var typeParams = namedTypeSymbol.TypeParameters
            .Select(tp => tp.Name)
            .ToArray();

        var ctors = namedTypeSymbol.Constructors
            .Where(c => c.DeclaredAccessibility == Accessibility.Public && !c.IsImplicitlyDeclared)
            .Where(SyntaxNodeHelpers.IsNotSg4MvcGenerated)
            .Select(c => new ConstructorData(c.Parameters.Select(p => p.Type.ToDisplayString()).ToList()))
            .ToArray();

        var methods = namedTypeSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.DeclaredAccessibility == Accessibility.Public && m.MethodKind == MethodKind.Ordinary)
            .Where(SyntaxNodeHelpers.IsNotSg4MvcGenerated)
            .Where(SyntaxNodeHelpers.IsNotSg4MvcExcluded)
            .Select(m => new ActionMethodData(
                m.Name,
                m.ReturnType.ToDisplayString(),
                m.GetAttributes().Any(a => a.AttributeClass.InheritsFrom(FullTypeNames.RequireHttpsAttribute)),
                m.Parameters.Select(p => new ParameterData(
                    p.Name,
                    p.Type.ToDisplayString(),
                    p.GetAttributes()
                        .Where(a => a.AttributeClass.InheritsFrom(FullTypeNames.BindAttribute))
                        .SelectMany(a => a.NamedArguments.Where(x => x.Key == "Prefix"))
                        .Select(x => x.Value.Value as String)
                        .FirstOrDefault()))
                .ToList()))
            .ToArray();

        return new ControllerDefinition
        {
            Namespace = fullNamespace,
            Name = namedTypeSymbol.Name.TrimEnd("Controller"),
            Area = cAreaName,
            IsSecure = isSecure,
            Symbol = namedTypeSymbol,
            SymbolName = namedTypeSymbol.Name,
            ContainingNamespace = namedTypeSymbol.ContainingNamespace?.ToDisplayString(),
            TypeParameterNames = typeParams,
            PublicConstructors = ctors,
            ActionMethods = methods,
        };
    }
}

