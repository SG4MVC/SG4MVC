## SG4MVC

SG4MVC is a source generator for ASP.NET MVC Core apps that creates strongly typed helpers that eliminate the use of literal strings in many places.  

It is a re-implementation of [R4MVC](https://github.com/T4MVC/R4MVC) using source generators.

R4MVC runs as you build, and currently has only been tested on net6.0

## Benefits

Instead of

````c#
@Html.ActionLink("Dinner Details", "Details", "Dinners", new { id = Model.DinnerID }, null)
````
SG4MVC lets you write
````c#
@Html.ActionLink("Dinner Details", MVC.Dinners.Details(Model.DinnerID))
````

When you're using tag helpers, instead of
```html
<a asp-action="Details" asp-controller="Dinners" asp-route-id="@Model.DinnerID">Dinner Details</a>
```
you can write (after registering Sg4Mvc tag helpers in _ViewImports.cshtml with the directive: `@addTagHelper *, Sg4Mvc`)
```html
<a mvc-action="MVC.Dinners.Details(Model.DinnerID)">Dinner Details</a>
```

and that's just the beginning!

### Use the following links to get started

*   **Install** SG4MVC is distributed using using [NuGet](http://nuget.org). Visit the [Installation page](https://github.com/T4MVC/R4MVC/wiki/Installation)
*   **Discuss**: Discuss it on [GitHub](https://github.com/SG4MVC/SG4MVC/issues)
*   **Contribute**
*   **History &amp; release notes**: [change history](CHANGELOG.md)
