using CBoosterSharp.Generator.Model.Navigation;
using CBoosterSharp.Model.Navigation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace CBoosterSharp.Generator.Generators;

[Generator]
public sealed class NavigationHelpersExtensionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline =
            context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) =>
                    node is ClassDeclarationSyntax cds &&
                    cds.AttributeLists.Count > 0,

                transform: static (ctx, _) =>
                {
                    if (ctx.SemanticModel.GetDeclaredSymbol(ctx.Node)
                        is not INamedTypeSymbol symbol)
                        return null;

                    var hasRoute =
                        symbol.GetAttributes()
                            .Any(a => a.AttributeClass?.Name == "RouteAttribute");

                    return hasRoute ? symbol : null;
                }
            )
            .Where(static x => x is not null)
            .Collect();

        context.RegisterSourceOutput(
            pipeline,
            static (spc, source) =>
            {
                var symbols = source
                    .Where(s => s is not null)
                    .Select(s => s!)
                    .ToArray();

                if (symbols.Length == 0)
                    return;

                Generate(spc, symbols);
            });
    }

    private static void Generate(SourceProductionContext context, INamedTypeSymbol[] symbols)
    {
        var routes = new List<RouteInfo>();

        foreach (var symbol in symbols)
        {
            var attr = symbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "RouteAttribute");

            if (attr is null)
                continue;

            var module = attr.ConstructorArguments[0].Value?.ToString();
            var path = attr.ConstructorArguments[1].Value?.ToString();
            var scope = attr.ConstructorArguments[2].Value?.ToString();

            var tag = attr.NamedArguments
                .FirstOrDefault(x => x.Key == "Tag")
                .Value.Value?.ToString();

            var generateHelpers =
                attr.NamedArguments
                    .FirstOrDefault(x => x.Key == "GenerateNavigationHelpers")
                    .Value.Value as bool? ?? false;

            if (!generateHelpers)
                continue;

            // =========================
            // Route Parameter detection
            // =========================
            RouteParameterInfo? parameter = null;

            var parameterProperty =
                symbol.GetMembers()
                    .OfType<IPropertySymbol>()
                    .FirstOrDefault(p =>
                        p.GetAttributes()
                            .Any(a =>
                                a.AttributeClass?.Name == "RouteParameterAttribute"));

            if (parameterProperty != null)
            {
                parameter = new RouteParameterInfo
                {
                    Type = parameterProperty.Type.ToDisplayString(),
                    PropertyName = parameterProperty.Name
                };
            }

            routes.Add(new RouteInfo
            {
                Module = module!,
                Namespace = symbol.ContainingNamespace.ToDisplayString(),
                ClassName = symbol.Name,
                Path = path!,
                Scope = scope ?? "App",
                Tag = tag,
                Parameter = parameter,
                GenerateNavigationHelpers = true
            });
        }

        var grouped = routes.GroupBy(r => r.Module);

        foreach (var group in grouped)
        {
            var moduleName = group.Key;

            var sb = new StringBuilder();

            sb.AppendLine("using CBoosterSharp.Navigation.Core;");
            sb.AppendLine();

            foreach (var ns in group.Select(r => r.Namespace).Distinct())
                sb.AppendLine($"using {ns};");

            sb.AppendLine();
            sb.AppendLine("namespace CBoosterSharp.Navigation.Generated;");
            sb.AppendLine();

            sb.AppendLine($"public static class {moduleName}NavigationHelpers");
            sb.AppendLine("{");

            foreach (var route in group)
            {
                var methodName = $"NavigateTo{route.ClassName}";

                // =========================
                // SIMPLE ROUTE
                // =========================
                if (route.Parameter is null)
                {
                    sb.AppendLine(
                        $"    public static void {methodName}(this Router router, Action<string>? onNavigating = null)");
                    sb.AppendLine("    {");
                    sb.AppendLine(
                        $"        router.Navigate(\"{route.Path}\", null, onNavigating);");
                    sb.AppendLine("    }");
                    sb.AppendLine();
                }

                // =========================
                // PARAMETER ROUTE
                // =========================
                else
                {
                    var type = route.Parameter.Type;
                    var paramName = ToCamel(route.Parameter.PropertyName);

                    sb.AppendLine(
                        $"    public static void {methodName}(this Router router, {type} {paramName}, Action<string>? onNavigating = null)");
                    sb.AppendLine("    {");
                    sb.AppendLine(
                        $"        router.Navigate(\"{route.Path}\", {paramName}, onNavigating);");
                    sb.AppendLine("    }");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("}");

            context.AddSource(
                $"{moduleName}NavigationHelpers.g.cs",
                SourceText.From(sb.ToString(), Encoding.UTF8)
            );
        }
    }

    private static string ToCamel(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        if (value.Length == 1)
            return value.ToLowerInvariant();

        return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }
}