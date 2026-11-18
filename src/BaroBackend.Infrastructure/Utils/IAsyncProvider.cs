namespace BaroBackend.Infrastructure.Utils;

public interface IAsyncProvider<T>
{
    Task<T> GetObject();
}