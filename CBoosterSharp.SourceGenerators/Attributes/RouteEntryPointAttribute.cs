namespace CBoosterSharp.Generator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class RouteEntryPointAttribute(
    string name
) : Attribute
{
    public string Name { get; } = name;
}
