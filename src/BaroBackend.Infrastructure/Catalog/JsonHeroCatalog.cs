using System.Text.Json;

using BaroBackend.Domain.Metagame.Data;
using BaroBackend.Domain.Metagame.Data.Catalog;
using BaroBackend.Infrastructure.Configuration;

using Microsoft.Extensions.Options;

namespace BaroBackend.Infrastructure.Catalog;

public class JsonHeroCatalog(IOptions<CatalogOptions> catalogOptions) : IHeroCatalogProvider
{
    private static readonly JsonSerializerOptions HeroDataSerializerOptions = new(JsonSerializerDefaults.Web);

    private Dictionary<string, HeroData>? heroDatas;
    private DictionaryHeroCatalog? snapshot;

    private readonly TaskCompletionSource<bool> readyTcs =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public bool IsReady => heroDatas is not null;

    public async Task Init(CancellationToken ct)
    {
        var heroesPath = Path.Combine(AppContext.BaseDirectory, catalogOptions.Value.HeroesPath);
        await using var jsonStream = File.OpenRead(heroesPath);
        var readData = await JsonSerializer.DeserializeAsync<List<HeroData>>(
            jsonStream, HeroDataSerializerOptions, ct);

        if (readData is null)
        {
            throw new InvalidOperationException(
                $"Failed to load hero catalog from '{heroesPath}'.");
        }

        heroDatas = readData.ToDictionary(h => h.HeroId, h => h);
        readyTcs.TrySetResult(true);
    }

    public Task WaitUntilReady()
    {
        return IsReady ? Task.CompletedTask : readyTcs.Task;
    }

    public IHeroCatalog GetObject()
    {
        return snapshot ??= new DictionaryHeroCatalog(heroDatas ?? []);
    }
}
