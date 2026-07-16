using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sg4Mvc.Generator.Extensions;
using Sg4Mvc.Generator.Pages;

namespace Sg4Mvc.Generator.Controllers;

public static class PageDefinitionTransform
{
    public static PageDefinition Perform(CompilationDetails compilationDetails, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var (classDeclarationSyntax, namedTypeSymbol) = compilationDetails;

        var namespaceSyntax = classDeclarationSyntax.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>();

        var fullNamespace = namespaceSyntax.Name
            .ToFullString()
            .Trim();

        var isSecure = namedTypeSymbol.GetAttributes()
            .Any(a => a.AttributeClass.InheritsFrom(FullTypeNames.RequireHttpsAttribute));

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
            .Where(m => m.Name.StartsWith("On"))
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

        return new PageDefinition(
            fullNamespace,
            namedTypeSymbol.Name.TrimEnd("Model"),
            isSecure,
            namedTypeSymbol,
            new List<String> { compilationDetails.ClassDeclarationSyntax.SyntaxTree.FilePath })
        {
            SymbolName = namedTypeSymbol.Name,
            ContainingNamespace = namedTypeSymbol.ContainingNamespace?.ToDisplayString(),
            TypeParameterNames = typeParams,
            PublicConstructors = ctors,
            HandlerMethods = methods,
        };
    }
}

