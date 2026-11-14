namespace BaroBackend.Infrastructure.Configuration;

public sealed class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";
    public string PostGres { get; init; } = null!;
}