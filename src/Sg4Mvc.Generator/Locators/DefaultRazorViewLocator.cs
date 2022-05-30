using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Sg4Mvc.Generator.Extensions;
using Path = System.IO.Path;

namespace Sg4Mvc.Generator.Locators;

public class DefaultRazorViewLocator : IViewLocator
{
    protected const String ViewsFolder = "Views";
    protected const String AreasFolder = "Areas";

    public DefaultRazorViewLocator(IFileLocator fileLocator, Settings settings)
    {
        FileLocator = fileLocator;
        Settings = settings;
    }

    private IFileLocator FileLocator { get; }
    protected Settings Settings { get; }

    protected virtual String GetViewsRoot(String projectRoot) => Path.Combine(projectRoot, ViewsFolder);
    protected virtual String GetAreaViewsRoot(String areaRoot, String areaName) => Path.Combine(areaRoot, ViewsFolder);

    public virtual IEnumerable<View> Find(GeneratorExecutionContext context)
    {
        var projectRoot = context.GetWorkingDirectory();

        foreach (var (Area, Controller, Path) in FindControllerViewFolders(projectRoot))
        {
            if (!FileLocator.DirectoryExists(Path))
            {
                continue;
            }

            foreach (var view in FindViews(projectRoot, Area, Controller, Path))
                yield return view;
        }
    }

    protected IEnumerable<(String Area, String Controller, String Path)> FindControllerViewFolders(String projectRoot)
    {
        var viewsRoot = GetViewsRoot(projectRoot);
        if (FileLocator.DirectoryExists(viewsRoot))
        {
            foreach (var controllerPath in FileLocator.GetDirectories(viewsRoot))
            {
                var controllerName = Path.GetFileName(controllerPath);
                yield return (String.Empty, controllerName, controllerPath);
            }
        }

        var areasPath = Path.Combine(projectRoot, AreasFolder);
        if (FileLocator.DirectoryExists(areasPath))
        {
            foreach (var areaRoot in FileLocator.GetDirectories(areasPath))
            {
                var areaName = Path.GetFileName(areaRoot);
                viewsRoot = GetAreaViewsRoot(areaRoot, areaName);
                if (FileLocator.DirectoryExists(viewsRoot))
                {
                    foreach (var controllerPath in FileLocator.GetDirectories(viewsRoot))
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
        foreach (var file in FileLocator.GetFiles(controllerPath, "*.cshtml"))
        {
            yield return GetView(projectRoot, file, controllerName, areaName);
        }

        foreach (var directory in FileLocator.GetDirectories(controllerPath))
        {
            foreach (var file in FileLocator.GetFiles(directory, "*.cshtml"))
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
