using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Sg4Mvc.Generator.Pages;

public class PageDefinition(String cNamespace,
    String name,
    Boolean isSecure,
    INamedTypeSymbol symbol,
    List<String> filePaths)
{
    public String Namespace { get; } = cNamespace;
    public String Name { get; } = name;
    public Boolean IsSecure { get; } = isSecure;
    public INamedTypeSymbol Symbol { get; } = symbol;

    public IList<String> FilePaths { get; set; } = filePaths;

    private String _fullyQualifiedGeneratedName = null;
    public String FullyQualifiedGeneratedName
    {
        get => _fullyQualifiedGeneratedName ?? $"{Namespace}.{Name}Model";
        set => _fullyQualifiedGeneratedName = value;
    }

    public String FullyQualifiedSg4ClassName { get; set; }

    public String GetFilePath()
    {
        return FilePaths
            .OrderByDescending(f => f.EndsWith(".cshtml.cs") && File.Exists(f.Substring(0, f.Length - 3)))
            .ThenByDescending(f => !f.Contains(".generated.cs"))
            .ThenBy(f => f)
            .FirstOrDefault();
    }
}
