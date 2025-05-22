namespace SignalR1;

public class DummyBackendChannelFactory : IBackendChannelFactory
{
    private readonly ILogger<DummyBackendChannel> _logger;

    public DummyBackendChannelFactory( ILogger<DummyBackendChannel> logger )
    {
        _logger = logger;
    }

    public IBackendChannel CreateChannel( string channelId )
        => new DummyBackendChannel( channelId, _logger );
}
