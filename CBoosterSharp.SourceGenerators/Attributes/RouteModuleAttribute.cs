namespace CBoosterSharp.Generator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class RouteModuleAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
