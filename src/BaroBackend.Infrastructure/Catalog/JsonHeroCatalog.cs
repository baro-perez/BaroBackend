using System.Text.Json;

using BaroBackend.Domain.Metagame.Data;
using BaroBackend.Infrastructure.Configuration;

using Microsoft.Extensions.Options;

namespace BaroBackend.Infrastructure.Catalog;

public class JsonHeroCatalog : IHeroCatalog, IReadyChecked
{
    private static readonly JsonSerializerOptions HeroDataSerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly string heroesPath;
    private List<HeroData>? heroData;

    public bool IsReady => heroData is not null;

    public JsonHeroCatalog(IOptions<CatalogOptions> catalogOptions)
    {
        this.heroesPath = Path.Combine(AppContext.BaseDirectory, catalogOptions.Value.HeroesPath);
    }

    public async Task Init(CancellationToken ct)
    {
        await using var jsonStream = File.OpenRead(heroesPath);
        heroData = await JsonSerializer.DeserializeAsync<List<HeroData>>(
            jsonStream, HeroDataSerializerOptions, ct);

        if (heroData is null)
        {
            throw new InvalidOperationException(
                $"Failed to load hero catalog from '{heroesPath}'.");
        }
    }

    public Task<IReadOnlyCollection<HeroData>> GetAll()
    {
        throw new NotImplementedException();
    }

    public Task<HeroData?> GetHeroById(string heroId)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetMaxLevel(string heroId)
    {
        throw new NotImplementedException();
    }
}