using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspNetSimple.Pages;

public partial class DefaultModel : PageModel
{
    public virtual IActionResult OnGet()
    {
        return Page();
    }

    public virtual void OnPost(Int32 id)
    {
    }

    public virtual void OnPostTest(Int32 id)
    {
    }

    public virtual Task OnPostDeleteAsync()
    {
        return Task.CompletedTask;
    }
}
