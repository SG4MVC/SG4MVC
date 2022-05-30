using System;
using System.Linq;

namespace Sg4Mvc.Generator;

public class StaticFile
{
    public StaticFile(Uri relativePath)
    {
        var pathParts = relativePath.ToString().Split('/', '\\');

        FileName = pathParts.Last();
        RelativePath = relativePath;
        Container = String.Join("/", pathParts.Take(pathParts.Length - 1));
    }

    public String FileName { get; set; }
    public Uri RelativePath { get; set; }
    public String Container { get; set; }
}
