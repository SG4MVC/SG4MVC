using System;
using System.Runtime.CompilerServices;

namespace Sg4Mvc;

public static class Sg4MvcHelpers
{
    private static String ProcessVirtualPathDefault(String virtualPath) => virtualPath;

    public static Func<String, String> ProcessVirtualPath = ProcessVirtualPathDefault;

    /// <summary>
    /// Creates an instance of <typeparamref name="T"/> without invoking any constructor.
    /// Used by generated code to instantiate Sg4Mvc_ controller/page stubs safely,
    /// avoiding NullReferenceExceptions from constructor parameters that would be null.
    /// </summary>
    public static T CreateUninitializedInstance<T>() where T : class
        => (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
}
