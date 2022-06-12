using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Sg4Mvc.Generator.Extensions;

/// <summary>
/// A collection of helper and fluent extension methods to help manipulate SyntaxNodes
/// </summary>
public static class SyntaxNodeHelpers
{
    public static Boolean InheritsFrom(this ITypeSymbol symbol, String fullName)
    {
        if (symbol.TypeKind == TypeKind.Interface && symbol.ToString() == fullName
            || symbol.AllInterfaces.Any(i => i.ToString() == fullName))
        {
            return true;
        }

        while (true)
        {
            if (symbol.TypeKind == TypeKind.Class && symbol.ToString() == fullName)
            {
                return true;
            }
            if (symbol.BaseType != null)
            {
                symbol = symbol.BaseType;
                continue;
            }
            break;
        }
        return false;
    }

    public static Boolean InheritsFrom<T>(this ITypeSymbol symbol)
    {
        var matchingTypeName = typeof(T).FullName;
        if (typeof(T).IsInterface)
        {
            return symbol.ToString() == matchingTypeName || symbol.AllInterfaces.Any(i => i.ToString() == matchingTypeName);
        }
        while (true)
        {
            if (symbol.TypeKind == TypeKind.Class && symbol.ToString() == matchingTypeName)
            {
                return true;
            }
            if (symbol.BaseType != null)
            {
                symbol = symbol.BaseType;
                continue;
            }
            break;
        }
        return false;
    }

    public static SyntaxToken[] CreateModifiers(params SyntaxKind[] kinds)
    {
        return kinds.Select(m => Token(TriviaList(), m, TriviaList(Space))).ToArray();
    }

    public static Boolean IsNotSg4MvcGenerated(this ISymbol method)
    {
        return !method.GetAttributes()
            .Any(a => a.AttributeClass.InheritsFrom<GeneratedCodeAttribute>());
    }

    public static Boolean IsNotSg4MvcExcluded(this ISymbol method)
    {
        return !method.GetAttributes()
            .Any(a => a.AttributeClass.InheritsFrom("Microsoft.AspNetCore.Mvc.Sg4MvcExcludeAttribute")
                || a.AttributeClass.Name == "Sg4MvcExclude");
    }

    private static String[] _controllerClassMethodNames = null;
    private static String[] _pageClassMethodNames = null;

    public static void PopulateControllerClassMethodNames(Compilation compilation)
    {
        var result = new List<String>();
        var typeSymbol = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.Controller");
        while (typeSymbol != null)
        {
            var methodNames = typeSymbol.GetMembers()
                .Where(r => r.Kind == SymbolKind.Method
                    && r.DeclaredAccessibility == Accessibility.Public
                    && r.IsVirtual)
                .Select(s => s.Name);
            result.AddRange(methodNames);
            typeSymbol = typeSymbol.BaseType;
        }
        _controllerClassMethodNames = result.Distinct().ToArray();

        result = new List<String>();
        typeSymbol = compilation.GetTypeByMetadataName(FullTypeNames.PageModel);
        while (typeSymbol != null)
        {
            var methodNames = typeSymbol.GetMembers()
                .Where(r => r.Kind == SymbolKind.Method
                    && r.DeclaredAccessibility == Accessibility.Public
                    && r.IsVirtual)
                .Select(s => s.Name);
            result.AddRange(methodNames);
            typeSymbol = typeSymbol.BaseType;
        }
        _pageClassMethodNames = result.Distinct().ToArray();
    }

    public static Boolean IsMvcAction(this IMethodSymbol method)
    {
        if (method.GetAttributes().Any(a => a.AttributeClass.InheritsFrom(FullTypeNames.NonActionAttribute)))
        {
            return false;
        }

        if (_controllerClassMethodNames.Contains(method.Name))
        {
            return false;
        }

        return true;
    }

    public static Boolean IsRazorPageAction(this IMethodSymbol method)
    {
        if (method.GetAttributes().Any(a => a.AttributeClass.InheritsFrom(FullTypeNames.NonActionAttribute)))
        {
            return false;
        }

        if (_pageClassMethodNames.Contains(method.Name))
        {
            return false;
        }

        if (!method.Name.StartsWith("On"))
        {
            return false;
        }

        return true;
    }

