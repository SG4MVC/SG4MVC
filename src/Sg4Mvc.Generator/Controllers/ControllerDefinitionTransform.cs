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

        var cAreaName = GetControllerArea(namedTypeSymbol);

        return new ControllerDefinition
        {
            Namespace = fullNamespace,
            Name = namedTypeSymbol.Name.TrimEnd("Controller"),
            Area = cAreaName,
            IsSecure = isSecure,
            Symbol = namedTypeSymbol,
        };
    }

    private static String GetControllerArea(INamedTypeSymbol controllerSymbol)
    {
        AttributeData areaAttribute = null;

        var typeSymbol = controllerSymbol;
        while (typeSymbol != null && areaAttribute == null)
        {
            areaAttribute = typeSymbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass.InheritsFrom(FullTypeNames.AreaAttribute));

            typeSymbol = typeSymbol.BaseType;
        }

        if (areaAttribute == null)
        {
            return String.Empty;
        }

        if (areaAttribute.AttributeClass.ToDisplayString() == FullTypeNames.AreaAttribute)
        {
            return areaAttribute.ConstructorArguments[0].Value?.ToString();
        }

        // parse the constructor to get the area name from derived types
        if (areaAttribute.AttributeClass.BaseType.ToDisplayString() == FullTypeNames.AreaAttribute)
        {
            // direct descendant. Reading the area name from the constructor
            var constructorInit = areaAttribute.AttributeConstructor.DeclaringSyntaxReferences
                .SelectMany(s => s.SyntaxTree.GetRoot()
                    .DescendantNodesAndSelf()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(c => c.Identifier.Text == areaAttribute.AttributeClass.Name))
                .SelectMany(s => s.DescendantNodesAndSelf().OfType<ConstructorInitializerSyntax>())
                .First();

            if (constructorInit.ArgumentList.Arguments.Count > 0)
            {
                var arg = constructorInit.ArgumentList.Arguments[0];
                if (arg.Expression is LiteralExpressionSyntax litExp)
                {
                    return litExp.Token.ValueText;
                }
            }
        }

        return String.Empty;
    }
}
