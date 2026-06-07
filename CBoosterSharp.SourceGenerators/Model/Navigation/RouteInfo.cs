using CBoosterSharp.Model.Navigation;

namespace CBoosterSharp.Generator.Model.Navigation;

public sealed class RouteInfo
{
    public string Module { get; init; } = null!;

    public string Namespace { get; init; } = null!;

    public string ClassName { get; init; } = null!;

    public string Path { get; init; } = null!;

    public string Scope { get; init; } = null!;

    public string? Tag { get; init; }

    public RouteParameterInfo? Parameter { get; init; }

    public bool GenerateNavigationHelpers { get; init; }
}
