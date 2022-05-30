using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Sg4Mvc.Generator.CodeGen;
using Xunit;

namespace Sg4Mvc.Test.CodeGen;

public class ConstructorMethodBuilderTests
{
    [Theory]
    [InlineData("ClassName")]
    [InlineData("Entity")]
    public void Constructor(String name)
    {
        var result = new ConstructorMethodBuilder(name)
            .Build();

        Assert.Equal(name + "(){}", result.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("p1")]
    [InlineData("p1", "p2")]
    public void Constructor_WithBaseConstructor(params String[] arguments)
    {
        if (arguments == null)
        {
            arguments = Array.Empty<String>();
        }

        var result = new ConstructorMethodBuilder("ClassName")
            .WithBaseConstructorCall(arguments.Select(a => SyntaxFactory.IdentifierName(a)).ToArray())
            .Build();

        Assert.Equal($"ClassName():base({String.Join(",", arguments)}){{}}", result.ToString());
    }
}
