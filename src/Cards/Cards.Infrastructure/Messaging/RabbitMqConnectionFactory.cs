using RabbitMQ.Client;

namespace Cards.Infrastructure.Messaging;

public class RabbitMqConnectionFactory : IAsyncDisposable
{
    private IConnection? _connection;

    public IConnection Connection => _connection
                                     ?? throw new InvalidOperationException("Not initialized");

    public async Task InitializeAsync(ConnectionFactory factory, CancellationToken ct)
    {
        _connection = await factory.CreateConnectionAsync(ct);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
