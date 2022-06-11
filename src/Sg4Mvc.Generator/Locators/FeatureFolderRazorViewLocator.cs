using System;
using System.Collections.Generic;
using System.Linq;
using Sg4Mvc.Generator.Controllers;
using Path = System.IO.Path;

namespace Sg4Mvc.Generator.Locators;

public class FeatureFolderRazorViewLocator : DefaultRazorViewLocator
{
    public FeatureFolderRazorViewLocator(IFileLocator fileLocator, Settings settings)
        : base(fileLocator, settings)
    {
    }

    private Boolean _allAreasAreFeatureFolders = false;

    protected override String GetViewsRoot(String projectRoot)
    {
        return Path.Combine(projectRoot, Settings.FeatureFolders.FeaturesPath);
    }

    protected override String GetAreaViewsRoot(String areaRoot, String areaName)
    {
        return _allAreasAreFeatureFolders ||
            Settings.FeatureFolders.FeatureOnlyAreas?.Contains(areaName, StringComparer.OrdinalIgnoreCase) == true
                ? areaRoot
                : Path.Combine(areaRoot, Settings.FeatureFolders.FeaturesPath);
    }

    public override IEnumerable<View> Find(String workingDirectory)
    {
        if (Settings.FeatureFolders?.Enabled != true)
        {
            return new List<View>(0);
        }

        _allAreasAreFeatureFolders = Settings.FeatureFolders.FeatureOnlyAreas?.Contains("*") == true;

        return base.Find(workingDirectory).ToList();
    }
}
