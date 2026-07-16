using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Sg4Mvc.Generator.Controllers;

[DebuggerDisplay("Controller: {Area} {Name}")]
public class ControllerDefinition : IEquatable<ControllerDefinition>
{
    public String Namespace { get; set; }
    public String Name { get; set; }
    public String Area { get; set; }
    public Boolean IsSecure { get; set; }
    public INamedTypeSymbol Symbol { get; set; }

    public String AreaKey { get; set; }

    public List<View> Views { get; set; } = [];

    private String _fullyQualifiedGeneratedName = null;

    public String FullyQualifiedGeneratedName
    {
        get => _fullyQualifiedGeneratedName ?? $"{Namespace}.{Name}Controller";
        set => _fullyQualifiedGeneratedName = value;
    }

    public String FullyQualifiedSg4ClassName { get; set; }

    // --- Equatable snapshot fields (no Roslyn symbols) ---
    // These are populated by the transform step and used by Roslyn to determine cache validity.

    /// <summary>Original Symbol.Name, e.g. "SearchController"</summary>
    public String SymbolName { get; set; }
    /// <summary>Symbol.ContainingNamespace.ToString()</summary>
    public String ContainingNamespace { get; set; }
    /// <summary>Type parameter names, e.g. ["T"] for a generic controller</summary>
    public IReadOnlyList<String> TypeParameterNames { get; set; } = Array.Empty<String>();
    /// <summary>Public non-generated constructors, by their parameter type strings</summary>
    public IReadOnlyList<ConstructorData> PublicConstructors { get; set; } = Array.Empty<ConstructorData>();
    /// <summary>All public non-generated action methods with full signature data</summary>
    public IReadOnlyList<ActionMethodData> ActionMethods { get; set; } = Array.Empty<ActionMethodData>();

    public bool Equals(ControllerDefinition other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Namespace == other.Namespace &&
               Name == other.Name &&
               Area == other.Area &&
               IsSecure == other.IsSecure &&
               SymbolName == other.SymbolName &&
               ContainingNamespace == other.ContainingNamespace &&
               TypeParameterNames.SequenceEqual(other.TypeParameterNames) &&
               PublicConstructors.SequenceEqual(other.PublicConstructors) &&
               ActionMethods.SequenceEqual(other.ActionMethods);
    }

    public override bool Equals(object obj) => Equals(obj as ControllerDefinition);

    public override int GetHashCode()
    {
        unchecked
        {
            int h = 17;
            h = h * 31 + (Namespace?.GetHashCode() ?? 0);
            h = h * 31 + (Name?.GetHashCode() ?? 0);
            h = h * 31 + (Area?.GetHashCode() ?? 0);
            h = h * 31 + IsSecure.GetHashCode();
            h = h * 31 + (SymbolName?.GetHashCode() ?? 0);
            return h;
        }
    }
}

