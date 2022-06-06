using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sg4Mvc.Generator.Extensions;

public static class SimpleExtensions
{
    public static void ReportAndRestart(this Stopwatch sw, SourceProductionContext spc, String description)
    {
        sw.StopAndReport(spc, description);
        sw.Restart();
    }

    public static void StopAndReport(this Stopwatch sw, SourceProductionContext spc, String description)
    {
        sw.Stop();

        //spc.ReportDiagnostic(Diagnostic.Create(
        //    "SG4PERF",
        //    "SG4MVC",
        //    $"{description} generation time {sw.ElapsedMilliseconds}ms ({sw.ElapsedTicks} ticks)",
        //    DiagnosticSeverity.Warning,
        //    DiagnosticSeverity.Warning,
        //    true,
        //    1));
    }

    public static String GetWorkingDirectory(this AnalyzerConfigOptionsProvider optionsProvider)
    {
        optionsProvider.GlobalOptions
            .TryGetValue("build_property.MSBuildProjectDirectory", out var projectDirectory);

        return projectDirectory;
    }

    public static String TrimEnd(this String value, String suffix)
    {
        if (value == null)
        {
            return null;
        }

        if (!value.EndsWith(suffix))
        {
            return value;
        }

        return value.Substring(0, value.Length - suffix.Length);
    }

    public static String SanitiseFieldName(this String name)
    {
        name = Regex.Replace(name, @"[\W\b]", "_", RegexOptions.IgnoreCase);
        name = Regex.Replace(name, @"^\d", @"_$0");

        Int32 i = 0;
        while (SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ||
            SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None ||
            !SyntaxFacts.IsValidIdentifier(name))
        {
            if (i++ > 10)
            {
                return name; // Sanity check.. The loop might be loopy!
            }

            name = "_" + name;
        }
        return name;
    }

    public static String GetRelativePath(this String path, String rootPath)
    {
        path = Path.GetFullPath(path);
        rootPath = Path.GetFullPath(rootPath);
        if (!path.StartsWith(rootPath))
        {
            return path;
        }

        return path.Substring(rootPath.Length);
    }
}
