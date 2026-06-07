using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace CBoosterSharp.Generator.Generators;

[Generator]
public sealed class RouteEntryPointGenerator
    : IIncrementalGenerator
{
    public void Initialize(
        IncrementalGeneratorInitializationContext context
    )
    {
        // =========================
        // ENTRY POINTS
        // =========================

        var entryPointsPipeline =
            context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) =>
                    node is ClassDeclarationSyntax cds &&
                    cds.AttributeLists.Count > 0,

                transform: static (ctx, _) =>
                {
                    if (ctx.SemanticModel.GetDeclaredSymbol(ctx.Node)
                        is not INamedTypeSymbol symbol)
                    {
                        return null;
                    }

                    var attr =
                        symbol.GetAttributes()
                            .FirstOrDefault(a =>
                                a.AttributeClass?.Name
                                    == "RouteEntryPointAttribute");

                    return attr is not null
                        ? (Symbol: symbol, Attribute: attr)
                        : ((INamedTypeSymbol Symbol, AttributeData Attribute)?)null;
                }
            )
            .Where(static x => x is not null)
            .Collect();

        // =========================
        // ROUTE MODULES
        // =========================

        var modulesPipeline =
            context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) =>
                    node is ClassDeclarationSyntax cds &&
                    cds.AttributeLists.Count > 0,

                transform: static (ctx, _) =>
                {
                    if (ctx.SemanticModel.GetDeclaredSymbol(ctx.Node)
                        is not INamedTypeSymbol symbol)
                    {
                        return null;
                    }

                    var attr =
                        symbol.GetAttributes()
                            .FirstOrDefault(a =>
                                a.AttributeClass?.Name
                                    == "RouteModuleAttribute");

                    return attr is not null
                        ? symbol
                        : null;
                }
            )
            .Where(static x => x is not null)
            .Collect();

        // =========================
        // COMBINE
        // =========================

        var combined =
            entryPointsPipeline.Combine(modulesPipeline);

        context.RegisterSourceOutput(
            combined,
            static (spc, source) =>
            {
                var entryPoints =
                    source.Left
                        .Where(x => x is not null)
                        .Select(x => x!.Value)
                        .ToArray();

                var modules =
                    source.Right
                        .Where(x => x is not null)
                        .Select(x => x!)
                        .ToArray();

                if (entryPoints.Length == 0)
                    return;

                Generate(
                    spc,
                    entryPoints,
                    modules
                );
            });
    }

    private static void Generate(
        SourceProductionContext context,
        (INamedTypeSymbol Symbol, AttributeData Attribute)[] entryPoints,
        INamedTypeSymbol[] modules
    )
    {
        // =========================
        // COLLECT ROUTES
        // =========================

        var routes =
            new List<(string Namespace, string RouteName)>();

        foreach (var module in modules)
        {
            var attr =
                module.GetAttributes()
                    .FirstOrDefault(a =>
                        a.AttributeClass?.Name
                            == "RouteModuleAttribute");

            if (attr is null)
                continue;

            var moduleName =
                attr.ConstructorArguments[0]
                    .Value?.ToString();

            if (string.IsNullOrWhiteSpace(moduleName))
                continue;

            routes.Add((
                Namespace:
                    module.ContainingNamespace
                        .ToDisplayString(),

                RouteName:
                    $"{moduleName}Route"
            ));
        }

        // =========================
        // GENERATE PER ENTRY POINT
        // =========================

        foreach (var entryPoint in entryPoints)
        {
            var symbol =
                entryPoint.Symbol;

            var attr =
                entryPoint.Attribute;

            var ns =
                symbol.ContainingNamespace
                    .ToDisplayString();

            var entryPointName =
                attr.ConstructorArguments[0]
                    .Value?.ToString();

            if (string.IsNullOrWhiteSpace(entryPointName))
                continue;

            var sb = new StringBuilder();

            sb.AppendLine(
                "using CBoosterSharp.Navigation.Core;");
            sb.AppendLine();

            sb.AppendLine(
                $"namespace {ns};");

            sb.AppendLine();

            sb.AppendLine(
                $"public static class {entryPointName}");

            sb.AppendLine("{");

            sb.AppendLine(
                "    public static void RegisterRoutes()");

            sb.AppendLine("    {");

            sb.AppendLine(
                "        Router.Current");

            foreach (var route in routes.Distinct())
            {
                var fqRoute =
                    $"{route.Namespace}.{route.RouteName}";

                sb.AppendLine(
                    $"            .RegisterRoute(new {fqRoute}())");
            }

            sb.AppendLine(
                "            .Build();");

            sb.AppendLine("    }");

            sb.AppendLine("}");

            context.AddSource(
                $"{entryPointName}.g.cs",
                SourceText.From(
                    sb.ToString(),
                    Encoding.UTF8
                )
            );
        }
    }
}