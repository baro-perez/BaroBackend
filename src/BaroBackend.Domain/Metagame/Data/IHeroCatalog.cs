using BaroBackend.Domain.Common;

namespace BaroBackend.Domain.Metagame.Data;

public interface IHeroCatalog : IIsReadyChecked, IIsSnapshotable<IHeroCatalog>
{
    Task<IReadOnlyCollection<HeroData>> GetAll();
    Task<HeroData?> GetHeroById(string heroId);
    Task<int> GetMaxLevel(string heroId);
    Task<bool> HeroIdIsValid(string heroId);
}