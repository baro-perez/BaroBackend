using BaroBackend.Domain.Common;

namespace BaroBackend.Domain.Metagame.Data.Catalog;

public interface IHeroCatalog
{
    IReadOnlyCollection<HeroData> GetAll();
    HeroData? GetHeroById(string heroId);
    int GetMaxLevel(string heroId);
    bool HeroIdIsValid(string heroId);
}