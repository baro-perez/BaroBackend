namespace BaroBackend.Infrastructure;

public interface IStartupTask
{
    Task RunAsync(CancellationToken ct);
}
