using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Sg4Mvc.Generator.Controllers;
using Sg4Mvc.Generator.Locators;
using Sg4Mvc.Generator.Pages;

namespace Sg4Mvc.Generator.Services;

public static class DataGroupingService
{

    public static (List<ControllerDefinition> Controllers, List<PageView> Pages) Prep(
        String workingDirectory,
        List<ControllerDefinition> controllers,
        List<PageDefinition> pageDefinitions,
        ImmutableArray<AdditionalText> additionalTexts
    )
    {
        var (viewLocators, pageViewLocators, staticFileLocators) = LocatorsFactory();

        // De-dupe the controllers by namespace
        controllers = controllers
            .GroupBy(c => c.Namespace + c.Name)
            .Select(g => g.First())
            .ToList();

        // De-dupe the pages by namespace
        pageDefinitions = pageDefinitions
            .GroupBy(c => c.Namespace + c.Name)
            .Select(g =>
            {
                var pageDefinition = g.First();

                pageDefinition.FilePaths = g
                    .SelectMany(gi => gi.FilePaths)
                    .Distinct()
                    .ToList();

                return pageDefinition;
            })
            .ToList();

        // Assign view files to controllers
        AssignViewsToControllers(workingDirectory, controllers, additionalTexts, viewLocators);

        // Add Area to names as needed to avoid clashes with controller names
        RenameAreasNameClashingControllers(controllers);

        var pages = GetPageViews(workingDirectory, pageDefinitions, additionalTexts, pageViewLocators);

        return (controllers, pages);
    }

    private static List<PageView> GetPageViews(
        String workingDirectory,
        List<PageDefinition> pageDefinitions,
        ImmutableArray<AdditionalText> additionalTexts,
        IPageViewLocator[] viewLocators
    )
    {
        var pages = viewLocators
            .SelectMany(vl => vl.Find(workingDirectory))
            .Where(p => p.IsPage)
            .ToList();

        foreach (var page in pages)
        {
            page.Definition = pageDefinitions
                .FirstOrDefault(d => d.GetFilePath() == page.FilePath + ".cs");
        }

        return pages;
    }

    private static void AssignViewsToControllers(
        String workingDirectory,
        List<ControllerDefinition> controllers,
        ImmutableArray<AdditionalText> additionalTexts,
        IViewLocator[] viewLocators
    )
    {
        var allViewFiles = viewLocators.SelectMany(x => x.Find(workingDirectory));

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
    }

    public static void RenameAreasNameClashingControllers(List<ControllerDefinition> controllers)
    {
        var areaMap = controllers
            .Select(c => c.Area)
            .Where(a => !String.IsNullOrEmpty(a))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(a => a);

        foreach (var area in areaMap.Keys.ToArray())
        {
            if (controllers.Any(c => c.Area == String.Empty && c.Name == area))
            {
                areaMap[area] = area + "Area";
            }
        }

        foreach (var controller in controllers.Where(a => !String.IsNullOrEmpty(a.Area)))
        {
            controller.AreaKey = areaMap[controller.Area];
        }
    }

    private static (IViewLocator[] viewLocators,
        IPageViewLocator[] pageViewLocators,
        IStaticFileLocator[] staticFileLocators) LocatorsFactory()
    {
        var settings = new Settings();
        var fileLocator = new PhysicalFileLocator();

        var viewLocators = new IViewLocator[]
        {
            new FeatureFolderRazorViewLocator(fileLocator, settings),
            new DefaultRazorViewLocator(fileLocator, settings)
        };

        var pageViewLocators = new IPageViewLocator[]
        {
            new DefaultRazorPageViewLocator(fileLocator, settings)
        };

        var staticFileLocators = new IStaticFileLocator[]
        {
            new DefaultStaticFileLocator(fileLocator, settings)
        };

        return (viewLocators, pageViewLocators, staticFileLocators);
    }
}
