using CBoosterSharp.Generator.Model.Navigation;
using CBoosterSharp.Model.Navigation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace CBoosterSharp.Generator.Generators;

[Generator]
public sealed class RouteGenerator : IIncrementalGenerator
{
    public void Initialize(
        IncrementalGeneratorInitializationContext context
    )
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
                            .Any(a =>
                                a.AttributeClass?.Name
                                    == "RouteAttribute");

                    var hasModule =
                        symbol.GetAttributes()
                            .Any(a =>
                                a.AttributeClass?.Name
                                    == "RouteModuleAttribute");

                    return hasRoute || hasModule
                        ? symbol
                        : null;
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

    private static void Generate(
        SourceProductionContext context,
        INamedTypeSymbol[] symbols
    )
    {
        var modules =
            new Dictionary<string, RouteModuleInfo>(
                StringComparer.Ordinal);

        var routes =
            new List<RouteInfo>();

        // =========================
        // COLLECT
        // =========================

        foreach (var symbol in symbols)
        {
            // ================= MODULES =================

            var moduleAttr =
                symbol.GetAttributes()
                    .FirstOrDefault(a =>
                        a.AttributeClass?.Name
                            == "RouteModuleAttribute");

            if (moduleAttr is not null)
            {
                var name =
                    moduleAttr.ConstructorArguments[0]
                        .Value?.ToString();

                if (!string.IsNullOrWhiteSpace(name) && name is not null)
                {
                    modules[name] =
                        new RouteModuleInfo
                        {
                            Name = name,
                            Namespace =
                                symbol.ContainingNamespace
                                    .ToDisplayString()
                        };
                }
            }

            // ================= ROUTES =================

            var routeAttr =
                symbol.GetAttributes()
                    .FirstOrDefault(a =>
                        a.AttributeClass?.Name
                            == "RouteAttribute");

            if (routeAttr is null)
                continue;

            var module =
                routeAttr.ConstructorArguments[0]
                    .Value?.ToString();

            var path =
                routeAttr.ConstructorArguments[1]
                    .Value?.ToString();

            var scopeValue =
                routeAttr.ConstructorArguments[2];

            var scope =
                scopeValue.Value is int enumValue
                    ? Enum.GetName(typeof(NavigationScope), enumValue)
                    : "App";

            var tag =
                routeAttr.NamedArguments
                    .FirstOrDefault(x => x.Key == "Tag")
                    .Value
                    .Value
                    ?.ToString();

            bool generateHelpers = false;

            var helpersArg =
                routeAttr.NamedArguments
                    .FirstOrDefault(x => x.Key == "GenerateNavigationHelpers");

            if (helpersArg.Value.Value is bool b)
            {
                generateHelpers = b;
            }

            if (string.IsNullOrWhiteSpace(module))
                continue;

            if (string.IsNullOrWhiteSpace(path))
                continue;

            // ================= PARAMETER =================

            RouteParameterInfo? parameter = null;

            var parameterProperties =
                symbol.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(p =>
                        p.GetAttributes()
                            .Any(a =>
                                a.AttributeClass?.Name
                                    == "RouteParameterAttribute"))
                    .ToList();

            if (parameterProperties.Count > 1)
            {
                continue;
            }

            if (parameterProperties.Count == 1)
            {
                var prop = parameterProperties[0];

                parameter =
                    new RouteParameterInfo
                    {
                        Type =
                            prop.Type.ToDisplayString(),

                        PropertyName =
                            prop.Name
                    };
            }

            if (module != null && path != null)
            {
                routes.Add(
                    new RouteInfo
                    {
                        Module = module,
                        Namespace =
                            symbol.ContainingNamespace
                                .ToDisplayString(),

                        ClassName = symbol.Name,

                        Path = path,

                        Scope = scope ?? "App",

                        Tag = tag,

                        Parameter = parameter
                    });
            }
        }

        // =========================
        // GENERATE PER MODULE
        // =========================

        foreach (var module in modules)
        {
            var moduleName = module.Key;

            var moduleInfo = module.Value;

            var moduleRoutes =
                routes
                    .Where(r =>
                        r.Module == moduleName)
                    .ToList();

            if (moduleRoutes.Count == 0)
                continue;

            var sb = new StringBuilder();

            sb.AppendLine("using CBoosterSharp.Navigation.Core;");
            sb.AppendLine("using CBoosterSharp.Navigation.Model;");
            sb.AppendLine();

            sb.AppendLine(
                $"namespace {moduleInfo.Namespace};");

            sb.AppendLine();

            sb.AppendLine(
                $"public sealed class {moduleName}Route : IRoute");

            sb.AppendLine("{");

            sb.AppendLine(
                "    public void DefineRoutes()");

            sb.AppendLine("    {");

            foreach (var route in moduleRoutes)
            {
                sb.AppendLine(
                "        Router.Current");
                // ================= SIMPLE PAGE =================

                if (route.Parameter is null)
                {
                    sb.AppendLine("            .RegisterScreen(");
                    sb.AppendLine("                new RouteDefinition(");
                    sb.AppendLine($"                    Path: {ToLiteral(route.Path)},");
                    sb.AppendLine($"                    Scope: NavigationScope.{route.Scope},");
                    sb.AppendLine($"                    Page: (_) => new {route.ClassName}(),");
                    sb.AppendLine($"                    Tag: {ToLiteral(route.Tag)}");
                    sb.AppendLine("                ));");
                }
                else
                {
                    // ================= PARAMETERIZED PAGE =================

                    var parameterType =
                        route.Parameter.Type;

                    var parameterSimpleName =
                        parameterType.Split('.').Last();

                    var parameterName =
                        ToCamel(parameterSimpleName);

                    sb.AppendLine("            .RegisterScreen(");
                    sb.AppendLine("                new RouteDefinition(");
                    sb.AppendLine($"                    Path: {ToLiteral(route.Path)},");
                    sb.AppendLine($"                    Scope: NavigationScope.{route.Scope},");
                    sb.AppendLine("                    Page: (data) =>");
                    sb.AppendLine("                    {");
                    sb.AppendLine($"                        if (data is not {parameterType} {parameterName})");
                    sb.AppendLine("                        {");
                    sb.AppendLine($"                            throw new ArgumentException(\"Expected navigation parameter of type {parameterType}\");");
                    sb.AppendLine("                        }");
                    sb.AppendLine("");
                    sb.AppendLine($"                        return new {route.ClassName}");
                    sb.AppendLine("                        {");
                    sb.AppendLine($"                            {route.Parameter.PropertyName} = {parameterName}");
                    sb.AppendLine("                        }; ");
                    sb.AppendLine("                    },");
                    sb.AppendLine($"                    Tag: {ToLiteral(route.Tag)}");
                    sb.AppendLine("                ));");
                }
            }

            sb.AppendLine("    }");

            sb.AppendLine("}");

            context.AddSource(
                $"{moduleName}Route.g.cs",
                SourceText.From(
                    sb.ToString(),
                    Encoding.UTF8));
        }
    }

    private static string ToCamel(
        string value
    )
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        if (value.Length == 1)
            return value.ToLowerInvariant();

        return char.ToLowerInvariant(value[0])
            + value.Substring(1);
    }

    private static string ToLiteral(string? value)
    {
        if (value is null)
            return "null";

        // Escape backslashes and quotes
        var escaped = value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        return "\"" + escaped + "\"";
    }
}
