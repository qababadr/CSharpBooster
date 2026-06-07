using CBoosterSharp.Core.Model;

[AttributeUsage(AttributeTargets.Class)]
public sealed class InjectableAttribute(DIScope scope = DIScope.Singleton) : Attribute
{
    public DIScope Scope { get; } = scope;
}
