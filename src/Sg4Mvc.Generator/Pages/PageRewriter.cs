using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sg4Mvc.Generator.Extensions;

namespace Sg4Mvc.Generator.Pages;

public class PageRewriter : CSharpSyntaxRewriter
{
    public PageRewriter(GeneratorExecutionContext context)
    {
        Context = context;
    }

    private GeneratorExecutionContext Context { get; }

    public List<ClassDeclarationSyntax> MvcPageClassNodes { get; } = new();

    public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        // grab the symbol first and pass to other visitors first
        var symbol = Context.Compilation
            .GetSemanticModel(node.SyntaxTree)
            .GetDeclaredSymbol(node);

        var newNode = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

        if (PageShouldBeProcessed(symbol))
        {
            Console.WriteLine($"Processing page {symbol.ContainingNamespace}.{symbol.Name}");

            // hold a list of all controller classes to use later for the generator
            MvcPageClassNodes.Add(node);

            if (!newNode.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                Debug.WriteLine("SG4MVC - Marking {0} class a partial", symbol);
                newNode = newNode.WithModifiers(SyntaxKind.PartialKeyword);
            }
        }

        return newNode;
    }

    internal static Boolean PageShouldBeProcessed(INamedTypeSymbol symbol)
    {
        return symbol.DeclaredAccessibility == Accessibility.Public
               && !symbol.IsAbstract
               && symbol.InheritsFrom(FullTypeNames.PageModel)
               && !symbol.GetAttributes().Any(a =>
                   a.AttributeClass.InheritsFrom("Microsoft.AspNetCore.Mvc.Sg4MvcExcludeAttribute"));
    }
}
