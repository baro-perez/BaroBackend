using BaroBackend.Domain.Metagame.Data.Catalog;
using BaroBackend.Infrastructure.Utils;

namespace BaroBackend.Infrastructure.Catalog;

public interface IHeroCatalogProvider : IProvider<IHeroCatalog>, IIsReadyChecked
{
}
