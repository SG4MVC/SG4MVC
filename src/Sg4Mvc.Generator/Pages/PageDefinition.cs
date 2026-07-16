using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Sg4Mvc.Generator.Controllers;

namespace Sg4Mvc.Generator.Pages;

public class PageDefinition : IEquatable<PageDefinition>
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

    public IList<String> FilePaths { get; set; }

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

    // --- Equatable snapshot fields (no Roslyn symbols) ---

    /// <summary>Original Symbol.Name, e.g. "IndexModel"</summary>
    public String SymbolName { get; set; }
    /// <summary>Symbol.ContainingNamespace.ToString()</summary>
    public String ContainingNamespace { get; set; }
    /// <summary>Type parameter names</summary>
    public IReadOnlyList<String> TypeParameterNames { get; set; } = Array.Empty<String>();
    /// <summary>Public non-generated constructors</summary>
    public IReadOnlyList<ConstructorData> PublicConstructors { get; set; } = Array.Empty<ConstructorData>();
    /// <summary>All public non-generated handler methods</summary>
    public IReadOnlyList<ActionMethodData> HandlerMethods { get; set; } = Array.Empty<ActionMethodData>();

    public bool Equals(PageDefinition other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Namespace == other.Namespace &&
               Name == other.Name &&
               IsSecure == other.IsSecure &&
               SymbolName == other.SymbolName &&
               ContainingNamespace == other.ContainingNamespace &&
               FilePaths.SequenceEqual(other.FilePaths) &&
               TypeParameterNames.SequenceEqual(other.TypeParameterNames) &&
               PublicConstructors.SequenceEqual(other.PublicConstructors) &&
               HandlerMethods.SequenceEqual(other.HandlerMethods);
    }

    public override bool Equals(object obj) => Equals(obj as PageDefinition);

    public override int GetHashCode()
    {
        unchecked
        {
            int h = 17;
            h = h * 31 + (Namespace?.GetHashCode() ?? 0);
            h = h * 31 + (Name?.GetHashCode() ?? 0);
            h = h * 31 + IsSecure.GetHashCode();
            h = h * 31 + (SymbolName?.GetHashCode() ?? 0);
            return h;
        }
    }
}

