using System;

namespace Sg4Mvc.Generator.Pages;

public class PageView : IView
{
    public PageView(String name, String filePath, String relativePath, String pagePath, Boolean isPage)
    {
        Name = name;
        FilePath = filePath;
        RelativePath = new Uri("~" + relativePath, UriKind.Relative);
        PagePath = pagePath;
        IsPage = isPage;

        var segments = pagePath.Split(new[] { '/', }, StringSplitOptions.RemoveEmptyEntries);
        Array.Resize(ref segments, segments.Length - 1);
        Segments = segments;
    }

    public String Name { get; }
    public String FilePath { get; }
    public Uri RelativePath { get; }
    public String PagePath { get; }
    public Boolean IsPage { get; }
    public String[] Segments { get; }

    public PageDefinition Definition { get; set; }

    public String TemplateKind => null;
}
