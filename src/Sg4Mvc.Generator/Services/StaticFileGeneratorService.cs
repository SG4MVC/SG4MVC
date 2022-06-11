using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private readonly IStaticFileLocator _staticFileLocator;
    private readonly Settings _settings;

    public StaticFileGeneratorService(IStaticFileLocator staticFileLocator, Settings settings)
    {
        _staticFileLocator = staticFileLocator;
        _settings = settings;
    }

    public MemberDeclarationSyntax GenerateStaticFiles(String projectRoot)
    {
        var staticFilesRoot = GetStaticFilesPath(projectRoot);
        Logging.ReportProgress("GetStaticFilesPath");

        var staticfiles = _staticFileLocator.Find(staticFilesRoot);
        Logging.ReportProgress("_staticFileLocator.Find");

        var linksClass = new ClassBuilder(_settings.LinksNamespace)
            .WithModifiers(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.PartialKeyword)
            .WithGeneratedNonUserCodeAttributes();
        Logging.ReportProgress("create linksClass");

        AddUrlFields(linksClass, String.Empty);
        Logging.ReportProgress("AddUrlFields");

        using (new PerformanceLogger("Total AddStaticFiles"))
        {
            AddStaticFiles(projectRoot, linksClass, String.Empty, staticfiles);
        }

        Logging.ReportStopwatch("CreatePathsTotal", CreatePathsTotal);
        Logging.ReportStopwatch("PrepareNamesTotal", PrepareNamesTotal);
        Logging.ReportStopwatch("WithModifiersTotal", WithModifiersTotal);
        Logging.ReportStopwatch("AddUrlFieldsTotal", AddUrlFieldsTotal);
        Logging.ReportStopwatch("AddValueFieldsTotal", AddValueFieldsTotal);
        Logging.ReportStopwatch("SanitiseFieldNameTotal", SanitiseFieldNameTotal);
        Logging.ReportStopwatch("WithValueFieldTotal", WithValueFieldTotal);

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

    public static Stopwatch CreatePathsTotal = new();
    public static Stopwatch PrepareNamesTotal = new();
    public static Stopwatch WithModifiersTotal = new();
    public static Stopwatch AddUrlFieldsTotal = new();
    public static Stopwatch AddValueFieldsTotal = new();
    public static Stopwatch SanitiseFieldNameTotal = new();
    public static Stopwatch WithValueFieldTotal = new();

    public void AddStaticFiles(String projectRoot,
        ClassBuilder parentClass,
        String path,
        List<StaticFile> files)
    {
        CreatePathsTotal.Start();
        var paths = files.Select(f => f.Container).Distinct().OrderBy(p => p)
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
        CreatePathsTotal.Stop();

        foreach (var childPath in paths)
        {
            PrepareNamesTotal.Start();
            var childFiles = files
                .Where(f => f.Container.StartsWith(childPath))
                .ToList();

            var className = childPath
                .Substring(path.Length > 0 ? path.Length + 1 : 0)
                .SanitiseFieldName();
            PrepareNamesTotal.Stop();

            parentClass.WithChildClass(className, containerClass =>
            {
                WithModifiersTotal.Start();
                containerClass.WithModifiers(
                    SyntaxKind.PublicKeyword,
                    SyntaxKind.StaticKeyword,
                    SyntaxKind.PartialKeyword);
                WithModifiersTotal.Stop();

                AddUrlFieldsTotal.Start();
                AddUrlFields(containerClass, Path.Combine(projectRoot, childPath)
                    .GetRelativePath(projectRoot).Replace('\\', '/'));
                AddUrlFieldsTotal.Stop();

                AddStaticFiles(projectRoot, containerClass, childPath, childFiles);
            });
        }

        var localFiles = files.Where(f => f.Container == path);

        AddValueFieldsTotal.Start();
        foreach (var file in localFiles)
        {
            SanitiseFieldNameTotal.Start();
            var fieldName = file.FileName.SanitiseFieldName();
            SanitiseFieldNameTotal.Stop();

            WithValueFieldTotal.Start();
            if (fieldName == parentClass.Name)
            {
                fieldName += "_";
            }

            parentClass.WithValueField(fieldName,
                "string",
                $"Url(\"{file.FileName}\")",
                SyntaxKind.PublicKeyword,
                SyntaxKind.StaticKeyword,
                SyntaxKind.ReadOnlyKeyword);
            WithValueFieldTotal.Stop();
        }
        AddValueFieldsTotal.Stop();
    }
}
