namespace BaroBackend.Infrastructure;

public sealed class StartupTask(Func<CancellationToken, Task> run) : IStartupTask
{
    public Task RunAsync(CancellationToken ct) => run(ct);
}