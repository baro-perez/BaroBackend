namespace BaroBackend.Infrastructure.Utils;

public interface IIsReadyChecked
{
    Task WaitUntilReady();
    bool IsReady { get; }
}
