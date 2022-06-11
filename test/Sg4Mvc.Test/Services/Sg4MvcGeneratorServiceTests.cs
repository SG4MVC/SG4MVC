using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Sg4Mvc.Generator;
using Sg4Mvc.Generator.Controllers;
using Sg4Mvc.Generator.Controllers.Interfaces;
using Sg4Mvc.Generator.Pages.Interfaces;
using Sg4Mvc.Generator.Services;
using Sg4Mvc.Generator.Services.Interfaces;
using Xunit;

namespace Sg4Mvc.Test.Services;

public class Sg4MvcGeneratorServiceTests
{
    private Sg4MvcGeneratorService GetGeneratorService(
        IControllerGeneratorService controllerGenerator = null,
        IPageGeneratorService pageGenerator = null,
        IStaticFileGeneratorService staticFileGenerator = null,
        Settings settings = null,
        SourceProductionContext? context = null
    )
    {
        settings ??= new Settings();

        controllerGenerator ??= new ControllerGeneratorService(settings);

        context ??= new SourceProductionContext();

        return new Sg4MvcGeneratorService(controllerGenerator, pageGenerator, staticFileGenerator, settings, context.Value);
    }

    [Fact]
    public void ViewControllers()
    {
        var controllers = new[]
        {
            new ControllerDefinition { Name = "Shared" },                           // Root view only controller
            new ControllerDefinition { Name = "Shared", Area = "Admin" },           // Area view only controller
            new ControllerDefinition { Name = "Shared", Namespace = "Project" },    // Regular controller (should be ignored here)
        };
        var settings = new Settings();
        var service = GetGeneratorService(settings: settings);

        var viewControllers = service.CreateViewOnlyControllerClasses(controllers).ToList();
        Assert.Collection(viewControllers,
            c => Assert.Equal("SharedController", c.Identifier.Value),
            c => Assert.Equal("AdminArea_SharedController", c.Identifier.Value));
        Assert.Collection(controllers,
            c => Assert.StartsWith(settings.Sg4MvcNamespace, c.FullyQualifiedGeneratedName),
            c => Assert.StartsWith(settings.Sg4MvcNamespace, c.FullyQualifiedGeneratedName),
            c => Assert.StartsWith("Project", c.FullyQualifiedGeneratedName)); // Don't update this field for regular controllers in this method
    }

    [Fact]
    public void ViewControllers_Sort()
    {
        var controllers = new[]
        {
            new ControllerDefinition { Name = "Shared", Area = "Admin" },
            new ControllerDefinition { Name = "Shared2" },
            new ControllerDefinition { Name = "Shared1" },
        };
        var settings = new Settings();
        var service = GetGeneratorService(settings: settings);

        var viewControllers = service.CreateViewOnlyControllerClasses(controllers).ToList();
        Assert.Collection(viewControllers,
            c => Assert.Equal("Shared1Controller", c.Identifier.Value),
            c => Assert.Equal("Shared2Controller", c.Identifier.Value),
            c => Assert.Equal("AdminArea_SharedController", c.Identifier.Value));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Project.SG4")]
    [InlineData("Sg4MvcCustom")]
    public void ViewControllers_UseSettingsNamespace(String sg4Namespace)
    {
        var controllers = new[]
        {
            new ControllerDefinition { Name = "Shared" },
        };
        var settings = new Settings();
        if (sg4Namespace != null)
        {
            settings.Sg4MvcNamespace = sg4Namespace;
        }

        var service = GetGeneratorService(settings: settings);

        var viewControllers = service.CreateViewOnlyControllerClasses(controllers).ToList();
        Assert.Collection(controllers, c => Assert.StartsWith(settings.Sg4MvcNamespace, c.FullyQualifiedGeneratedName));
    }

    [Fact]
    public void AreaClasses()
    {
        var controllers = new[]
        {
            new ControllerDefinition { Name = "Users", Area = "Admin" },
            new ControllerDefinition { Name = "Shared", Area = "Admin" },
            new ControllerDefinition { Name = "Shared" },
        };
        var areaControllers = controllers.ToLookup(c => c.Area);
        var service = GetGeneratorService();

        var areaClasses = service.CreateAreaClasses(areaControllers).ToList();
        Assert.Collection(areaClasses,
            a => Assert.Equal("AdminAreaClass", a.Identifier.Value));
    }

    [Fact]
    public void AreaClasses_Sort()
    {
        var controllers = new[]
        {
            new ControllerDefinition { Name = "Shared", Area = "Admin2" },
            new ControllerDefinition { Name = "Shared", Area = "Admin1" },
        };
        var areaControllers = controllers.ToLookup(c => c.Area);
        var service = GetGeneratorService();

        var areaClasses = service.CreateAreaClasses(areaControllers).ToList();
        Assert.Collection(areaClasses,
            a => Assert.Equal("Admin1AreaClass", a.Identifier.Value),
            a => Assert.Equal("Admin2AreaClass", a.Identifier.Value));
    }
}
