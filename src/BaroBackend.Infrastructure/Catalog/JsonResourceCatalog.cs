using System.Text.Json;

using BaroBackend.Domain.Metagame;
using BaroBackend.Domain.Metagame.Data;
using BaroBackend.Domain.Metagame.Data.Catalog;
using BaroBackend.Infrastructure.Configuration;
using BaroBackend.Infrastructure.Utils;

using Microsoft.Extensions.Options;

namespace BaroBackend.Infrastructure.Catalog;

public class JsonResourceCatalog(IHeroCatalogProvider heroCatalogProvider, IOptions<CatalogOptions> catalogOptions)
    : IResourceCatalogProvider
{
    private static readonly JsonSerializerOptions ResourceDataSerializerOptions = new(JsonSerializerDefaults.Web);

    private Dictionary<string, ResourceData>? resourceDatas;
    private DictionaryResourceCatalog? snapshot;

    private readonly TaskCompletionSource<bool> readyTcs =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    public bool IsReady => resourceDatas is not null;

    public async Task Init(CancellationToken ct)
    {
        var resourcePath = Path.Combine(AppContext.BaseDirectory, catalogOptions.Value.ResourcesPath);

        await using var jsonStream = File.OpenRead(resourcePath);
        var readData = await JsonSerializer.DeserializeAsync<List<ResourceData>>(
            jsonStream, ResourceDataSerializerOptions, ct);

        if (readData is null)
        {
            throw new InvalidOperationException(
                $"Failed to load resource catalog from '{resourcePath}'.");
        }

        await heroCatalogProvider.WaitUntilReady();

        var heroCatalog = heroCatalogProvider.GetObject();
        foreach (var hero in heroCatalog.GetAll())
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

    public IResourceCatalog GetObject()
    {
        return snapshot ??= new DictionaryResourceCatalog(resourceDatas ?? []);
    }
}
