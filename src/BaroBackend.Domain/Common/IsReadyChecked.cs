namespace BaroBackend.Domain.Common;

public interface IIsReadyChecked
{
    Task WaitUntilReady();
    bool IsReady { get; }
}
