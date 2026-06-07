namespace CBoosterSharp.Core.Container;

using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

public sealed class AppContainer
{
    private AppContainer() { }

    private static readonly Lazy<AppContainer> lazy =
        new(() => new AppContainer());

    public static AppContainer Shared => lazy.Value;

    private readonly IServiceCollection services = new ServiceCollection();

    private IServiceProvider? serviceProvider;

    private readonly List<IDIModule> _modules = [];

    public void RegisterAsSingleton<TService>(
        Func<IServiceProvider, TService> factory
    )
    where TService : class
    {
        services.AddSingleton(factory);
    }

    public void RegisterAsScoped<TService>(
        Func<IServiceProvider, TService> factory
    )
    where TService : class
    {
        services.AddScoped(factory);
    }

    public void RegisterAsSingleton<TService, TImplementation>()
    where TService : class
    where TImplementation : class, TService
    {
        services.AddSingleton<TService, TImplementation>();
    }

    public void RegisterAsScoped<TService, TImplementation>()
    where TService : class
    where TImplementation : class, TService
    {
        services.AddScoped<TService, TImplementation>();
    }

    public void RegisterAsSingleton<TService>()
    where TService : class
    {
        services.AddSingleton<TService>();
    }

    public void RegisterAsScoped<TService>()
    where TService : class
    {
        services.AddScoped<TService>();
    }

    public void RegisterAsTransient<TService, TImplementation>()
    where TService : class
    where TImplementation : class, TService
    {
        services.AddTransient<TService, TImplementation>();
    }

    public void RegisterAsTransient<TService>()
    where TService : class
    {
        services.AddTransient<TService>();
    }

    public void RegisterAsTransient<TService>(Func<IServiceProvider, TService> factory)
    where TService : class
    {
        services.AddTransient(factory);
    }

    public void Build()
    {
        foreach (IDIModule module in _modules)
        {
            module.Install();
        }

        serviceProvider = services.BuildServiceProvider();

        Ioc.Default.ConfigureServices(serviceProvider);
    }

    public TService? Resolve<TService>()
    {
        if (serviceProvider == null)
            throw new InvalidOperationException("Container not built. Call Build() first.");

        return serviceProvider.GetService<TService>();
    }

    public AppContainer RegisterModule(IDIModule module)
    {
        _modules.Add(module);
        return this;
    }
}
