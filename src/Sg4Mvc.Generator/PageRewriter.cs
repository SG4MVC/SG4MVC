using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sg4Mvc.Generator.Extensions;

namespace Sg4Mvc.Generator;

/// <summary>
/// Handles changes to non-generated mvc controller inheriting classes
/// </summary>
public class PageRewriter : CSharpSyntaxRewriter
{
    private readonly List<ClassDeclarationSyntax> _mvcPageClassNodes = new List<ClassDeclarationSyntax>();

    public PageRewriter(GeneratorExecutionContext context)
    {
        Context = context;
    }

    private GeneratorExecutionContext Context { get; }

    public ClassDeclarationSyntax[] MvcPageClassNodes => this._mvcPageClassNodes.ToArray();

    public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        // grab the symbol first and pass to other visitors first
        var symbol = Context.Compilation.GetSemanticModel(node.SyntaxTree).GetDeclaredSymbol(node);
        var newNode = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);
        if (PageShouldBeProcessed(symbol))
        {
            Console.WriteLine($"Processing page {symbol.ContainingNamespace}.{symbol.Name}");

            // hold a list of all controller classes to use later for the generator
            _mvcPageClassNodes.Add(node);

            if (!newNode.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                Debug.WriteLine("SG4MVC - Marking {0} class a partial", symbol);
                newNode = newNode.WithModifiers(SyntaxKind.PartialKeyword);
            }
        }

        return newNode;
    }

    internal static Boolean PageShouldBeProcessed(INamedTypeSymbol symbol)
        => symbol.DeclaredAccessibility == Accessibility.Public &&
            !symbol.IsAbstract &&
            symbol.InheritsFrom(FullTypeNames.PageModel) &&
            !symbol.GetAttributes().Any(a =>
                a.AttributeClass.InheritsFrom("Microsoft.AspNetCore.Mvc.SG4MvcExcludeAttribute"));

    public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);

        // only public methods not marked as virtual or override
        if (node.Modifiers.Any(SyntaxKind.PublicKeyword)
            && !node.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword) || m.IsKind(SyntaxKind.OverrideKeyword)))
        {
            var symbol = Context.Compilation.GetSemanticModel(node.SyntaxTree).GetDeclaredSymbol(node);

            if (PageShouldBeProcessed(symbol.ContainingType) && symbol.IsRazorPageAction() && symbol.IsNotSg4MvcExcluded())
            {
                Debug.WriteLine(
                    "SG4MVC - Marking controller method {0} as virtual from {1}",
                    symbol.ToString(),
                    symbol.ContainingType?.ToString());
                node = node.WithModifiers(SyntaxKind.VirtualKeyword);
            }
        }

        return node;
    }
}
