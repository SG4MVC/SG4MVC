using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sg4Mvc.Generator.Extensions;
using Sg4Mvc.Generator.Services.Interfaces;

namespace Sg4Mvc.Generator.Services;

public class ControllerRewriterService : IControllerRewriterService
{
    public ControllerRewriterService(IControllerGeneratorService controllerGenerator)
    {
        ControllerGenerator = controllerGenerator;
    }

    private IControllerGeneratorService ControllerGenerator { get; }

    public IList<ControllerDefinition> RewriteControllers(GeneratorExecutionContext context)
    {
        var controllers = new Dictionary<String, ControllerDefinition>();

        foreach (var tree in context.Compilation.SyntaxTrees.Where(x => !x.FilePath.EndsWith(".generated.cs")))
        {
            // if syntaxtree has errors, skip code generation
            if (tree.GetDiagnostics().Any(x => x.Severity == DiagnosticSeverity.Error))
            {
                continue;
            }

            // this first part, finds all the controller classes, modifies them and saves the changes
            var controllerRewriter = new ControllerRewriter(context);
            SyntaxNode newNode;
            try
            {
                newNode = controllerRewriter.Visit(tree.GetRoot());
            }
            catch
            {
                // If roslyn can't get the root of the tree, just continue to the next item
                continue;
            }

            // save the controller nodes from each visit to pass to the generator
            foreach (var controllerNode in controllerRewriter.MvcControllerClassNodes)
            {
                var nameSpace = controllerNode.FirstAncestorOrSelf<NamespaceDeclarationSyntax>() as BaseNamespaceDeclarationSyntax
                    ?? controllerNode.FirstAncestorOrSelf<FileScopedNamespaceDeclarationSyntax>();

                var cNamespace = nameSpace.Name
                    .ToFullString()
                    .Trim();

                var cSymbol = context.Compilation.GetSemanticModel(tree).GetDeclaredSymbol(controllerNode);

                var cFullName = cNamespace + "." + cSymbol.Name;

                if (controllers.ContainsKey(cFullName))
                {
                    controllers[cFullName].FilePaths.Add(tree.FilePath);
                    continue;
                }

                var isSecure = cSymbol.GetAttributes()
                    .Any(a => a.AttributeClass.InheritsFrom(FullTypeNames.RequireHttpsAttribute));

                var cAreaName = ControllerGenerator.GetControllerArea(cSymbol);

                controllers[cFullName] = new ControllerDefinition
                {
                    Namespace = cNamespace,
                    Name = cSymbol.Name.TrimEnd("Controller"),
                    Area = cAreaName,
                    IsSecure = isSecure,
                    Symbol = cSymbol,
                    FilePaths = new List<String> { tree.FilePath },
                };
            }
        }

        return controllers.Values.ToList();
    }
}
