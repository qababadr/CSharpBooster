using CBoosterSharp.Generator.Model;

namespace CBoosterSharp.Generator.Attributes;

[AttributeUsage(
    AttributeTargets.Interface |
    AttributeTargets.Class,
    AllowMultiple = false
)]
public sealed class InjectableAttribute(
    string? implementation = null,
    DIScope scope = DIScope.Singleton
) : Attribute
{
    public string? Implementation { get; } = implementation;

    public DIScope Scope { get; } = scope;
}