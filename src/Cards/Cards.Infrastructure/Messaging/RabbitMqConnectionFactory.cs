using RabbitMQ.Client;

namespace Cards.Infrastructure.Messaging;

/// <summary>
/// Holds a single shared RabbitMQ <see cref="IConnection"/> initialized on startup and disposed on shutdown.
/// </summary>
public class RabbitMqConnectionFactory : IAsyncDisposable
{
    private IConnection? _connection;

    /// <summary>Gets the initialized connection; throws <see cref="InvalidOperationException"/> if <see cref="InitializeAsync"/> has not been called.</summary>
    public IConnection Connection => _connection
                                     ?? throw new InvalidOperationException("Not initialized");

    /// <summary>Opens and stores the underlying RabbitMQ connection.</summary>
    public async Task InitializeAsync(ConnectionFactory factory, CancellationToken ct)
    {
        _connection = await factory.CreateConnectionAsync(ct);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
