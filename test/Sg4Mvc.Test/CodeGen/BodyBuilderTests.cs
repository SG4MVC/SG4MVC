using System;
using Sg4Mvc.Generator.CodeGen;
using Xunit;

namespace Sg4Mvc.Test.CodeGen;

public class BodyBuilderTests
{
    [Fact]
    public void BodyEmpty()
    {
        var result = new BodyBuilder()
            .Build();

        Assert.Equal("{}", result.ToString());
    }

    [Theory]
    [InlineData("result", "ActionResult", "controller")]
    [InlineData("result", "ActionResult", "controller", "action")]
    [InlineData("obj", "object")]
    public void Body_WithVariableFromObject(String name, String type, params String[] arguments)
    {
        var result = new BodyBuilder()
            .VariableFromNewObject(name, type, arguments)
            .Build();

        Assert.Equal($"{{var{name}=new{type}({String.Join(",", arguments)});}}", result.ToString());
    }

    [Theory]
    [InlineData("result", "action", "ToString")]
    [InlineData("obj", "value", "GetValues", "param1")]
    [InlineData("obj", "value", "GetValues", "param1", "param2")]
    public void Body_WithVariableFromMethod(String name, String entity, String method, params String[] arguments)
    {
        var result = new BodyBuilder()
            .VariableFromMethodCall(name, entity, method, arguments)
            .Build();

        Assert.Equal($"{{var{name}={entity}.{method}({String.Join(",", arguments)});}}", result.ToString());
    }

    [Theory]
    [InlineData("result")]
    [InlineData("value")]
    public void Body_ReturnsVariable(String name)
    {
        var result = new BodyBuilder()
            .ReturnVariable(name)
            .Build();

        Assert.Equal($"{{return{name};}}", result.ToString());
    }

    [Theory]
    [InlineData("ActionResult", "controller")]
    [InlineData("ActionResult", "controller", "action")]
    [InlineData("object")]
    public void Body_ReturnsNewObject(String type, params String[] arguments)
    {
        var result = new BodyBuilder()
            .ReturnNewObject(type, arguments)
            .Build();

        Assert.Equal($"{{returnnew{type}({String.Join(",", arguments)});}}", result.ToString());
    }

    [Theory]
    [InlineData("action", "ToUrl", "controller")]
    [InlineData("action", "ToUrl", "controller", "action")]
    [InlineData("entity", "ToString")]
    public void Body_ReturnsMethodCall(String entity, String method, params String[] arguments)
    {
        var result = new BodyBuilder()
            .ReturnMethodCall(entity, method, arguments)
            .Build();

        Assert.Equal($"{{return{entity}.{method}({String.Join(",", arguments)});}}", result.ToString());
    }

    [Theory]
    [InlineData("action", "ToUrl", "controller")]
    [InlineData("action", "ToUrl", "controller", "action")]
    [InlineData("entity", "ToString")]
    public void Body_MethodCall(String entity, String method, params String[] arguments)
    {
        var result = new BodyBuilder()
            .MethodCall(entity, method, arguments)
            .Build();

        Assert.Equal($"{{{entity}.{method}({String.Join(",", arguments)});}}", result.ToString());
    }

    [Fact]
    public void Body_MultiMethods()
    {
        var result = new BodyBuilder()
            .VariableFromNewObject("result", "object")
            .ReturnVariable("result")
            .Build();

        Assert.Equal("{varresult=newobject();returnresult;}", result.ToString());
    }

    [Fact]
    public void Body_Statement()
    {
        var result = new BodyBuilder()
            .Statement(b => b
                .VariableFromNewObject("result", "object")
                .ReturnVariable("result"))
            .Build();

        Assert.Equal("{varresult=newobject();returnresult;}", result.ToString());
    }
}
