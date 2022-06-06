using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sg4Mvc.Generator.Extensions;

namespace Sg4Mvc.Generator.Controllers;

public class ControllerRewriter : CSharpSyntaxRewriter
{
    public ControllerRewriter(GeneratorExecutionContext context)
    {
        Context = context;
    }

    private GeneratorExecutionContext Context { get; }

    public List<CompilationDetails> MvcControllerClassNodes { get; } = new();

    public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        // grab the symbol first and pass to other visitors first
        var symbol = Context.Compilation
            .GetSemanticModel(node.SyntaxTree)
            .GetDeclaredSymbol(node);

        var newNode = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

        if (ControllerShouldBeProcessed(symbol))
        {
            Console.WriteLine($"Processing controller {symbol.ContainingNamespace}.{symbol.Name}");

            // hold a list of all controller classes to use later for the generator
            //MvcControllerClassNodes.Add(new CompilationDetails(newNode, symbol));
        }

        return newNode;
    }

    internal static Boolean ControllerShouldBeProcessed(INamedTypeSymbol symbol)
    {
        return symbol.InheritsFrom("Microsoft.AspNetCore.Mvc.ControllerBase")
               && !symbol.GetAttributes().Any(a => a.AttributeClass.InheritsFrom("Microsoft.AspNetCore.Mvc.Sg4MvcExcludeAttribute"));
    }
}
