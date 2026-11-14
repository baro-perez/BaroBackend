namespace BaroBackend.Infrastructure;

public interface IReadyChecked
{
    bool IsReady { get; }
}