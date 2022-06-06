using System;
using Sg4Mvc.Generator.Controllers;
using Xunit;

namespace Sg4Mvc.Test.Locators;

public abstract class RazorViewLocatorTestsBase
{
    protected void AssertView(View view, String areaName, String controllerName, String viewName, String templateKind, String viewPath)
    {
        Assert.Equal(areaName, view.AreaName);
        Assert.Equal(controllerName, view.ControllerName);
        Assert.Equal(viewName, view.Name);
        Assert.Equal(viewPath, view.RelativePath.ToString());
        if (templateKind != null)
        {
            Assert.Equal(templateKind, view.TemplateKind);
        }
        else
        {
            Assert.Null(view.TemplateKind);
        }
    }
}
