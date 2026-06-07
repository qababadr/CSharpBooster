# CSharpBooster

CSharpBooster is a compile-time code generation toolkit for .NET applications that eliminates boilerplate around:

- Use Cases
- Dependency Injection
- Service Registration
- Navigation & Routing
- Route Parameters
- Navigation Helpers
- Clean architecture-friendly

Built with source generators, CSharpBooster produces strongly typed code at compile time with zero runtime reflection.

---

# Installation

Install the required packages:

```bash
dotnet add package CBoosterSharp.Generator
```

---

# Features

## Use Case Generation

Generate wrappers, dependency registration, and clean application boundaries directly from interfaces.

### Define a Repository

```csharp
using CBoosterSharp.Generator.Attributes;
using CBoosterSharp.Generator.Model;

[Usecaseable(
    GenerateDI = true,
    GenerateWrapper = true,
    DIScope = DIScope.Singleton
)]
[Injectable(implementation: nameof(AuthRepositoryImpl))]
public interface IAuthRepository
{
    Task<int> Login(
        string email,
        string password,
        CancellationToken cancellationToken = default
    );

    Task Logout();
}
```

### Generated

CSharpBooster automatically generates:

- Repository wrapper classes
- Dependency registration code
- Use case accessors
- Constructor injection support

No manual service registration required.

---

# Dependency Injection

## Register Services

Use the `Injectable` attribute to register classes automatically.

```csharp
[Injectable]
public partial class LoaderController : ObservableObject
{
}
```

---

## Modules

Modules provide a structured way to expose custom dependencies.

```csharp
[Module]
public class AuthDomainModule
{
    [Provides(DIScope.Singleton)]
    public static FilePathService ProvideFilePathService()
    {
        return new FilePathService();
    }
}
```

### Supported Scopes

```csharp
DIScope.Singleton
DIScope.Scoped
DIScope.Transient
```

---

# Dependency Entry Point

Generate a single application entry point for all dependencies.

```csharp
[DIEntryPoint(name: "PlayGroundDependencies")]
public partial class App : Application
{
}
```

Generated:

```csharp
PlayGroundDependencies.InstallDependencies();
```

Usage:

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    PlayGroundDependencies.InstallDependencies();
}
```

---

# Navigation

CSharpBooster provides strongly typed navigation generation.

No route strings.

No manual parameter extraction.

No reflection.

---

## Route Modules

Create a route module.

```csharp
[RouteModule(name: "ProductFeatureRoutes")]
public sealed partial class ProductFeatureRoutes
{
}
```

---

## Screens

### Products Screen

```csharp
[Route(
    module: "ProductFeatureRoutes",
    path: "/products",
    scope: NavigationScope.App,
    Tag = "Products",
    GenerateNavigationHelpers = true
)]
public partial class ProductsScreen : Page
{
}
```

---

### Product Details Screen

```csharp
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
}
```

---

# Route Parameters

Parameters are strongly typed.

```csharp
[RouteParameter]
public required Product Product { get; init; }
```

Generated navigation APIs automatically enforce parameter requirements.

---

# Route Registration

Generate a centralized route registry.

```csharp
[RouteEntryPoint(name: "PlayGroundRoutes")]
public partial class App : Application
{
}
```

Generated:

```csharp
PlayGroundRoutes.RegisterRoutes();
```

Usage:

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    PlayGroundRoutes.RegisterRoutes();
}
```

---

# Generated Navigation Helpers

Navigate without route strings.

```csharp
Router.Current.NavigateToProductsScreen();
```

Navigate with parameters:

```csharp
Router.Current.NavigateToProductDetailScreen(
    product,
    onNavigating: (string tag) =>
    {
        MessageBox.Show(
            $"Navigating to product detail screen for {product.Name}");
    });
```

---

# Application Setup

```csharp
[DIEntryPoint(name: "PlayGroundDependencies")]
[RouteEntryPoint(name: "PlayGroundRoutes")]
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        PlayGroundDependencies.InstallDependencies();
        PlayGroundRoutes.RegisterRoutes();
    }
}
```

---

# Why CSharpBooster?

### Less Boilerplate

No manual service registration.

### Strongly Typed Navigation

No route strings.

### Compile-Time Safety

Errors are caught during build.

### Source Generated

No runtime reflection.

### MVVM Friendly

Works naturally with modern .NET architectures.

### Scalable

Designed for small projects and enterprise applications alike.

---

# License

Licensed under the MIT License.
