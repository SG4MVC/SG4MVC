using System;

namespace Sg4Mvc.Generator.Controllers;

public class View : IView
{
    public View(String areaName,
        String controllerName,
        String viewName,
        Uri relativePath,
        String templateKind)
    {
        AreaName = areaName;
        ControllerName = controllerName;
        Name = viewName;
        RelativePath = relativePath;
        TemplateKind = templateKind;
    }

    public String AreaName { get; }
    public String ControllerName { get; }
    public String Name { get; }
    public Uri RelativePath { get; }
    public String TemplateKind { get; set; }
}
