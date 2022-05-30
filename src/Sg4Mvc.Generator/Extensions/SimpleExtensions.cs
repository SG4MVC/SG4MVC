using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Sg4Mvc.Generator.Extensions;

public static class SimpleExtensions
{
    public static String GetWorkingDirectory(this GeneratorExecutionContext context)
    {
        context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.MSBuildProjectDirectory", out var projectDirectory);

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
