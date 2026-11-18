namespace BaroBackend.Infrastructure.Utils;

public interface IStartupTask
{
    Task RunAsync(CancellationToken ct);
}
