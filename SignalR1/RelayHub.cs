using Microsoft.AspNetCore.SignalR;
using System.Text;

namespace SignalR1;

public class RelayHub : Hub
{
    private readonly RelaySessionManager _sessionManager;
    private readonly IBackendChannelFactory _channelFactory;
    private readonly ILogger<RelayHub> _logger;

    public RelayHub( RelaySessionManager sessionManager,
                    IBackendChannelFactory channelFactory,
                    ILogger<RelayHub> logger )
    {
        _sessionManager = sessionManager;
        _channelFactory = channelFactory;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var clientId = Context.UserIdentifier ?? Context.ConnectionId;
        var connectionId = Context.ConnectionId;

        if ( !_sessionManager.TryRegisterClient( clientId, connectionId, out _ ) )
        {
            _logger.LogWarning( "接続拒否: ClientId={ClientId} は既に接続中", clientId );
            await Clients.Caller.SendAsync( "ConnectionRejected", "既にこのクライアントIDは接続されています。" );
            Context.Abort();
            return;
        }

        _logger.LogInformation( "接続: ClientId={ClientId}, ConnectionId={ConnectionId}", clientId, connectionId );
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync( Exception? exception )
    {
        var clientId = Context.UserIdentifier ?? Context.ConnectionId;

        if ( _sessionManager.TryGetClient( clientId, out var context ) )
        {
            foreach ( var channel in context.GetAllChannels() )
            {
                _logger.LogInformation( "セッション切断: チャネル {ChannelId} を削除", channel.ChannelId );
                await channel.StopAsync();
                await channel.DisposeAsync();
            }

            _sessionManager.UnregisterClient( clientId );
            _logger.LogInformation( "切断: ClientId={ClientId}", clientId );
        }

        await base.OnDisconnectedAsync( exception );
    }

    /// <summary>
    /// 明示的にチャネルを作成する
    /// </summary>
    public async Task CreateChannel( string channelId )
    {
        var clientId = Context.UserIdentifier ?? Context.ConnectionId;

        if ( _sessionManager.TryGetClient( clientId, out var context ) )
        {
            if ( context.TryGetChannel( channelId, out _ ) )
            {
                await Clients.Caller.SendAsync( "ChannelError", channelId, "チャネルはすでに存在しています。" );
                return;
            }

            var backendChannel = _channelFactory.CreateChannel(channelId);
            context.TryAddChannel( channelId, backendChannel );

            await backendChannel.StartAsync();
            _logger.LogInformation( "チャネル作成: {ChannelId} by {ClientId}", channelId, clientId );
            await Clients.Caller.SendAsync( "ChannelCreated", channelId );
        }
    }

    /// <summary>
    /// チャネルにデータ送信し、応答を受け取る
    /// </summary>
    public async Task SendToChannel( string channelId, string message )
    {
        var clientId = Context.UserIdentifier ?? Context.ConnectionId;

        if ( _sessionManager.TryGetClient( clientId, out var context ) )
        {
            context.LastActive = DateTime.UtcNow;

            if ( !context.TryGetChannel( channelId, out var backendChannel ) )
            {
                await Clients.Caller.SendAsync( "ChannelError", channelId, "チャネルが存在しません。" );
                return;
            }

            var data = Encoding.UTF8.GetBytes(message);

            try
            {
                await backendChannel.SendAsync( data );

                var response = await backendChannel.ReceiveAsync();
                var responseText = Encoding.UTF8.GetString(response);

                await Clients.Caller.SendAsync( "ReceiveFromChannel", channelId, responseText );
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "チャネル通信エラー: ChannelId={ChannelId}", channelId );
                await Clients.Caller.SendAsync( "ChannelError", channelId, "チャネル通信中にエラーが発生しました。" );
            }
        }
    }

    /// <summary>
    /// 明示的にチャネルを削除する
    /// </summary>
    public async Task CloseChannel( string channelId )
    {
        var clientId = Context.UserIdentifier ?? Context.ConnectionId;

        if ( _sessionManager.TryGetClient( clientId, out var context ) )
        {
            if ( !context.TryGetChannel( channelId, out var backendChannel ) )
            {
                await Clients.Caller.SendAsync( "ChannelError", channelId, "チャネルが存在しません。" );
                return;
            }

            await backendChannel.StopAsync();
            await backendChannel.DisposeAsync();
            context.TryRemoveChannel( channelId );

            _logger.LogInformation( "チャネル削除: {ChannelId} by {ClientId}", channelId, clientId );
            await Clients.Caller.SendAsync( "ChannelClosed", channelId );
        }
    }
}
