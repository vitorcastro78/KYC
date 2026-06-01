namespace KYC.Web.Services;

/// <summary>
/// Serializa operações EF/MediatR no mesmo circuito Blazor Server (um DbContext scoped por ligação).
/// Componentes filhos devem usar <c>OnChanged</c> sem voltar a entrar no gate (ex.: <c>ReloadUngated</c>).
/// </summary>
public interface ICircuitDbGate
{
    Task RunAsync(Func<Task> action, CancellationToken cancellationToken = default);

    Task<T> RunAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);
}

public sealed class CircuitDbGate : ICircuitDbGate, IAsyncDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _disposed;

    public Task RunAsync(Func<Task> action, CancellationToken cancellationToken = default) =>
        RunAsync(async () =>
        {
            await action();
            return true;
        }, cancellationToken);

    public async Task<T> RunAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(CircuitDbGate));

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(CircuitDbGate));

            return await action();
        }
        finally
        {
            try
            {
                _semaphore.Release();
            }
            catch (ObjectDisposedException)
            {
                // ignore on teardown
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        _disposed = true;
        _semaphore.Dispose();
        return ValueTask.CompletedTask;
    }
}
