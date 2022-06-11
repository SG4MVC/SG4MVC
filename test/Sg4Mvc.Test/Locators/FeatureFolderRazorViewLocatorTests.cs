using System;
using Sg4Mvc.Generator;
using Sg4Mvc.Generator.Locators;
using Xunit;

namespace Sg4Mvc.Test.Locators;

public class FeatureFolderRazorViewLocatorTests : RazorViewLocatorTestsBase
{
    [Fact]
    public void FeatureFolders_Disabled()
    {
        var locator = new FeatureFolderRazorViewLocator(VirtualFileLocator.Default, new Settings());
        var workingDirectory = Guid.NewGuid().ToString();

        Assert.Empty(locator.Find(workingDirectory));
    }

    [Fact]
    public void FeatureFolders_Enabled()
    {
        var locator = new FeatureFolderRazorViewLocator(VirtualFileLocator.Default, new Settings
        {
            FeatureFolders = new Settings.FeatureFoldersClass { Enabled = true }
        });
        var workingDirectory = Guid.NewGuid().ToString();

        Assert.Collection(locator.Find(workingDirectory),
            v => AssertView(v, "", "Users", "Index", null, "~/Features/Users/Index.cshtml"),
            v => AssertView(v, "", "Users", "Details", null, "~/Features/Users/Details.cshtml"),
            v => AssertView(v, "Admin", "Home", "Index", null, "~/Areas/Admin/Features/Home/Index.cshtml")
        );
    }
}
