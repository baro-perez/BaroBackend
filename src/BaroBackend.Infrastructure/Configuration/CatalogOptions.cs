namespace BaroBackend.Infrastructure.Configuration;

public sealed class CatalogOptions
{
    public static class SourceType
    {
        public const string LocalJson = "json-local";
    }

    public const string SectionName = "Catalog";

    public string Source { get; init; } = SourceType.LocalJson;
    public string HeroesPath { get; init; } = "localdata/herocatalog.json";
}
