using Ivy.Core;
using Ivy.Core.Auth;
using Ivy.Core.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Ivy;

public class ConnectedAccountsService : IConnectedAccountsService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAuthSession _authSession;
    private readonly AppSessionStore _sessionStore;

    public ConnectedAccountsService(
        IServiceProvider serviceProvider,
        IAuthSession authSession,
        AppSessionStore sessionStore)
    {
        _serviceProvider = serviceProvider;
        _authSession = authSession;
        _sessionStore = sessionStore;
    }

    public string[] GetAvailableProviders()
    {
        // Get all keyed IAuthProvider registrations
        var keyedProviders = _serviceProvider.GetKeyedServices<IAuthProvider>(null);
        return keyedProviders
            .Select(kvp => kvp.ToString() ?? string.Empty)
            .Where(key => !string.IsNullOrEmpty(key))
            .ToArray();
    }

    public async Task<Uri> ConnectAccountAsync(string provider, WebhookEndpoint callback, CancellationToken cancellationToken = default)
    {
        var authProvider = _serviceProvider.GetKeyedService<IAuthProvider>(provider)
            ?? throw new InvalidOperationException($"Connected account provider '{provider}' is not registered.");

        var connectedSession = _authSession.ConnectedAccounts.TryGetValue(provider, out var existing)
            ? existing
            : new AuthSession(httpMessageHandler: null);

        var authOptions = authProvider.GetAuthOptions();
        if (authOptions.Length == 0)
        {
            throw new InvalidOperationException($"Provider '{provider}' has no auth options configured.");
        }

        // Use first auth option by default
        return await authProvider.GetOAuthUriAsync(connectedSession, authOptions[0], callback, cancellationToken);
    }

    public async Task<AuthToken?> HandleConnectCallbackAsync(string provider, HttpRequest request, CancellationToken cancellationToken = default)
    {
        var authProvider = _serviceProvider.GetKeyedService<IAuthProvider>(provider)
            ?? throw new InvalidOperationException($"Connected account provider '{provider}' is not registered.");

        var connectedSession = _authSession.ConnectedAccounts.TryGetValue(provider, out var existing)
            ? existing
            : new AuthSession(httpMessageHandler: null);

        var token = await authProvider.HandleOAuthCallbackAsync(connectedSession, request, cancellationToken);
        if (token != null)
        {
            connectedSession.AuthToken = token;
            _authSession.AddConnectedAccount(provider, connectedSession);

            // Persist to cookies
            _sessionStore.RegisterAuthSessionCookies(_authSession);
        }

        return token;
    }

    public async Task DisconnectAccountAsync(string provider, CancellationToken cancellationToken = default)
    {
        if (!_authSession.ConnectedAccounts.ContainsKey(provider))
        {
            return;
        }

        _authSession.RemoveConnectedAccount(provider);

        // Persist to cookies
        _sessionStore.RegisterAuthSessionCookies(_authSession, connectedAccountsToDelete: [provider]);

        await Task.CompletedTask;
    }

    public IAuthSession? GetAccountSession(string provider)
    {
        return _authSession.ConnectedAccounts.TryGetValue(provider, out var session)
            ? session
            : null;
    }

    public async Task RefreshAllAsync(CancellationToken cancellationToken = default)
    {
        var refreshTasks = new List<Task>();

        foreach (var (provider, session) in _authSession.ConnectedAccounts)
        {
            if (session.AuthToken == null || string.IsNullOrEmpty(session.AuthToken.AccessToken))
            {
                continue;
            }

            var authProvider = _serviceProvider.GetKeyedService<IAuthProvider>(provider);
            if (authProvider == null)
            {
                continue;
            }

            // Check if token needs refresh (within 5 minutes of expiry)
            var lifetime = await authProvider.GetAccessTokenLifetimeAsync(session, cancellationToken);
            if (lifetime?.Expires == null || lifetime.Expires > DateTimeOffset.UtcNow.AddMinutes(5))
            {
                continue;
            }

            refreshTasks.Add(RefreshConnectedAccountAsync(provider, authProvider, session, cancellationToken));
        }

        await Task.WhenAll(refreshTasks);
    }

    private async Task RefreshConnectedAccountAsync(string provider, IAuthProvider authProvider, IAuthSession session, CancellationToken cancellationToken)
    {
        try
        {
            var refreshedToken = await authProvider.RefreshAccessTokenAsync(session, cancellationToken);
            if (refreshedToken != null)
            {
                session.AuthToken = refreshedToken;
                _authSession.AddConnectedAccount(provider, session);

                // Persist to cookies
                _sessionStore.RegisterAuthSessionCookies(_authSession);
            }
        }
        catch (Exception)
        {
            // Log error and continue - don't fail all refreshes if one fails
            // TODO: Add logging
        }
    }
}
