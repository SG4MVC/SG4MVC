using System;
using System.Collections.Generic;
using Sg4Mvc.Generator.Controllers;

namespace Sg4Mvc.Generator.Locators;

public interface IViewLocator
{
    IEnumerable<View> Find(String workingDirectory);
}
