using System.Collections.Concurrent;

namespace SignalR1
{
    public class ClientRelayContext
    {
        public string ClientId { get; }
        public string ConnectionId { get; }
        public DateTime ConnectedAt { get; }
        public DateTime LastActive { get; set; }

        private readonly ConcurrentDictionary<string, IBackendChannel> _channels = new();

        public ClientRelayContext( string clientId, string connectionId )
        {
            ClientId = clientId;
            ConnectionId = connectionId;
            ConnectedAt = DateTime.UtcNow;
            LastActive = ConnectedAt;
        }

        public bool TryAddChannel( string channelId, IBackendChannel channel )
            => _channels.TryAdd( channelId, channel );

        public bool TryRemoveChannel( string channelId )
            => _channels.TryRemove( channelId, out _ );

        public bool TryGetChannel( string channelId, out IBackendChannel channel )
            => _channels.TryGetValue( channelId, out channel );

        public IEnumerable<IBackendChannel> GetAllChannels()
            => _channels.Values;
    }
}
