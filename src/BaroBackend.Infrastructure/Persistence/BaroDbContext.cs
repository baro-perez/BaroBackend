using BaroBackend.Domain.Metagame;
using Microsoft.EntityFrameworkCore;


namespace BaroBackend.Infrastructure.Persistence;
public sealed class BaroDbContext(DbContextOptions<BaroDbContext> options) : DbContext(options)
{

    public DbSet<Player> Players => Set<Player>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaroDbContext).Assembly);
        modelBuilder.HasPostgresExtension(PostgresConstants.CaseInsensitiveText);
    }
}