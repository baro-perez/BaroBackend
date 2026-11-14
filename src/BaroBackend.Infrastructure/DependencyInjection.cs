using BaroBackend.Domain.Metagame.Data;
using BaroBackend.Infrastructure.Catalog;
using BaroBackend.Infrastructure.Configuration;
using BaroBackend.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BaroBackend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddConfigurationOptions(services);

        ConfigureDatabase(services, configuration);
        ConfigureCatalog(services, configuration);

        return services;
    }

    private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var catalogOptions = configuration
            .GetSection(ConnectionStringsOptions.SectionName)
            .Get<ConnectionStringsOptions>() ?? new ConnectionStringsOptions();

        services.AddDbContext<BaroDbContext>(o => o
            .UseNpgsql(catalogOptions.PostGres)
            .UseSnakeCaseNamingConvention());
    }

    private static void ConfigureCatalog(IServiceCollection services, IConfiguration configuration)
    {
        var catalogOptions = configuration
            .GetSection(CatalogOptions.SectionName)
            .Get<CatalogOptions>() ?? new CatalogOptions();

        switch (catalogOptions.Source)
        {
            case CatalogOptions.SourceType.LocalJson:
                services.AddSingleton<JsonHeroCatalog>();
                services.AddSingleton<IHeroCatalog>(s => s.GetRequiredService<JsonHeroCatalog>());
                services.AddSingleton<IStartupTask>(s => 
                    new StartupTask(ct => s.GetRequiredService<JsonHeroCatalog>().Init(ct)));
                break;
            default:
                throw new InvalidOperationException(
                    $"Unknown catalog source '{catalogOptions.Source}'.");
        }
    }

    private static void AddConfigurationOptions(IServiceCollection services)
    {
        services.AddOptions<ConnectionStringsOptions>()
            .BindConfiguration(ConnectionStringsOptions.SectionName)
            .Validate(o => !string.IsNullOrWhiteSpace(o.PostGres),
                $"{ConnectionStringsOptions.SectionName}:{nameof(ConnectionStringsOptions.PostGres)}")
            .ValidateOnStart();

        services.AddOptions<CatalogOptions>()
            .BindConfiguration(CatalogOptions.SectionName)
            .Validate(o => !string.IsNullOrWhiteSpace(o.HeroesPath),
                $"{CatalogOptions.SectionName}:{nameof(CatalogOptions.HeroesPath)}")
            .ValidateOnStart();
    }
}