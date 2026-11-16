namespace BaroBackend.Domain.Utils;

public static class DictionaryUtils
{
    public static Dictionary<TKey, TValue> Clone<TKey,TValue>(this IReadOnlyDictionary<TKey,TValue> source)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, TValue>();
        foreach(var (k, v) in source)
        {
            result.Add(k, v);
        }
        return result;
    }
}
