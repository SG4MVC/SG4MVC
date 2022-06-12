using Sg4Mvc.Generator.Controllers;
using Xunit;

namespace Sg4Mvc.Test;

public class ControllerDefinitionTests
{
    [Fact]
    public void FullyQualifiedName_Default()
    {
        var controller = new ControllerDefinition
        {
            Namespace = "Project.Root",
            Name = "Home",
        };
        Assert.Equal($"{controller.Namespace}.{controller.Name}Controller", controller.FullyQualifiedGeneratedName);
    }

    [Fact]
    public void FullyQualifiedName_Custom()
    {
        var customName = "Project.Root.Controllers.HomeController";
        var controller = new ControllerDefinition
        {
            Namespace = "Project.Root",
            Name = "Home",
            FullyQualifiedGeneratedName = customName,
        };
        Assert.Equal(customName, controller.FullyQualifiedGeneratedName);
    }

    [Fact]
    public void Views_NotNull() => Assert.NotNull(new ControllerDefinition().Views);
}
