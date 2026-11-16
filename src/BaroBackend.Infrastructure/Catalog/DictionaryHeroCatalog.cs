using BaroBackend.Domain.Metagame.Data;
using BaroBackend.Domain.Utils;

namespace BaroBackend.Infrastructure.Catalog;

public class DictionaryHeroCatalog : IHeroCatalog
{
    private readonly Dictionary<string, HeroData> heroDatas;
    private DictionaryHeroCatalog? snapshot;
    public bool IsReady => true;

    public DictionaryHeroCatalog(IReadOnlyDictionary<string, HeroData> source)
    {
        this.heroDatas = source.Clone();
    }

    public Task WaitUntilReady()
    {
        return Task.CompletedTask;
    }

    public IHeroCatalog GetSnapshot()
    {
        return snapshot ??= new DictionaryHeroCatalog(heroDatas);
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