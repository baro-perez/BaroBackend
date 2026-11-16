using System.Text.Json;

using BaroBackend.Domain.Metagame;
using BaroBackend.Domain.Metagame.Data;
using BaroBackend.Infrastructure.Configuration;

using Microsoft.Extensions.Options;

namespace BaroBackend.Infrastructure.Catalog;

public class JsonResourceCatalog : IResourceCatalog
{
    private static readonly JsonSerializerOptions ResourceDataSerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IHeroCatalog heroCatalog;
    private readonly string resourcePath;

    private Dictionary<string, ResourceData>? resourceDatas;
    private DictionaryResourceCatalog? snapshot;

    private readonly TaskCompletionSource<bool> readyTcs =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    public bool IsReady => resourceDatas is not null;

    public JsonResourceCatalog(
        IHeroCatalog heroCatalog,
        IOptions<CatalogOptions> catalogOptions)
    {
        this.heroCatalog = heroCatalog;
        this.resourcePath = Path.Combine(AppContext.BaseDirectory, catalogOptions.Value.ResourcesPath);
    }

    public async Task Init(CancellationToken ct)
    {
        await using var jsonStream = File.OpenRead(resourcePath);
        var readData = await JsonSerializer.DeserializeAsync<List<ResourceData>>(
            jsonStream, ResourceDataSerializerOptions, ct);

        if (readData is null)
        {
            throw new InvalidOperationException(
                $"Failed to load resource catalog from '{resourcePath}'.");
        }

        await heroCatalog.WaitUntilReady();
        foreach (var hero in await heroCatalog.GetAll())
        {
            readData.Add(new(
                Resources.HeroTokenId(hero.HeroId),
                Resources.HeroTokenName(hero.Name)));
        }

        readyTcs.TrySetResult(true);
        resourceDatas = readData.ToDictionary(h => h.ResourceId, h => h);
    }

    public Task WaitUntilReady()
    {
        return IsReady ? Task.CompletedTask : readyTcs.Task;
    }

    public IResourceCatalog GetSnapshot()
    {
        return snapshot ??= new DictionaryResourceCatalog(resourceDatas ?? []);
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
