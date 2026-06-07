namespace CBoosterSharp.Generator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class DIEntryPointAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
