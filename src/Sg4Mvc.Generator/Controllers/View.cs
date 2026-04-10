using System;

namespace Sg4Mvc.Generator.Controllers;

public class View(String areaName,
    String controllerName,
    String viewName,
    Uri relativePath,
    String templateKind) : IView
{
    public String AreaName { get; } = areaName;
    public String ControllerName { get; } = controllerName;
    public String Name { get; } = viewName;
    public Uri RelativePath { get; } = relativePath;
    public String TemplateKind { get; set; } = templateKind;
}
