namespace SignalR1;

public interface IBackendChannelFactory
{
    IBackendChannel CreateChannel( string channelId );
}
