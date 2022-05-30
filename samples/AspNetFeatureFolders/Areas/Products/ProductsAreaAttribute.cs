using Microsoft.AspNetCore.Mvc;

namespace AspNetFeatureFolders.Areas.Products;

public class ProductsAreaAttribute : AreaAttribute
{
    public ProductsAreaAttribute() : base("Products")
    {
    }
}
