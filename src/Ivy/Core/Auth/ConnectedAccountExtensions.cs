using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Core.Auth;

public static class ConnectedAccountExtensions
{
    /// <summary>
    /// Registers a connected account provider that can be used independently of the main auth provider.
    /// Connected accounts persist separately and are not cleared when the user logs out of the main auth.
    /// </summary>
    /// <typeparam name="TProvider">The IAuthProvider implementation for this connected account.</typeparam>
    /// <param name="server">The Ivy server instance.</param>
    /// <param name="providerKey">The unique key for this provider (e.g., "github", "linear").</param>
    /// <returns>The server instance for method chaining.</returns>
    public static global::Ivy.Server AddConnectedAccountProvider<TProvider>(this global::Ivy.Server server, string providerKey)
        where TProvider : class, IAuthProvider
    {
        server.Services.AddKeyedScoped<IAuthProvider>(providerKey, (sp, _) =>
            ActivatorUtilities.CreateInstance<TProvider>(sp));
        return server;
    }
}
