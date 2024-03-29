﻿using System;
using System.Linq;
using Sg4Mvc.Generator.Locators;

namespace Sg4Mvc.Test.Locators;

class VirtualFileLocator : IFileLocator
{
    private readonly String[] _paths;
    public VirtualFileLocator(String[] paths)
    {
        _paths = paths;
    }

    public const String ProjectRoot = @"D:\Project";
    public const String ProjectRoot_wwwroot = @"D:\Project\wwwroot";

    public static VirtualFileLocator Default
        => new VirtualFileLocator(new[]
        {
            @"D:\Project\Program.cs",
            @"D:\Project\Startup.cs",
            @"D:\Project\Areas\Admin\Controllers\HomeController.cs",
            @"D:\Project\Areas\Admin\Features\Home\HomeController.cs",
            @"D:\Project\Areas\Admin\Features\Home\Index.cshtml",
            @"D:\Project\Areas\Admin\Views\Home\Index.cshtml",
            @"D:\Project\Areas\Admin\Views\Shared\EditorTemplates\User.cshtml",
            @"D:\Project\Areas\Admin\Views\Shared\_Layout.cshtml",
            @"D:\Project\Controllers\UsersController.cshtml",
            @"D:\Project\Features\Users\UsersController.cs",
            @"D:\Project\Features\Users\Index.cshtml",
            @"D:\Project\Features\Users\Details.cshtml",
            @"D:\Project\Views\EditorTemplates\User.cshtml",
            @"D:\Project\Views\Users\EditorTemplates\User.cshtml",
            @"D:\Project\Views\Users\Toolbars\ProToolbar.cshtml",
            @"D:\Project\Views\Users\Index.cshtml",
            @"D:\Project\Views\Users\Details.cshtml",
            @"D:\Project\wwwroot\lib\jslib\core.js",
            @"D:\Project\wwwroot\js\site.js",
            @"D:\Project\wwwroot\css\site.css",
            @"D:\Project\wwwroot\favicon.ico",
        });

    public Boolean DirectoryExists(String path)
        => _paths.Any(p => p.StartsWith(path.TrimEnd('\\') + "\\"));

    public String[] GetDirectories(String parentPath)
        => _paths
            .Where(p => p.StartsWith(parentPath))
            .Where(p => p.Substring(parentPath.Length).TrimStart('\\').Count(c => c == '\\') >= 1)
            .Select(p => p.Substring(0, p.Substring(parentPath.Length + 1).IndexOf('\\') + parentPath.Length + 1))
            .Distinct()
            .ToArray();

    public String[] GetFiles(String parentPath, String filter, Boolean recurse = false)
        => _paths
            .Where(p => p.StartsWith(parentPath))
            .Where(p =>
            {
                switch (filter)
                {
                    case "*":
                        return true;
                    case "*.cshtml":
                        return p.EndsWith(".cshtml");
                    default:
                        throw new NotSupportedException();
                }
            })
            .Where(p => recurse || !p.Substring(parentPath.Length).TrimStart('\\').Contains("\\"))
            .ToArray();
}
