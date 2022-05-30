using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sg4Mvc.Generator.CodeGen;
using Sg4Mvc.Generator.Extensions;
using Sg4Mvc.Generator.Locators;
using Sg4Mvc.Generator.Services.Interfaces;

namespace Sg4Mvc.Generator.Services;

public class StaticFileGeneratorService : IStaticFileGeneratorService
{
    private readonly IEnumerable<IStaticFileLocator> _staticFileLocators;
    private readonly Settings _settings;

    public StaticFileGeneratorService(IEnumerable<IStaticFileLocator> staticFileLocators, Settings settings)
    {
        _staticFileLocators = staticFileLocators;
        _settings = settings;
    }

    public MemberDeclarationSyntax GenerateStaticFiles(String projectRoot)
    {
        var staticFilesRoot = GetStaticFilesPath(projectRoot);
        var staticfiles = _staticFileLocators.SelectMany(x => x.Find(staticFilesRoot));

        var linksClass = new ClassBuilder(_settings.LinksNamespace)
            .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.PartialKeyword)
            .WithGeneratedNonUserCodeAttributes();
        AddUrlFields(linksClass, String.Empty);
        AddStaticFiles(projectRoot, linksClass, String.Empty, staticfiles);
        return linksClass.Build();
    }

    // This will eventually read the Startup class, to identify the location(s) of the static roots
    public String GetStaticFilesPath(String projectRoot) => Path.Combine(projectRoot, _settings.StaticFilesPath);

    private void AddUrlFields(ClassBuilder builder, String path)
    {
        builder
            .WithStringField("UrlPath", "~" + path, SyntaxKind.PublicKeyword, SyntaxKind.ConstKeyword)
            .WithMethod("Url", "string", m => m
                .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                .WithExpresisonBody(BodyBuilder.MethodCallExpression(Constants.Sg4MvcHelpersClass, Constants.Sg4MvcHelpers_ProcessVirtualPath, new[] { "UrlPath" })))
            .WithMethod("Url", "string", m => m
                .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                .WithParameter("fileName", "string")
                .WithExpresisonBody(BodyBuilder.MethodCallExpression(Constants.Sg4MvcHelpersClass, Constants.Sg4MvcHelpers_ProcessVirtualPath, new[] { "UrlPath + \"/\" + fileName" })));
    }

    public void AddStaticFiles(String projectRoot, ClassBuilder parentClass, String path, IEnumerable<StaticFile> files)
    {
        var paths = files
            .Select(f => f.Container)
            .Distinct()
            .OrderBy(p => p)
            .Where(c => c.StartsWith(path) && c.Length > path.Length)
            .Select(c =>
            {
                var index = c.IndexOf('/', path.Length > 0 ? path.Length + 1 : 0);
                if (index == -1)
                {
                    return c;
                }

                return c.Substring(0, index);
            })
            .Distinct();

        foreach (var childPath in paths)
        {
            var childFiles = files.Where(f => f.Container.StartsWith(childPath));
            var className = childPath.Substring(path.Length > 0 ? path.Length + 1 : 0).SanitiseFieldName();
            parentClass.WithChildClass(className, containerClass =>
            {
                containerClass.WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.PartialKeyword);
                AddUrlFields(containerClass, Path.Combine(projectRoot, childPath).GetRelativePath(projectRoot).Replace('\\', '/'));
                AddStaticFiles(projectRoot, containerClass, childPath, childFiles);
            });
        }

        var localFiles = files.Where(f => f.Container == path);
        foreach (var file in localFiles)
        {
            var fieldName = file.FileName.SanitiseFieldName();
            if (fieldName == parentClass.Name)
            {
                fieldName += "_";
            }

            parentClass.WithValueField(fieldName, "string", $"Url(\"{file.FileName}\")", SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword);
        }
    }
}
