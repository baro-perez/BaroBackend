using BaroBackend.Domain.Metagame.Data;
using BaroBackend.Domain.Utils;

namespace BaroBackend.Infrastructure.Catalog;

public class DictionaryResourceCatalog : IResourceCatalog
{
    private readonly Dictionary<string, ResourceData> resourceDatas;
    private DictionaryResourceCatalog? snapshot;
    public bool IsReady => true;

    public DictionaryResourceCatalog(IReadOnlyDictionary<string, ResourceData> source)
    {
        this.resourceDatas = source.Clone();
    }

    public IResourceCatalog GetSnapshot()
    {
        return snapshot ??= new DictionaryResourceCatalog(resourceDatas);
    }

    public Task WaitUntilReady()
    {
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<ResourceData>> GetAll()
    {
        IReadOnlyCollection<ResourceData> result =
            resourceDatas is null ? [] : resourceDatas.Values.ToArray();
        return Task.FromResult(result);
    }

    public Task<ResourceData?> GetResourceById(string resourceId)
    {
        return resourceDatas is null
            ? Task.FromResult<ResourceData?>(null)
            : Task.FromResult(resourceDatas.TryGetValue(resourceId, out var result) ? result : null);
    }

    public Task<bool> ResourceIdIsValid(string resourceId)
    {
        return resourceDatas is null
            ? Task.FromResult(false)
            : Task.FromResult(resourceDatas.ContainsKey(resourceId));
    }
}
