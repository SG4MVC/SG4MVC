using System;
using System.Collections.Generic;

namespace Sg4Mvc.Generator.Locators;

public interface IStaticFileLocator
{
    List<StaticFile> Find(String staticPathRoot);
}
