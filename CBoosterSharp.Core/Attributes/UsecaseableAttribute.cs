using CBoosterSharp.Core.Model;

namespace CBoosterSharp.Core.Attributes;

[AttributeUsage(AttributeTargets.Interface)]
public class UsecaseableAttribute : Attribute
{
    public bool PrefixWithRepo { get; set; } = true;

    public bool GenerateWrapper { get; set; } = true;

    public Visibility Visibility { get; set; }
        = Visibility.Public;

    public bool GenerateDI { get; set; } = false;

    public DIScope DIScope { get; set; }
        = DIScope.Singleton;
}
