using System;

namespace Sg4Mvc;

public static class Sg4MvcHelpers
{
    private static String ProcessVirtualPathDefault(String virtualPath) => virtualPath;

    public static Func<String, String> ProcessVirtualPath = ProcessVirtualPathDefault;
}
