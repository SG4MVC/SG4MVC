using System;

namespace Sg4Mvc.Generator;

public class Settings
{
    public String _generatedByVersion { get; set; }
    public Boolean ShouldSerialize_generatedByVersion() => UpdateGeneratedByVersion;
    public Boolean UpdateGeneratedByVersion { get; set; } = true;
    public String HelpersPrefix { get; set; } = "MVC";
    public String PageHelpersPrefix { get; set; } = "MVCPages";
    public String Sg4MvcNamespace { get; set; } = "Sg4Mvc";
    public String LinksNamespace { get; set; } = "Links";
    public String StaticFilesPath { get; set; } = "wwwroot";
    public String[] ExcludedStaticFileExtensions { get; set; }
    public String[] ReferencedNamespaces { get; set; }
    public String[] PragmaCodes { get; set; }
    public Boolean GenerateParamsForActionMethods { get; set; } = false;
    public String ParamsPropertySuffix { get; set; } = "Params";

    // Don't include the page ViewsClass by default, and hide the option unless it's enabled
    // Not sure if we'd even need that, but leaving it in for the time being
    public Boolean GeneratePageViewsClass { get; set; } = false;
    public Boolean ShouldSerializeGeneratePageViewsClass() => GeneratePageViewsClass;

    public FeatureFoldersClass FeatureFolders { get; set; } = new();
    public class FeatureFoldersClass
    {
        public Boolean Enabled { get; set; }
        public String FeaturesPath { get; set; } = "Features";
        public Boolean StaticFileAccess { get; set; }
        public String[] FeatureOnlyAreas { get; set; }
    }
}
