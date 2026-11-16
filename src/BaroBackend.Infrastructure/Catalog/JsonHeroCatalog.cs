using System.Text.Json;

using BaroBackend.Domain.Common;
using BaroBackend.Domain.Metagame.Data;
using BaroBackend.Infrastructure.Configuration;

using Microsoft.Extensions.Options;

namespace BaroBackend.Infrastructure.Catalog;
public class JsonHeroCatalog : IHeroCatalog
{
    private static readonly JsonSerializerOptions HeroDataSerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly string heroesPath;

    private Dictionary<string, HeroData>? heroDatas;
    private DictionaryHeroCatalog? snapshot;

    private readonly TaskCompletionSource<bool> readyTcs =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public bool IsReady => heroDatas is not null;

    public JsonHeroCatalog(IOptions<CatalogOptions> catalogOptions)
    {
        this.heroesPath = Path.Combine(AppContext.BaseDirectory, catalogOptions.Value.HeroesPath);
    }

    public async Task Init(CancellationToken ct)
    {
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

    public IHeroCatalog GetSnapshot()
    {
        return snapshot ??= new DictionaryHeroCatalog(heroDatas ?? []);
    }

    public Task<IReadOnlyCollection<HeroData>> GetAll()
    {
        IReadOnlyCollection<HeroData> result = 
            heroDatas is null ? [] : heroDatas.Values.ToArray();
        return Task.FromResult(result);
    }

    public Task<HeroData?> GetHeroById(string heroId)
    {
        return heroDatas is null
            ? Task.FromResult<HeroData?>(null)
            : Task.FromResult(heroDatas.TryGetValue(heroId, out var result) ? result : null);
    }

    public Task<int> GetMaxLevel(string heroId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HeroIdIsValid(string heroId)
    {
        return heroDatas is null
            ? Task.FromResult(false)
            : Task.FromResult(heroDatas.ContainsKey(heroId));
    }
}
