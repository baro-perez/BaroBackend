using BaroBackend.Domain.Metagame.Data;
using BaroBackend.Domain.Metagame.Data.Catalog;
using BaroBackend.Domain.Utils;

namespace BaroBackend.Infrastructure.Catalog;

public class DictionaryResourceCatalog : IResourceCatalog
{
    private readonly IReadOnlyDictionary<string, ResourceData> resourceDatas;

    /// <param name="source">Gets copied</param>
    public DictionaryResourceCatalog(IReadOnlyDictionary<string, ResourceData> source)
    {
        this.resourceDatas = source.Clone();
    }

    public IReadOnlyCollection<ResourceData> GetAll()
    {
        return resourceDatas is null ? [] : resourceDatas.Values.ToArray();
    }

    public ResourceData? GetResourceById(string resourceId)
    {
        return (resourceDatas?.TryGetValue(resourceId, out var result) ?? false) ? result : null;
    }

    public bool ResourceIdIsValid(string resourceId)
    {
        return resourceDatas?.ContainsKey(resourceId) ?? false;
    }
}