    public static List<IMethodSymbol> GetPublicNonGeneratedControllerMethods(this ITypeSymbol controller)
    {
        return controller.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.DeclaredAccessibility == Accessibility.Public && m.MethodKind == MethodKind.Ordinary)
            .Where(IsNotSg4MvcGenerated)
            .Where(IsNotSg4MvcExcluded)
            .Where(IsMvcAction)
            .ToList();
    }

    public static List<IMethodSymbol> GetPublicNonGeneratedPageMethods(this ITypeSymbol controller)
    {
        return controller.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.DeclaredAccessibility == Accessibility.Public && m.MethodKind == MethodKind.Ordinary)
            .Where(IsNotSg4MvcGenerated)
            .Where(IsNotSg4MvcExcluded)
            .Where(IsRazorPageAction)
            .ToList();
    }

    private static AttributeSyntax CreateGeneratedCodeAttribute()
    {
        var arguments =
            AttributeArgumentList(
                SeparatedList(
                    new[]
                    {
                        AttributeArgument(
                            LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(Constants.ProjectName))),
                        AttributeArgument(
                            LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(Constants.Version)))
                    }));
        return Attribute(IdentifierName("GeneratedCode"), arguments);
    }

    public static AttributeListSyntax GeneratedNonUserCodeAttributeList()
        => AttributeList(SeparatedList(new[] { CreateGeneratedCodeAttribute(), Attribute(IdentifierName("DebuggerNonUserCode")) }));

    public static MethodDeclarationSyntax WithNonActionAttribute(this MethodDeclarationSyntax node)
        => node.AddAttributeLists(AttributeList(SingletonSeparatedList(Attribute(IdentifierName("NonAction")))));

    public static MethodDeclarationSyntax WithNonHandlerAttribute(this MethodDeclarationSyntax node)
        => node.AddAttributeLists(AttributeList(SingletonSeparatedList(Attribute(IdentifierName("NonHandler")))));

    public static FieldDeclarationSyntax WithGeneratedAttribute(this FieldDeclarationSyntax node)
        => node.AddAttributeLists(AttributeList(SingletonSeparatedList(CreateGeneratedCodeAttribute())));

    public static PropertyDeclarationSyntax WithGeneratedNonUserCodeAttribute(this PropertyDeclarationSyntax node)
        => node.AddAttributeLists(AttributeList(SeparatedList(new[] { CreateGeneratedCodeAttribute(), Attribute(IdentifierName("DebuggerNonUserCode")) })));

    /// TODO: Can this use a aeparated list?
    public static ClassDeclarationSyntax WithModifiers(this ClassDeclarationSyntax node, params SyntaxKind[] modifiers)
    {
        return node.AddModifiers(CreateModifiers(modifiers));
    }

    public static ConstructorDeclarationSyntax WithModifiers(this ConstructorDeclarationSyntax node, params SyntaxKind[] modifiers)
    {
        return node.AddModifiers(CreateModifiers(modifiers));
    }

    public static MethodDeclarationSyntax WithModifiers(this MethodDeclarationSyntax node, params SyntaxKind[] modifiers)
    {
        return node.AddModifiers(CreateModifiers(modifiers));
    }

    public static FieldDeclarationSyntax WithModifiers(this FieldDeclarationSyntax node, params SyntaxKind[] modifiers)
    {
        return node.AddModifiers(CreateModifiers(modifiers));
    }

    public static PropertyDeclarationSyntax WithModifiers(this PropertyDeclarationSyntax node, params SyntaxKind[] modifiers)
    {
        if (modifiers.Length == 0)
        {
            return node;
        }

        return node.AddModifiers(CreateModifiers(modifiers));
    }

    public static MemberAccessExpressionSyntax MemberAccess(String entityName, String memberName)
    {
        return MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            IdentifierName(entityName),
            IdentifierName(memberName));
    }

    public static String GetRouteName(this IParameterSymbol property)
    {
        return property.GetAttributes()
            .Where(attr => attr.AttributeClass.InheritsFrom(FullTypeNames.BindAttribute))
            .SelectMany(attr => attr.NamedArguments.Where(arg => arg.Key == "Prefix"))
            .Select(arg => arg.Value.Value as String)
            .Where(prefix => !String.IsNullOrEmpty(prefix))
            .DefaultIfEmpty(property.Name)
            .First();
    }
}
