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

        Assert.Empty(locator.Find(VirtualFileLocator.ProjectRoot));
    }

    [Fact]
    public void FeatureFolders_Enabled()
    {
        var locator = new FeatureFolderRazorViewLocator(VirtualFileLocator.Default, new Settings
        {
            FeatureFolders = new Settings.FeatureFoldersClass { Enabled = true }
        });

        Assert.Collection(locator.Find(VirtualFileLocator.ProjectRoot),
            v => AssertView(v, "", "Users", "Index", null, "~/Features/Users/Index.cshtml"),
            v => AssertView(v, "", "Users", "Details", null, "~/Features/Users/Details.cshtml"),
            v => AssertView(v, "Admin", "Home", "Index", null, "~/Areas/Admin/Features/Home/Index.cshtml")
        );
    }
}
