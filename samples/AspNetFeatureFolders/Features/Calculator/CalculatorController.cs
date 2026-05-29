using System.Threading;
using Microsoft.AspNetCore.Mvc;

namespace AspNetFeatureFolders.Features.Calculator;

public partial class CalculatorController : Controller
{
    public virtual IActionResult Index() => View();
    public virtual IActionResult Index5(MyViewModel model, CancellationToken cancellationToken) => View();
}

public class MyViewModel
{
    public System.String Id { get; set; }
}
