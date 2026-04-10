using System;
using System.Collections.Generic;
using System.Linq;

namespace Sg4Mvc.Generator.Locators;

public class DefaultStaticFileLocator(
    IFileLocator fileLocator,
    Settings settings)
    : IStaticFileLocator
{
    public List<StaticFile> Find(String staticPathRoot)
    {
        var files = fileLocator.GetFiles(staticPathRoot, "*", recurse: true).AsEnumerable();
        if (settings.ExcludedStaticFileExtensions?.Length > 0)
        {
            files = files.Where(f => !settings.ExcludedStaticFileExtensions.Any(e => f.EndsWith(e)));
        }

        if (!staticPathRoot.EndsWith("/"))
        {
            staticPathRoot += "/";
        }

        var rootUri = new Uri(staticPathRoot);

        return files
            .Select(f => new StaticFile(rootUri.MakeRelativeUri(new Uri(f))))
            .ToList();
    }
}
