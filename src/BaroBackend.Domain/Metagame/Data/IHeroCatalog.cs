namespace BaroBackend.Domain.Metagame.Data;

public interface IHeroCatalog
{
    Task<IReadOnlyCollection<HeroData>> GetAll();
    Task<HeroData?> GetHeroById(string heroId);
    Task<int> GetMaxLevel(string heroId);
}