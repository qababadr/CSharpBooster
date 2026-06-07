using CBoosterSharp.Generator.Model.Navigation;

namespace CBoosterSharp.Generator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class RouteAttribute(
    string module,
    string path,
    NavigationScope scope
) : Attribute
{
    public string Module { get; } = module;

    public string Path { get; } = path;

    public NavigationScope Scope { get; } = scope;

    public string? Tag { get; set; }

    public bool GenerateNavigationHelpers { get; set; } = false;
}
