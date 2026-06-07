using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace CBoosterSharp.Generator.Generators;

[Generator]
public sealed class DIEntryPointGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var appPipeline =
            context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) =>
                    node is ClassDeclarationSyntax cds &&
                    cds.AttributeLists.Count > 0,

                transform: static (ctx, _) =>
                {
                    var syntax = (ClassDeclarationSyntax)ctx.Node;

                    if (ctx.SemanticModel.GetDeclaredSymbol(syntax)
                        is not INamedTypeSymbol symbol)
                        return ((INamedTypeSymbol, AttributeData)?)null;

                    var attr = symbol.GetAttributes()
                        .FirstOrDefault(a =>
                            a.AttributeClass?.Name == "DIEntryPointAttribute");

                    if (attr == null)
                        return ((INamedTypeSymbol, AttributeData)?)null;

                    return ((INamedTypeSymbol, AttributeData)?)((symbol, attr));
                }
            )
            .Where(static x => x is not null);

        var modulesPipeline =
            context.CompilationProvider.Select(static (compilation, _) =>
            {
                var modules = new List<string>();

                CollectModules(
                    compilation.Assembly.GlobalNamespace,
                    modules
                );

                return modules;
            });

        var combined =
            appPipeline.Combine(modulesPipeline);

        context.RegisterSourceOutput(
            combined,
            (spc, source) =>
            {
                var left = source.Left;
                if (left is null) return;
                var (symbol, attr) = left.Value;
                Generate(
                    spc,
                    symbol,
                    attr,
                    source.Right
                );
            });
    }

    private static void Generate(
        SourceProductionContext context,
        INamedTypeSymbol appSymbol,
        AttributeData attribute,
        List<string> modules
    )
    {
        var ns = appSymbol.ContainingNamespace.ToDisplayString();

        var name =
            attribute.ConstructorArguments.Length > 0
                ? attribute.ConstructorArguments[0].Value?.ToString()
                : "AppDependencies";

        if (string.IsNullOrWhiteSpace(name))
            name = "AppDependencies";

        var registrations = string.Join(
            "\n",
            modules.Select(m =>
                $"            .RegisterModule(new {m}())")
        );

        var code = $@"
using CBoosterSharp.Generator.Container;

namespace {ns};

public static class {name}
{{
    public static void InstallDependencies()
    {{
        DIContainer.Shared
{registrations}
            .Build();
    }}
}}";

        context.AddSource(
            $"{name}.g.cs",
            SourceText.From(code, Encoding.UTF8)
        );
    }

    private static void CollectModules(
        INamespaceSymbol ns,
        List<string> modules
    )
    {
        foreach (var member in ns.GetMembers())
        {
            if (member is INamespaceSymbol childNs)
            {
                CollectModules(childNs, modules);
            }

            if (member is not INamedTypeSymbol type)
                continue;

            var hasUsecaseable =
                type.GetAttributes()
                    .Any(a => a.AttributeClass?.Name == "UsecaseableAttribute");

            var hasModule =
                type.GetAttributes()
                    .Any(a => a.AttributeClass?.Name == "ModuleAttribute");

            var hasInjectable =
                type.GetAttributes()
                    .Any(a => a.AttributeClass?.Name == "InjectableAttribute");

            // ================= USECASE GENERATED DI MODULE =================

            if (hasUsecaseable || hasInjectable)
            {
                var feature = GetFeatureName(type.Name);

                // IMPORTANT:
                // Generated DI modules are emitted in ".Domain"
                var rootNs = type.ContainingNamespace.ToDisplayString();

                var domainNs = rootNs;

                var fqName = $"{domainNs}.{feature}DIModule";

                if (!modules.Contains(fqName))
                    modules.Add(fqName);
            }

            // ================= MANUAL MODULES =================

            if (hasModule)
            {
                var rootNs = type.ContainingNamespace.ToDisplayString();

                // DI namespace -> Domain namespace
                var domainNs = rootNs.Replace(".DI", ".Domain");

                var feature = GetFeatureName(type.Name);

                var fqName = $"{domainNs}.{feature}DIModule";

                if (!modules.Contains(fqName))
                    modules.Add(fqName);
            }
        }
    }

    private static string GetFeatureName(string repo)
    {
        if (string.IsNullOrEmpty(repo)) return repo;

        var name = repo;

        if (name.StartsWith("I") && name.Length > 1)
            name = name.Substring(1);

        if (name.EndsWith("Repository"))
            name = name.Substring(0, name.Length - "Repository".Length);

        if (name.EndsWith("DomainModule"))
            name = name.Substring(0, name.Length - "DomainModule".Length);
        else if (name.EndsWith("Module"))
            name = name.Substring(0, name.Length - "Module".Length);

        return name;
    }
}