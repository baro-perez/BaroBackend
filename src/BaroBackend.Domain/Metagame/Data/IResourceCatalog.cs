using BaroBackend.Domain.Common;

namespace BaroBackend.Domain.Metagame.Data;

public interface IResourceCatalog : IIsReadyChecked, IIsSnapshotable<IResourceCatalog>
{
    Task<IReadOnlyCollection<ResourceData>> GetAll();
    Task<ResourceData?> GetResourceById(string heroId);
    Task<bool> ResourceIdIsValid(string resourceId);
}