using System.Collections.Concurrent;
using System.Text.Json;

namespace SignalR1;

public class RelaySessionManager
{
    private readonly ConcurrentDictionary<string, ClientRelayContext> _clients = new();
    private readonly ILogger<RelaySessionManager> _logger;

    public RelaySessionManager( ILogger<RelaySessionManager> logger )
    {
        _logger = logger;
    }

    public bool TryRegisterClient( string clientId, string connectionId, out ClientRelayContext context )
    {
        context = new ClientRelayContext( clientId, connectionId );
        return _clients.TryAdd( clientId, context );
    }

    public void UnregisterClient( string clientId )
        => _clients.TryRemove( clientId, out _ );

    public bool TryGetClient( string clientId, out ClientRelayContext context )
        => _clients.TryGetValue( clientId, out context );

    public IEnumerable<ClientRelayContext> GetAllClients() => _clients.Values;

    /// <summary>
    /// 現在の全セッション情報をログに出力します。
    /// </summary>
    public void DumpSessions()
    {
        _logger.LogInformation( "=== RelaySessionManager Dump Start ===" );

        foreach ( var client in _clients.Values )
        {
            _logger.LogInformation( "ClientId: {ClientId}, ConnId: {ConnectionId}, ConnectedAt: {ConnectedAt}, LastActive: {LastActive}",
                client.ClientId, client.ConnectionId, client.ConnectedAt, client.LastActive );

            foreach ( var channel in client.GetAllChannels() )
            {
                _logger.LogInformation( "  - ChannelId: {ChannelId}", channel.ChannelId );
            }
        }

        _logger.LogInformation( "=== RelaySessionManager Dump End ===" );
    }

    /// <summary>
    /// JSON形式でセッションの状態を返します。
    /// </summary>
    public string DumpSessionsAsJson()
    {
        var dump = _clients.Values.Select(client => new
        {
            client.ClientId,
            client.ConnectionId,
            client.ConnectedAt,
            client.LastActive,
            Channels = client.GetAllChannels().Select(ch => new { ch.ChannelId })
        });

        return JsonSerializer.Serialize( dump, new JsonSerializerOptions
        {
            WriteIndented = true
        } );
    }
}
