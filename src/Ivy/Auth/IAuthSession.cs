using Ivy.Core.HttpTunneling;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Ivy;

public interface IAuthSession : IAuthTokenHandlerSession
{
    public IReadOnlyDictionary<string, IAuthTokenHandlerSession> BrokeredSessions { get; }

    public void AddBrokeredSession(string provider, IAuthTokenHandlerSession session);
    public void RemoveBrokeredSession(string provider);
    public void ClearBrokeredSessions();

    public event Action<string>? BrokeredSessionAdded;
    public event Action<string>? BrokeredSessionRemoved;

    public IReadOnlyDictionary<string, IAuthSession> ConnectedAccounts { get; }

    public void AddConnectedAccount(string provider, IAuthSession session);
    public void RemoveConnectedAccount(string provider);
    public void ClearConnectedAccounts();

    public event Action<string>? ConnectedAccountAdded;
    public event Action<string>? ConnectedAccountRemoved;
}

public class AuthSession(
    AuthToken? authToken = null,
    string? authSessionData = null,
    TunneledHttpMessageHandler? httpMessageHandler = null,
    Dictionary<string, IAuthTokenHandlerSession>? brokeredSessions = null,
    Dictionary<string, IAuthSession>? connectedAccounts = null) : AuthTokenHandlerSession(authToken, authSessionData, httpMessageHandler), IAuthSession
{
    private readonly Dictionary<string, IAuthTokenHandlerSession> _brokeredSessions = brokeredSessions ?? [];
    private readonly Dictionary<string, IAuthSession> _connectedAccounts = connectedAccounts ?? [];

    [ActivatorUtilitiesConstructor]
    public AuthSession(TunneledHttpMessageHandler? httpMessageHandler = null) : this(null, null, httpMessageHandler)
    {
    }

    public IReadOnlyDictionary<string, IAuthTokenHandlerSession> BrokeredSessions => _brokeredSessions;

    public event Action<string>? BrokeredSessionAdded;
    public event Action<string>? BrokeredSessionRemoved;

    public void AddBrokeredSession(string provider, IAuthTokenHandlerSession session)
    {
        var isNew = !_brokeredSessions.ContainsKey(provider);
        _brokeredSessions[provider] = session;
        if (isNew)
        {
            BrokeredSessionAdded?.Invoke(provider);
        }
    }

    public void RemoveBrokeredSession(string provider)
    {
        if (_brokeredSessions.Remove(provider))
        {
            BrokeredSessionRemoved?.Invoke(provider);
        }
    }

    public void ClearBrokeredSessions()
    {
        var providers = _brokeredSessions.Keys.ToList();
        _brokeredSessions.Clear();
        foreach (var provider in providers)
        {
            BrokeredSessionRemoved?.Invoke(provider);
        }
    }

    public IReadOnlyDictionary<string, IAuthSession> ConnectedAccounts => _connectedAccounts;

    public event Action<string>? ConnectedAccountAdded;
    public event Action<string>? ConnectedAccountRemoved;

    public void AddConnectedAccount(string provider, IAuthSession session)
    {
        var isNew = !_connectedAccounts.ContainsKey(provider);
        _connectedAccounts[provider] = session;
        if (isNew)
        {
            ConnectedAccountAdded?.Invoke(provider);
        }
    }

    public void RemoveConnectedAccount(string provider)
    {
        if (_connectedAccounts.Remove(provider))
        {
            ConnectedAccountRemoved?.Invoke(provider);
        }
    }

    public void ClearConnectedAccounts()
    {
        var providers = _connectedAccounts.Keys.ToList();
        _connectedAccounts.Clear();
        foreach (var provider in providers)
        {
            ConnectedAccountRemoved?.Invoke(provider);
        }
    }
}

public readonly struct AuthSessionSnapshot
{
    public readonly AuthToken? AuthToken { get; init; }
    public readonly IReadOnlyDictionary<string, IAuthTokenHandlerSession> BrokeredSessions { get; init; }
    public readonly IReadOnlyDictionary<string, IAuthSession> ConnectedAccounts { get; init; }
    public readonly string? AuthSessionData { get; init; }
}
