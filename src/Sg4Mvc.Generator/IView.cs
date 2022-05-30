using System;

namespace Sg4Mvc.Generator;

public interface IView
{
    Uri RelativePath { get; }
    String TemplateKind { get; }
    String Name { get; }
}
