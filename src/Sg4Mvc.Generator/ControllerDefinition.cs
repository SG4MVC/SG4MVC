using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Sg4Mvc.Generator;

[DebuggerDisplay("Controller: {Area} {Name}")]
public class ControllerDefinition
{
    public String Namespace { get; set; }
    public String Name { get; set; }
    public String Area { get; set; }
    public Boolean IsSecure { get; set; }
    public INamedTypeSymbol Symbol { get; set; }

    public String AreaKey { get; set; }
    public IList<String> FilePaths = new List<String>();
    public IList<View> Views { get; set; } = new List<View>();

    private String _fullyQualifiedGeneratedName = null;
    public String FullyQualifiedGeneratedName
    {
        get => _fullyQualifiedGeneratedName ?? $"{Namespace}.{Name}Controller";
        set => _fullyQualifiedGeneratedName = value;
    }
    public String FullyQualifiedSg4ClassName { get; set; }

    public String GetFilePath()
    {
        return FilePaths
            .OrderByDescending(f => Area == null || f.Contains($"/Areas/{Area}/Controllers/") || f.Contains($"\\Areas\\{Area}\\Controllers\\"))
            .ThenByDescending(f => f.Contains("/Controllers/") || f.Contains("\\Controllers\\"))
            .ThenByDescending(f => !f.Contains(".generated.cs"))
            .ThenBy(f => f)
            .First();
    }
}
