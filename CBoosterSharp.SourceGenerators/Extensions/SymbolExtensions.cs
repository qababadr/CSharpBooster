using Microsoft.CodeAnalysis;

namespace CBoosterSharp.Generator.Extensions;

internal static class SymbolExtensions
{
    public static bool HasAttribute(this ISymbol symbol, string attributeName)
    {
        return symbol.GetAttributes()
            .Any(a => a.AttributeClass?.Name == attributeName);
    }

    public static AttributeData? GetAttribute(this ISymbol symbol, string attributeName)
    {
        return symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == attributeName);
    }
}
