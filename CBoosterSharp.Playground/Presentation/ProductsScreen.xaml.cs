using CBoosterSharp.Generator.Attributes;
using CBoosterSharp.Generator.Model.Navigation;
using System.Windows.Controls;

namespace CBoosterSharp.Playground.Presentation;

[Route(
    module: "ProductFeatureRoutes",
    path: "/products",
    scope: NavigationScope.App,
    Tag = "Products",
    GenerateNavigationHelpers = true
)]
public partial class ProductsScreen : Page
{
    public ProductsScreen()
    {
        InitializeComponent();
    }
}
