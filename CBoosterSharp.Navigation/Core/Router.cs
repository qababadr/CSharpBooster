using CBoosterSharp.Navigation.Exceptions;
using CBoosterSharp.Navigation.Model;
using System.Windows.Navigation;

namespace CBoosterSharp.Navigation.Core;

public sealed class Router
{
    public string? CurrentUrl { get; private set; } = string.Empty;
    public string? PreviousUrl { get; private set; } = string.Empty;
    public static Router Current => lazy.Value;
    public event EventHandler? NavigationStateChanged;
    public bool CanGoBackApp => _backStack.Count > 0;
    public bool CanGoForwardApp => _forwardStack.Count > 0;

    private static readonly Lazy<Router> lazy = new(() => new Router());
    private NavigationService? _rootNavigation;
    private NavigationService? _appNavigation;
    private readonly List<IRoute> _routes = [];
    private bool _built;
    private bool _isNavigating;
    private readonly Lock _attachLock = new();
    private readonly Lock _buildLock = new();
    private readonly Lock _navigationLock = new();
    private readonly Stack<string> _backStack = new();
    private readonly Stack<string> _forwardStack = new();

    private readonly Dictionary<string, RouteDefinition> _screenLookup
        = new(StringComparer.OrdinalIgnoreCase);

    private Router() { }

    public void AttachRoot(NavigationService navigation)
    {
        lock (_attachLock)
        {
            _rootNavigation ??= navigation;
            HookNavigationEvents(_rootNavigation);
        }
    }

    public void AttachApp(NavigationService navigation)
    {
        lock (_attachLock)
        {
            _appNavigation ??= navigation;
            HookNavigationEvents(_appNavigation);
        }
    }

    private void HookNavigationEvents(NavigationService nav)
    {
        nav.Navigated -= OnNavigationStateChanged;
        nav.Navigating -= OnNavigationStateChanged;

        nav.Navigated += OnNavigationStateChanged;
        nav.Navigating += OnNavigationStateChanged;
    }


    public void DetachRootNavigationService()
    {
        if (_rootNavigation is not null)
        {
            _rootNavigation.Navigated -= OnNavigationStateChanged;
            _rootNavigation.Navigating -= OnNavigationStateChanged;
        }

        _rootNavigation = null;
    }

    public void DetachAppNavigationService()
    {
        if (_appNavigation is not null)
        {
            _appNavigation.Navigated -= OnNavigationStateChanged;
            _appNavigation.Navigating -= OnNavigationStateChanged;
        }

        _appNavigation = null;
    }

    public void GoBackApp()
    {
        NavigationService navigation = RequireAppNavigation();
        GoBack(navigation);
    }

    public void GoBackRoot()
    {
        NavigationService navigation = RequireRootNavigation();
        GoBack(navigation);
    }

    public void GoForwardApp()
    {
        NavigationService navigation = RequireAppNavigation();
        GoForward(navigation);
    }

    public void GoForwardRoot()
    {
        NavigationService navigation = RequireRootNavigation();
        GoForward(navigation);
    }

    public void Navigate(
        string url,
        object? data = null,
        Action<string>? onNavigating = null
    )
    {
        ThrowRouterExceptionIfNeeded();

        lock (_navigationLock)
        {
            if (_isNavigating) return;

            _isNavigating = true;
        }

        try
        {

            var match = Match(url)
                ?? throw new NavigationException($"{url} is not defined in routes definition");

            if (match.Definition.Tag is not null)
                onNavigating?.Invoke(match.Definition.Tag);

            NavigationService navigation = GetNavigation(match.Definition);

            var page = match.Definition.Page(data);

            if (!string.IsNullOrEmpty(CurrentUrl))
                _backStack.Push(CurrentUrl);

            _forwardStack.Clear();

            PreviousUrl = CurrentUrl;
            CurrentUrl = match.Definition.Path;

            navigation.Navigate(page);

            RaiseNavigationStateChanged();
        }
        catch (Exception exp)
        {
            throw new NavigationException("Router exception", exp);
        }
        finally
        {
            lock (_navigationLock)
            {
                _isNavigating = false;
            }
        }
    }


    public Router RegisterRoute(IRoute route)
    {
        lock (_buildLock)
        {
            if (_built)
                throw new InvalidOperationException("Cannot register routes after Build");

            _routes.Add(route);
        }
        return this;
    }

    public void RegisterScreen(RouteDefinition screen)
    {
        var key = screen.Path.Trim('/');

        if (_screenLookup.ContainsKey(key))
            throw new RouterException($"A screen with the path '{key}' is already registered.");

        _screenLookup[key] = screen;
    }

    public void Build()
    {
        lock (_buildLock)
        {
            if (_built)
                throw new InvalidOperationException("Router is already built");

            foreach (var route in _routes)
            {
                route.DefineRoutes();
            }

            _built = true;
        }
    }

    private void ThrowRouterExceptionIfNeeded()
    {
        if (_screenLookup.Count == 0)
            throw new RouterException("No screens are registered in the router");

        if (!_built)
            throw new RouterException("Router.Build() must be called before navigation");
    }

    private void RaiseNavigationStateChanged()
        => NavigationStateChanged?.Invoke(this, EventArgs.Empty);

    private RouteMatch? Match(string url)
    {
        url = url.Trim('/');
        if (_screenLookup.TryGetValue(url, out var screen))
            return new RouteMatch(screen, new Dictionary<string, string>());
        return null;
    }

    private void OnNavigationStateChanged(object? sender, EventArgs e)
        => RaiseNavigationStateChanged();

    private NavigationService GetNavigation(RouteDefinition definition)
    {
        return definition.Scope switch
        {
            NavigationScope.Root when _rootNavigation != null => _rootNavigation,
            NavigationScope.App when _appNavigation != null => _appNavigation,
            _ => throw new RouterException(
                $"NavigationService for scope '{definition.Scope}' is not attached"
            )
        };
    }

    private void GoBack(NavigationService? navigation)
    {
        if (navigation == null || !navigation.CanGoBack || _backStack.Count == 0)
            return;

        // Push current page into forward stack
        if (!string.IsNullOrEmpty(CurrentUrl))
            _forwardStack.Push(CurrentUrl);

        // Pop previous page from back stack
        var previous = _backStack.Pop();

        PreviousUrl = CurrentUrl;
        CurrentUrl = previous;

        navigation.GoBack();

        RaiseNavigationStateChanged();
    }

    private void GoForward(NavigationService? navigation)
    {
        if (navigation == null || !navigation.CanGoForward || _forwardStack.Count == 0)
            return;

        // Push current page into back stack
        if (!string.IsNullOrEmpty(CurrentUrl))
            _backStack.Push(CurrentUrl);

        // Pop next page from forward stack
        var next = _forwardStack.Pop();

        PreviousUrl = CurrentUrl;
        CurrentUrl = next;

        navigation.GoForward();

        RaiseNavigationStateChanged();
    }

    private NavigationService RequireAppNavigation()
    {
        return _appNavigation
            ?? throw new RouterException("App NavigationService is not attached");
    }

    private NavigationService RequireRootNavigation()
    {
        return _rootNavigation
            ?? throw new RouterException("Root NavigationService is not attached");
    }
}
