namespace CBoosterSharp.Navigation.Model;

public sealed record RouteMatch(
    RouteDefinition Definition,
    IReadOnlyDictionary<string, string> Parameters
);
