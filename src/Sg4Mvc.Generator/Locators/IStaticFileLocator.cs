using System;
using System.Collections.Generic;

namespace Sg4Mvc.Generator.Locators;

public interface IStaticFileLocator
{
    IEnumerable<StaticFile> Find(String staticPathRoot);
}
