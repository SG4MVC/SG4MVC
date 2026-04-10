using System;
using System.Collections.Generic;
using Sg4Mvc.Generator.Controllers;
using Sg4Mvc.Generator.Extensions;
using Path = System.IO.Path;

namespace Sg4Mvc.Generator.Locators;

public class DefaultRazorViewLocator(
    IFileLocator fileLocator,
    Settings settings)
    : IViewLocator
{
    protected const String ViewsFolder = "Views";
    protected const String AreasFolder = "Areas";

    protected Settings Settings { get; } = settings;

    protected virtual String GetViewsRoot(String projectRoot)
    {
        return Path.Combine(projectRoot, ViewsFolder);
    }

    protected virtual String GetAreaViewsRoot(String areaRoot, String areaName)
    {
        return Path.Combine(areaRoot, ViewsFolder);
    }

    public virtual IEnumerable<View> Find(String workingDirectory)
    {
        foreach (var (Area, Controller, Path) in FindControllerViewFolders(workingDirectory))
        {
            if (!fileLocator.DirectoryExists(Path))
            {
                continue;
            }

            foreach (var view in FindViews(workingDirectory, Area, Controller, Path))
                yield return view;
        }
    }

    protected IEnumerable<(String Area, String Controller, String Path)> FindControllerViewFolders(String projectRoot)
    {
        var viewsRoot = GetViewsRoot(projectRoot);
        if (fileLocator.DirectoryExists(viewsRoot))
        {
            foreach (var controllerPath in fileLocator.GetDirectories(viewsRoot))
            {
                var controllerName = Path.GetFileName(controllerPath);
                yield return (String.Empty, controllerName, controllerPath);
            }
        }

        var areasPath = Path.Combine(projectRoot, AreasFolder);
        if (fileLocator.DirectoryExists(areasPath))
        {
            foreach (var areaRoot in fileLocator.GetDirectories(areasPath))
            {
                var areaName = Path.GetFileName(areaRoot);
                viewsRoot = GetAreaViewsRoot(areaRoot, areaName);
                if (fileLocator.DirectoryExists(viewsRoot))
                {
                    foreach (var controllerPath in fileLocator.GetDirectories(viewsRoot))
                    {
                        var controllerName = Path.GetFileName(controllerPath);
                        yield return (areaName, controllerName, controllerPath);
                    }
                }
            }
        }
    }

    protected virtual IEnumerable<View> FindViews(String projectRoot, String areaName, String controllerName, String controllerPath)
    {
        foreach (var file in fileLocator.GetFiles(controllerPath, "*.cshtml"))
        {
            yield return GetView(projectRoot, file, controllerName, areaName);
        }

        foreach (var directory in fileLocator.GetDirectories(controllerPath))
        {
            foreach (var file in fileLocator.GetFiles(directory, "*.cshtml"))
            {
                yield return GetView(projectRoot, file, controllerName, areaName, Path.GetFileName(directory));
            }
        }
    }

    private View GetView(String projectRoot, String filePath, String controllerName, String areaName, String templateKind = null)
    {
        var relativePath = new Uri("~" + filePath.GetRelativePath(projectRoot).Replace("\\", "/"), UriKind.Relative);

        return new View(areaName, controllerName, Path.GetFileNameWithoutExtension(filePath), relativePath, templateKind);
    }
}
