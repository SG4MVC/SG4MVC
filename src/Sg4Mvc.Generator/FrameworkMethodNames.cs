using System;
using System.Linq;

namespace Sg4Mvc.Generator;

/// <summary>
/// Holds the virtual public method names from ASP.NET Core base classes.
/// Extracted once from the Compilation and cached by Roslyn's incremental pipeline.
/// Implements IEquatable so Roslyn can cache this node across compilations.
/// </summary>
public sealed class FrameworkMethodNames : IEquatable<FrameworkMethodNames>
{
    public String[] ControllerMethods { get; }
    public String[] PageMethods { get; }

    public FrameworkMethodNames(String[] controllerMethods, String[] pageMethods)
    {
        ControllerMethods = controllerMethods ?? Array.Empty<String>();
        PageMethods = pageMethods ?? Array.Empty<String>();
    }

    public bool Equals(FrameworkMethodNames other) =>
        other != null &&
        ControllerMethods.SequenceEqual(other.ControllerMethods) &&
        PageMethods.SequenceEqual(other.PageMethods);

    public override bool Equals(object obj) => Equals(obj as FrameworkMethodNames);

    public override int GetHashCode()
    {
        unchecked
        {
            int h = 17;
            foreach (var m in ControllerMethods) h = h * 31 + (m?.GetHashCode() ?? 0);
            foreach (var m in PageMethods) h = h * 31 + (m?.GetHashCode() ?? 0);
            return h;
        }
    }
}
