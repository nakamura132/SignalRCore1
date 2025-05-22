using Xunit;
using SignalR1;
using Microsoft.Extensions.Logging.Abstractions;

namespace SignalR1.Tests;

public class RelaySessionManagerTests
{
    [Fact]
    public void RegisterClient_ShouldSucceed_WhenNewClient()
    {
        // Arrange
        var logger = NullLogger<RelaySessionManager>.Instance;
        var manager = new RelaySessionManager(logger);

        var clientId = "test-client-1";
        var connectionId = "conn-abc";

        // Act
        var success = manager.TryRegisterClient(clientId, connectionId, out var context);

        // Assert
        Assert.True( success );
        Assert.Equal( clientId, context.ClientId );
        Assert.Equal( connectionId, context.ConnectionId );
    }

    [Fact]
    public void RegisterClient_ShouldFail_WhenDuplicateClient()
    {
        var logger = NullLogger<RelaySessionManager>.Instance;
        var manager = new RelaySessionManager(logger);

        var clientId = "duplicate-client";
        var conn1 = "conn-1";
        var conn2 = "conn-2";

        var first = manager.TryRegisterClient(clientId, conn1, out _);
        var second = manager.TryRegisterClient(clientId, conn2, out _);

        Assert.True( first );
        Assert.False( second );
    }

    [Fact]
    public void DumpSessionsAsJson_ShouldReturnValidJson()
    {
        var logger = NullLogger<RelaySessionManager>.Instance;
        var manager = new RelaySessionManager(logger);

        manager.TryRegisterClient( "c1", "conn-1", out var ctx );
        ctx.TryAddChannel( "ch1", new MockBackendChannel( "ch1" ) );
        ctx.TryAddChannel( "ch2", new MockBackendChannel( "ch2" ) );

        var json = manager.DumpSessionsAsJson();

        Assert.Contains( "c1", json );
        Assert.Contains( "ch1", json );
        Assert.Contains( "ch2", json );
    }

    private class MockBackendChannel : IBackendChannel
    {
        public string ChannelId { get; }

        public MockBackendChannel( string id )
        {
            ChannelId = id;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        public Task SendAsync( ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default ) => Task.CompletedTask;
        public Task<byte[]> ReceiveAsync( CancellationToken cancellationToken = default ) => Task.FromResult( new byte[] { } );
        public Task StartAsync( CancellationToken cancellationToken = default ) => Task.CompletedTask;
        public Task StopAsync( CancellationToken cancellationToken = default ) => Task.CompletedTask;
    }
}
