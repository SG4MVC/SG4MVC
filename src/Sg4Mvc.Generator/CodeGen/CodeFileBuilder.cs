﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sg4Mvc.Generator.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Sg4Mvc.Generator.CodeGen;

public class CodeFileBuilder
{
    private static readonly String[] _pragmaCodes = { "1591", "3008", "3009", "0108" };
    private const String _headerText =
        @"// <auto-generated />
// This file was generated by SG4MVC.

// Make sure the compiler doesn't complain about missing Xml comments and CLS compliance
// 0108: suppress ""Foo hides inherited member Foo.Use the new keyword if hiding was intended."" when a controller and its abstract parent are both processed";


    private CompilationUnitSyntax _compilationUnit;
    private readonly Settings _settings;
    private readonly Boolean _autoGenerated;

    public CodeFileBuilder(Settings settings, Boolean autoGenerated)
    {
        _settings = settings;
        _autoGenerated = autoGenerated;
        var usings = new List<String>
        {
            "System",
            "System.CodeDom.Compiler",
            "System.Diagnostics",
            "System.Threading.Tasks",
            "Microsoft.AspNetCore.Mvc",
            "Microsoft.AspNetCore.Mvc.RazorPages",
            "Microsoft.AspNetCore.Routing",
            settings.Sg4MvcNamespace,
        };
        if (settings.ReferencedNamespaces != null)
        {
            usings.AddRange(settings.ReferencedNamespaces);
        }

        _compilationUnit = CompilationUnit()
            .WithUsings(List(usings.Select(u => UsingDirective(ParseName(u)))));

        if (_autoGenerated)
        {
            var headTrivia = _compilationUnit.GetLeadingTrivia()
                .Add(Comment(_headerText))
                .Add(CarriageReturnLineFeed)
                .Add(GetPragmaCodes(SyntaxKind.DisableKeyword));
            _compilationUnit = _compilationUnit.WithLeadingTrivia(headTrivia);
        }
    }

    private SyntaxTrivia GetPragmaCodes(SyntaxKind syntaxKind)
    {
        List<String> pragmaCodes = _pragmaCodes.ToList();

        if (_settings.PragmaCodes != null)
        {
            pragmaCodes.AddRange(_settings.PragmaCodes);
        }

        return Trivia(
            PragmaWarningDirectiveTrivia(
                Token(syntaxKind),
                SeparatedList(pragmaCodes.Select(p => ParseExpression(p))),
                true));
    }

    public CodeFileBuilder WithNamespace(NamespaceDeclarationSyntax @namespace)
    {
        _compilationUnit = _compilationUnit.AddMembers(@namespace);
        return this;
    }

    public CodeFileBuilder WithNamespaces(IEnumerable<NamespaceDeclarationSyntax> namespaces)
    {
        foreach (var ns in namespaces)
            _compilationUnit = _compilationUnit.AddMembers(ns);
        return this;
    }

    public CodeFileBuilder WithMembers(params MemberDeclarationSyntax[] members)
    {
        _compilationUnit = _compilationUnit.AddMembers(members);
        return this;
    }

    public CodeFileBuilder WithMembers(Boolean condition, params MemberDeclarationSyntax[] members)
    {
        if (condition)
        {
            return WithMembers(members);
        }

        return this;
    }

    public CompilationUnitSyntax Build()
    {
        if (_autoGenerated)
        {
            var endTrivia = _compilationUnit.GetTrailingTrivia()
                .Add(ElasticCarriageReturnLineFeed)
                .Add(GetPragmaCodes(SyntaxKind.RestoreKeyword));

            _compilationUnit = _compilationUnit.WithTrailingTrivia(endTrivia);
        }

        return _compilationUnit.NormalizeWhitespace();
    }

    public void WriteToFile(SourceProductionContext context, String fileName)
    {
        context.WriteFile(this.Build(), fileName);
    }
}
