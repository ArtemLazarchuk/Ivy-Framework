using Ivy.Core;
using Microsoft.AspNetCore.Http;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IConnectedAccountsService
{
    /// <summary>
    /// Lists the keys of all registered connected account providers.
    /// </summary>
    string[] GetAvailableProviders();

    /// <summary>
    /// Initiates an OAuth flow for connecting an account with the specified provider.
    /// </summary>
    /// <param name="provider">The provider key (e.g., "github", "linear").</param>
    /// <param name="callback">The callback endpoint to return to after OAuth completion.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The authorization URI to redirect the user to.</returns>
    Task<Uri> ConnectAccountAsync(string provider, WebhookEndpoint callback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles the OAuth callback after the user authorizes the connected account.
    /// </summary>
    /// <param name="provider">The provider key.</param>
    /// <param name="request">The HTTP request containing OAuth callback parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The auth token for the connected account, or null if the callback failed.</returns>
    Task<AuthToken?> HandleConnectCallbackAsync(string provider, HttpRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects (removes) a connected account for the specified provider.
    /// </summary>
    /// <param name="provider">The provider key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DisconnectAccountAsync(string provider, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the auth session for a connected account.
    /// </summary>
    /// <param name="provider">The provider key.</param>
    /// <returns>The auth session for the connected account, or null if not connected.</returns>
    IAuthSession? GetAccountSession(string provider);

    /// <summary>
    /// Refreshes all connected accounts that need refreshing.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RefreshAllAsync(CancellationToken cancellationToken = default);
}
