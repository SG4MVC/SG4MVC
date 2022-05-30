using System;

namespace Sg4Mvc.Generator.Locators;

public interface IFileLocator
{
    String[] GetFiles(String parentPath, String filter, Boolean recurse = false);
    String[] GetDirectories(String parentPath);
    Boolean DirectoryExists(String path);
}
