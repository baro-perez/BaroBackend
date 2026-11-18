using BaroBackend.Domain.Metagame.Data;
using BaroBackend.Domain.Metagame.Data.Catalog;
using BaroBackend.Domain.Utils;

namespace BaroBackend.Infrastructure.Catalog;

public class DictionaryHeroCatalog : IHeroCatalog
{
    private readonly IReadOnlyDictionary<string, HeroData> heroDatas;

    /// <param name="source">Gets copied</param>
    public DictionaryHeroCatalog(IReadOnlyDictionary<string, HeroData> source)
    {
        this.heroDatas = source.Clone();
    }

    public IReadOnlyCollection<HeroData> GetAll()
    {
        IReadOnlyCollection<HeroData> result =
            heroDatas is null ? [] : heroDatas.Values.ToArray();
        return result;
    }

    public HeroData? GetHeroById(string heroId)
    {
        return (heroDatas?.TryGetValue(heroId, out var result) ?? false) ? result : null;
    }

    public int GetMaxLevel(string heroId)
    {
        throw new NotImplementedException();
    }

    public bool HeroIdIsValid(string heroId)
    {
        return heroDatas?.ContainsKey(heroId) ?? false;
    }
}