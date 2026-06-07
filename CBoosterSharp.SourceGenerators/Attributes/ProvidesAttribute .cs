using CBoosterSharp.Generator.Model;

namespace CBoosterSharp.Generator.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class ProvidesAttribute(DIScope scope = DIScope.Singleton) : Attribute
{
    public DIScope Scope { get; } = scope;
}
