using CBoosterSharp.Generator.Attributes;
using CBoosterSharp.Generator.Model.Navigation;
using CBoosterSharp.Playground.Domain.Model;
using System.Windows.Controls;

namespace CBoosterSharp.Playground.Presentation;

[Route(
    module: "ProductFeatureRoutes",
    path: "/products/detail",
    scope: NavigationScope.App,
    Tag = "Product Details",
    GenerateNavigationHelpers = true
)]
public partial class ProductDetailScreen : Page
{

    [RouteParameter]
    public required Product Product { get; init; }

    public ProductDetailScreen()
    {
        InitializeComponent();
    }
}
