using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace Sg4Mvc.Test.CodeGen;

public static class ModifierHelpers
{
    public static String ToStringModifiers(this SyntaxKind[] modifiers)
    {
        if (modifiers == null || modifiers.Length == 0)
        {
            return null;
        }

        return String.Join("", modifiers.Select(m => ToStringModifier(m)));
    }

    public static String ToStringModifier(this SyntaxKind modifier)
    {
        var result = modifier.ToString().ToLowerInvariant();
        if (!result.EndsWith("keyword"))
        {
            throw new InvalidOperationException("Bad modifier: " + modifier);
        }

        return result.Substring(0, result.Length - "keyword".Length) + " ";
    }
}
