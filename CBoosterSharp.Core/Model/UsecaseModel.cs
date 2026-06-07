using CBoosterSharp.Core.Model;
using Microsoft.CodeAnalysis;

namespace CBoosterSharp.SourceGenerators.Model;

internal sealed record UsecaseModel(
    INamedTypeSymbol Symbol,
    AttributeData Attribute
)
{
    public UsecaseConfig Config => new()
    {
        PrefixWithRepo = GetBool("PrefixWithRepo", true),
        GenerateWrapper = GetBool("GenerateWrapper", true),
        GenerateDI = GetBool("GenerateDI", false),
        DIScope = GetEnum("DIScope", DIScope.Singleton),
        Visibility = GetEnum("Visibility", Visibility.Public)
    };

    private bool GetBool(string name, bool fallback)
    {
        var arg = Attribute.NamedArguments.FirstOrDefault(x => x.Key == name);
        return arg.Value.Value is bool b ? b : fallback;
    }

    private T GetEnum<T>(string name, T fallback) where T : struct
    {
        var arg = Attribute.NamedArguments.FirstOrDefault(x => x.Key == name);
        return arg.Value.Value is T v ? v : fallback;
    }
}
