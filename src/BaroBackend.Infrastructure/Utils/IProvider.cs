namespace BaroBackend.Infrastructure.Utils;

public interface IProvider<T>
{
    T GetObject();
}