using System;
using System.Collections.Generic;
using Sg4Mvc.Generator.Pages;

namespace Sg4Mvc.Generator.Locators;

public interface IPageViewLocator
{
    IEnumerable<PageView> Find(String workingDirectory);
}
