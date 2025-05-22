namespace SignalR1;

public class DummyBackendChannel : IBackendChannel
{
    public string ChannelId { get; }

    private readonly ILogger<DummyBackendChannel> _logger;

    public DummyBackendChannel( string channelId, ILogger<DummyBackendChannel> logger )
    {
        ChannelId = channelId;
        _logger = logger;
    }

    public ValueTask DisposeAsync()
    {
        _logger.LogInformation( "DisposeAsync called for ChannelId: {ChannelId}", ChannelId );
        return ValueTask.CompletedTask;
    }

    public Task<byte[]> ReceiveAsync( CancellationToken cancellationToken = default )
    {
        _logger.LogInformation( "ReceiveAsync called for ChannelId: {ChannelId}", ChannelId );
        return Task.FromResult( Array.Empty<byte>() );
    }

    public Task SendAsync( ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default )
    {
        _logger.LogInformation( "SendAsync called for ChannelId: {ChannelId}, DataLength: {Length}", ChannelId, data.Length );
        return Task.CompletedTask;
    }

    public Task StartAsync( CancellationToken cancellationToken = default )
    {
        _logger.LogInformation( "StartAsync called for ChannelId: {ChannelId}", ChannelId );
        return Task.CompletedTask;
    }

    public Task StopAsync( CancellationToken cancellationToken = default )
    {
        _logger.LogInformation( "StopAsync called for ChannelId: {ChannelId}", ChannelId );
        return Task.CompletedTask;
    }
}
