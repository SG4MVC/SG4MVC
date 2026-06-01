using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Sg4Mvc.Generator;

public class Settings : IEquatable<Settings>
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

    /// <summary>
    /// Loads settings from the sg4mvc.json AdditionalText, if present.
    /// Suitable for use inside the incremental generator pipeline.
    /// </summary>
    public static Settings Load(AdditionalText settingsFile, System.Threading.CancellationToken ct)
    {
        if (settingsFile is null)
            return new Settings();

        var text = settingsFile.GetText(ct)?.ToString();
        if (String.IsNullOrWhiteSpace(text))
            return new Settings();

        try
        {
            return global::System.Text.Json.JsonSerializer.Deserialize<Settings>(text) ?? new Settings();
        }
        catch
        {
            return new Settings();
        }
    }

    public bool Equals(Settings other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return HelpersPrefix == other.HelpersPrefix &&
               PageHelpersPrefix == other.PageHelpersPrefix &&
               Sg4MvcNamespace == other.Sg4MvcNamespace &&
               LinksNamespace == other.LinksNamespace &&
               StaticFilesPath == other.StaticFilesPath &&
               GenerateParamsForActionMethods == other.GenerateParamsForActionMethods &&
               ParamsPropertySuffix == other.ParamsPropertySuffix &&
               GeneratePageViewsClass == other.GeneratePageViewsClass &&
               UpdateGeneratedByVersion == other.UpdateGeneratedByVersion &&
               NullableSequenceEqual(ExcludedStaticFileExtensions, other.ExcludedStaticFileExtensions) &&
               NullableSequenceEqual(ReferencedNamespaces, other.ReferencedNamespaces) &&
               NullableSequenceEqual(PragmaCodes, other.PragmaCodes) &&
               FeatureFoldersEqual(FeatureFolders, other.FeatureFolders);
    }

    public override bool Equals(object obj) => Equals(obj as Settings);

    public override int GetHashCode()
    {
        unchecked
        {
            int h = 17;
            h = h * 31 + (HelpersPrefix?.GetHashCode() ?? 0);
            h = h * 31 + (Sg4MvcNamespace?.GetHashCode() ?? 0);
            h = h * 31 + (LinksNamespace?.GetHashCode() ?? 0);
            h = h * 31 + (StaticFilesPath?.GetHashCode() ?? 0);
            return h;
        }
    }

    private static bool NullableSequenceEqual(String[] a, String[] b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.SequenceEqual(b);
    }

    private static bool FeatureFoldersEqual(FeatureFoldersClass a, FeatureFoldersClass b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.Enabled == b.Enabled &&
               a.FeaturesPath == b.FeaturesPath &&
               a.StaticFileAccess == b.StaticFileAccess &&
               NullableSequenceEqual(a.FeatureOnlyAreas, b.FeatureOnlyAreas);
    }
}
