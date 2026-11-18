using BaroBackend.Domain.Common;

namespace BaroBackend.Domain.Metagame.Data.Catalog;

public interface IResourceCatalog
{
    IReadOnlyCollection<ResourceData> GetAll();
    ResourceData? GetResourceById(string heroId);
    bool ResourceIdIsValid(string resourceId);
}