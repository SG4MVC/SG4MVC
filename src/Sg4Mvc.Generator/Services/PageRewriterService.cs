using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sg4Mvc.Generator.Extensions;
using Sg4Mvc.Generator.Services.Interfaces;

namespace Sg4Mvc.Generator.Services;

public class PageRewriterService : IPageRewriterService
{
    private readonly IFilePersistService _filePersistService;
    private readonly Settings _settings;
    public PageRewriterService(IFilePersistService filePersistService, Settings settings)
    {
        _filePersistService = filePersistService;
        _settings = settings;
    }

    public IList<PageDefinition> RewritePages(GeneratorExecutionContext context)
    {
        var pages = new Dictionary<String, PageDefinition>();

        foreach (var tree in context.Compilation.SyntaxTrees.Where(x => !x.FilePath.EndsWith(".generated.cs")))
        {
            // if syntaxtree has errors, skip code generation
            if (tree.GetDiagnostics().Any(x => x.Severity == DiagnosticSeverity.Error))
            {
                continue;
            }

            // this first part, finds all the page classes, modifies them and saves the changes
            var pageRewriter = new PageRewriter(context);
            SyntaxNode newNode;
            try
            {
                newNode = pageRewriter.Visit(tree.GetRoot());
            }
            catch
            {
                // If roslyn can't get the root of the tree, just continue to the next item
                continue;
            }

            // save the page nodes from each visit to pass to the generator
            foreach (var pageNode in pageRewriter.MvcPageClassNodes)
            {
                var nameSpace = pageNode.FirstAncestorOrSelf<NamespaceDeclarationSyntax>() as BaseNamespaceDeclarationSyntax
                    ?? pageNode.FirstAncestorOrSelf<FileScopedNamespaceDeclarationSyntax>();

                var cNamespace = nameSpace.Name.ToFullString().Trim();

                var cSymbol = context.Compilation.GetSemanticModel(tree).GetDeclaredSymbol(pageNode);

                var cFullName = cNamespace + "." + cSymbol.Name;

                if (pages.ContainsKey(cFullName))
                {
                    pages[cFullName].FilePaths.Add(tree.FilePath);
                    continue;
                }

                var isSecure = cSymbol.GetAttributes()
                    .Any(a => a.AttributeClass.InheritsFrom(FullTypeNames.RequireHttpsAttribute));

                pages[cFullName] = new PageDefinition(
                    cNamespace,
                    cSymbol.Name.TrimEnd("Model"),
                    isSecure,
                    cSymbol,
                    new List<String> { tree.FilePath });
            }
        }

        return pages.Values.ToList();
    }
}
