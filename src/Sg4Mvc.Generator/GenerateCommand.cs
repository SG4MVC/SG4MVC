using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Sg4Mvc.Generator.Controllers;
using Sg4Mvc.Generator.Controllers.Interfaces;
using Sg4Mvc.Generator.Locators;
using Sg4Mvc.Generator.Pages;
using Sg4Mvc.Generator.Pages.Interfaces;
using Sg4Mvc.Generator.Services;

namespace Sg4Mvc.Generator;

public class GenerateCommand
{
    public GenerateCommand(IControllerRewriterService controllerRewriter,
        IPageRewriterService pageRewriter,
        IEnumerable<IViewLocator> viewLocators,
        IEnumerable<IPageViewLocator> pageViewLocators,
        Sg4MvcGeneratorService generatorService,
        Settings settings)
    {
        ControllerRewriter = controllerRewriter;
        PageRewriter = pageRewriter;
        ViewLocators = viewLocators;
        PageViewLocators = pageViewLocators;
        GeneratorService = generatorService;
        Settings = settings;
    }

    private IControllerRewriterService ControllerRewriter { get; }
    private IPageRewriterService PageRewriter { get; }
    private IEnumerable<IViewLocator> ViewLocators { get; }
    private IEnumerable<IPageViewLocator> PageViewLocators { get; }
    private Sg4MvcGeneratorService GeneratorService { get; }
    private Settings Settings { get; }

    public void Run(GeneratorExecutionContext context)
    {
        Console.WriteLine("Project: " + context.Compilation.AssemblyName);
        Console.WriteLine();

        // Prep the project Compilation object, and process the Controller public methods list
        //SyntaxNodeHelpers.PopulateControllerClassMethodNames();

        // Analyse the controllers in the project, as well as locate all the view files
        var controllers = ControllerRewriter.RewriteControllers(context);
        var allViewFiles = ViewLocators.SelectMany(x => x.Find("context"));

        // Assign view files to controllers
        foreach (var views in allViewFiles.GroupBy(v => new { v.AreaName, v.ControllerName }))
        {
            var controller = controllers
                .FirstOrDefault(c => String.Equals(c.Name, views.Key.ControllerName, StringComparison.OrdinalIgnoreCase)
                                  && String.Equals(c.Area, views.Key.AreaName, StringComparison.OrdinalIgnoreCase));

            if (controller == null)
            {
                controllers.Add(controller = new ControllerDefinition
                {
                    Area = views.Key.AreaName,
                    Name = views.Key.ControllerName,
                });
            }

            foreach (var view in views)
            {
                controller.Views.Add(view);
            }
        }

        // Generate mappings for area names, to avoid clashes with controller names
        //var areaMap = GenerateAreaMap(controllers);
        foreach (var controller in controllers.Where(a => !String.IsNullOrEmpty(a.Area)))
        {
            //controller.AreaKey = areaMap[controller.Area];
        }

        // Analyse the razor pages in the project, as well as locate all the view files
        var definitions = PageRewriter.RewritePages(context);
        IList<PageView> pages = PageViewLocators
            .SelectMany(x => x.Find("context"))
            .Where(p => p.IsPage)
            .ToList();

        foreach (var page in pages)
        {
            page.Definition = definitions.FirstOrDefault(d => d.GetFilePath() == (page.FilePath + ".cs"));
        }

        // Generate the Sg4Mvc.generated.cs file
        //GeneratorService.Generate(context, controllers, pages);
    }
}
