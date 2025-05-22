namespace SignalR1
{
    public interface IBackendChannel : IAsyncDisposable
    {
        string ChannelId { get; }

        Task SendAsync( ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default );
        Task<byte[]> ReceiveAsync( CancellationToken cancellationToken = default );

        Task StartAsync( CancellationToken cancellationToken = default ); // 実接続 or モック接続開始
        Task StopAsync( CancellationToken cancellationToken = default );  // 接続終了
    }
}
