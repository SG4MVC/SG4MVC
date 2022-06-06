using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Sg4Mvc.Generator.Controllers;

[DebuggerDisplay("Controller: {Area} {Name}")]
public class ControllerDefinition
{
    public String Namespace { get; set; }
    public String Name { get; set; }
    public String Area { get; set; }
    public Boolean IsSecure { get; set; }
    public INamedTypeSymbol Symbol { get; set; }

    public String AreaKey { get; set; }

    public IList<View> Views { get; set; } = new List<View>();

    private String _fullyQualifiedGeneratedName = null;

    public String FullyQualifiedGeneratedName
    {
        get => _fullyQualifiedGeneratedName ?? $"{Namespace}.{Name}Controller";
        set => _fullyQualifiedGeneratedName = value;
    }

    public String FullyQualifiedSg4ClassName { get; set; }
}
