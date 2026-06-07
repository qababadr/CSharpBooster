using System.Windows.Controls;

namespace CBoosterSharp.Navigation.Model;

public sealed record RouteDefinition(
    string Path,
    NavigationScope Scope,
    Func<object?, Page> Page,
    string Tag = ""
);
