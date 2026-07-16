using System;
using System.Collections.Generic;
using System.Linq;

namespace Sg4Mvc.Generator.Controllers;

/// <summary>
/// Equatable snapshot of a method parameter — no Roslyn symbols.
/// </summary>
public sealed class ParameterData : IEquatable<ParameterData>
{
    public string Name { get; }
    public string TypeFullName { get; }
    public string RoutePrefix { get; }   // from [Bind(Prefix="...")] if present

    public ParameterData(string name, string typeFullName, string routePrefix)
    {
        Name = name;
        TypeFullName = typeFullName;
        RoutePrefix = routePrefix;
    }

    public bool Equals(ParameterData other) =>
        other != null &&
        Name == other.Name &&
        TypeFullName == other.TypeFullName &&
        RoutePrefix == other.RoutePrefix;

    public override bool Equals(object obj) => Equals(obj as ParameterData);
    public override int GetHashCode() => HashCode(Name, TypeFullName, RoutePrefix);

    private static int HashCode(params string[] values)
    {
        unchecked
        {
            int h = 17;
            foreach (var v in values)
                h = h * 31 + (v?.GetHashCode() ?? 0);
            return h;
        }
    }
}

/// <summary>
/// Equatable snapshot of a public method — no Roslyn symbols.
/// </summary>
public sealed class ActionMethodData : IEquatable<ActionMethodData>
{
    public string Name { get; }
    public string ReturnTypeFullName { get; }
    public bool IsSecure { get; }
    public IReadOnlyList<ParameterData> Parameters { get; }

    public ActionMethodData(string name, string returnTypeFullName, bool isSecure, IReadOnlyList<ParameterData> parameters)
    {
        Name = name;
        ReturnTypeFullName = returnTypeFullName;
        IsSecure = isSecure;
        Parameters = parameters ?? Array.Empty<ParameterData>();
    }

    public bool Equals(ActionMethodData other)
    {
        if (other == null) return false;
        return Name == other.Name &&
               ReturnTypeFullName == other.ReturnTypeFullName &&
               IsSecure == other.IsSecure &&
               Parameters.Count == other.Parameters.Count &&
               Parameters.SequenceEqual(other.Parameters);
    }

    public override bool Equals(object obj) => Equals(obj as ActionMethodData);
    public override int GetHashCode()
    {
        unchecked
        {
            int h = 17;
            h = h * 31 + (Name?.GetHashCode() ?? 0);
            h = h * 31 + (ReturnTypeFullName?.GetHashCode() ?? 0);
            h = h * 31 + IsSecure.GetHashCode();
            foreach (var p in Parameters) h = h * 31 + p.GetHashCode();
            return h;
        }
    }
}

/// <summary>
/// Equatable snapshot of a constructor — no Roslyn symbols.
/// </summary>
public sealed class ConstructorData : IEquatable<ConstructorData>
{
    public IReadOnlyList<string> ParameterTypes { get; }

    public ConstructorData(IReadOnlyList<string> parameterTypes)
    {
        ParameterTypes = parameterTypes ?? Array.Empty<string>();
    }

    public bool Equals(ConstructorData other) =>
        other != null && ParameterTypes.SequenceEqual(other.ParameterTypes);

    public override bool Equals(object obj) => Equals(obj as ConstructorData);
    public override int GetHashCode()
    {
        unchecked
        {
            int h = 17;
            foreach (var t in ParameterTypes) h = h * 31 + (t?.GetHashCode() ?? 0);
            return h;
        }
    }
}
