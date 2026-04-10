using System;
using System.IO;

namespace Sg4Mvc.Generator.Locators;

public class PhysicalFileLocator : IFileLocator
{
    public Boolean DirectoryExists(String path)
        => Directory.Exists(path);

    public String[] GetDirectories(String parentPath)
        => Directory.GetDirectories(parentPath);

    public String[] GetFiles(String parentPath, String filter, Boolean recurse = false)
    {
        if (!DirectoryExists(parentPath))
        {
            return [];
        }

        return Directory.GetFiles(parentPath, filter, recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
    }
}
