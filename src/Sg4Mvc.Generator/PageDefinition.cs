using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Sg4Mvc.Generator;

public class PageDefinition
{
    public PageDefinition(String cNamespace, String name, Boolean isSecure, INamedTypeSymbol symbol, List<String> filePaths)
    {
        Namespace = cNamespace;
        Name = name;
        IsSecure = isSecure;
        Symbol = symbol;
        FilePaths = filePaths;
    }

    public String Namespace { get; }
    public String Name { get; }
    public Boolean IsSecure { get; }
    public INamedTypeSymbol Symbol { get; }

    public IList<String> FilePaths = new List<String>();

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
