namespace BaroBackend.Domain.Common;

public interface IIsSnapshotable<T>
{
    T GetSnapshot();
}