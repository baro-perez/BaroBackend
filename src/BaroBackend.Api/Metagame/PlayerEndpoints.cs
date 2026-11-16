using System.Timers;

using BaroBackend.Api.Metagame.EndpointDtos;
using BaroBackend.Domain.Metagame;
using BaroBackend.Domain.Metagame.Data;
using BaroBackend.Infrastructure.Persistence;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Npgsql;

public static class PlayerEndpoints
{
    public const string BasePath = "/players";
    public const string GetPlayerRoute = "/{playerId:guid}";

    public const string BalanceSuffix = "/balances";
    public const string GetBalancesRoute = GetPlayerRoute + BalanceSuffix;
    public const string GetBalanceRoute = GetBalancesRoute + "/{resourceId:minlength(1)}";
    public const string DevModifySuffix = "/dev-modify";

    public const string HeroesSuffix = "/heroes";
    public const string GetHeroesRoute = GetPlayerRoute + HeroesSuffix;
    public const string GetHeroRoute = GetHeroesRoute + "/{heroId:minlength(1)}";
    public const string UnlockSuffix = "/unlock";
    public const string GetUnlockHeroRoute = GetHeroRoute + UnlockSuffix;
    public const string LevelUpSuffix = "/level-up";
    public const string GetLevelUpHeroRoute = GetHeroRoute + LevelUpSuffix;
    public const string DevUnlockOneSuffix = "/dev-unlock";
    public const string DevUnlockAllSuffix = "/dev-unlockall";

    public const string DevToolContext = "dev_tool";

    public static string ResolvePlayerRoute(Guid playerId)
    {
        return $"{BasePath}/{playerId}";
    }

    public static string ResolvePlayerBalancesRoute(Guid playerId)
    {
        return $"{ResolvePlayerRoute(playerId)}{BalanceSuffix}";
    }

    public static string ResolvePlayerBalanceRoute(Guid playerId, string resourceId)
    {
        return $"{ResolvePlayerRoute(playerId)}{BalanceSuffix}/{resourceId}";
    }

    public static string ResolvePlayerHeroesRoute(Guid playerId)
    {
        return $"{ResolvePlayerRoute(playerId)}{HeroesSuffix}";
    }

    public static string ResolvePlayerHeroRoute(Guid playerId, string heroId)
    {
        return $"{ResolvePlayerRoute(playerId)}{HeroesSuffix}/{heroId}";
    }

    public static string ResolvePlayerBalanceDevModifyRoute(Guid playerId)
    {
        return $"{ResolvePlayerRoute(playerId)}{BalanceSuffix}{DevModifySuffix}";
    }

    public static string ResolvePlayerHeroesDevUnlockAllRoute(Guid playerId)
    {
        return $"{ResolvePlayerRoute(playerId)}{HeroesSuffix}{DevUnlockAllSuffix}";
    }

    public static string ResolvePlayerHeroesDevUnlockOneRoute(Guid playerId, string heroId)
    {
        return $"{ResolvePlayerRoute(playerId)}{HeroesSuffix}/{heroId}{DevUnlockOneSuffix}";
    }

    public static void MapPlayerEndpoints(
        IEndpointRouteBuilder app,
        IWebHostEnvironment environment)
    {
        var group = app.MapGroup(BasePath).WithTags("Players");

        group.MapGet(GetPlayerRoute, GetPlayer).WithName(nameof(GetPlayerRoute));
        group.MapPost("/", CreatePlayer);

        group.MapGet(GetBalancesRoute, GetPlayerBalances).WithName(nameof(GetBalancesRoute));
        group.MapGet(GetBalanceRoute, GetPlayerBalance).WithName(nameof(GetBalanceRoute));

        group.MapGet(GetHeroesRoute, GetPlayerHeroes).WithName(nameof(GetHeroesRoute));
        group.MapGet(GetHeroRoute, GetPlayerHero).WithName(nameof(GetHeroRoute));
        group.MapPost(GetUnlockHeroRoute, UnlockHero).WithName(nameof(GetUnlockHeroRoute));
        group.MapPost(GetLevelUpHeroRoute, LevelUpHero).WithName(nameof(GetLevelUpHeroRoute));

        if (environment.IsDevelopment())
        {
            group.MapPost(GetBalancesRoute + DevModifySuffix, DevModifyResourceByDelta);
            group.MapPost(GetHeroesRoute + DevUnlockOneSuffix, DevUnlockOneHero);
            group.MapPost(GetHeroesRoute + DevUnlockAllSuffix, DevUnlockAllHeroes);
        }
    }

    private static async Task<IResult> GetPlayer(
        [FromRoute] Guid playerId,
        BaroDbContext db,
        CancellationToken ct)
    {
        var player = await db.Players.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == playerId, ct);
        return player != null ?
            Results.Ok(PlayerDto.From(player)) :
            Results.NotFound();
    }

    private static async Task<IResult> CreatePlayer(
        CreatePlayerDto request,
        BaroDbContext db,
        TimeProvider time,
        CancellationToken ct)
    {
        var now = time.GetUtcNow();
        var newPlayer = Player.Create(request.Name, now);
        db.Players.Add(newPlayer);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is 
            PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Player name already taken",
                detail: $"A player named '{request.Name}' already exists.");
        }

        return Results.CreatedAtRoute(
            nameof(GetPlayerRoute),
            new {playerId = newPlayer.Id },
            PlayerDto.From(newPlayer));
    }

    private static async Task<IResult> GetPlayerBalance(
        Guid playerId,
        string resourceId,
        BaroDbContext db,
        CancellationToken ct)
    {
        var player = await db.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == playerId, ct);

        if (player is null)
        {
            return Results.NotFound();
        }

        var balance = player.Balances
            .FirstOrDefault(h => h.ResourceId == resourceId);

        if (balance is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(ResourceAndAmountDto.From(balance));
    }

    private static async Task<IResult> GetPlayerBalances(
        Guid playerId,
        BaroDbContext db,
        CancellationToken ct)
    {
        var player = await db.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == playerId, ct);

        if (player is null)
        {
            return Results.NotFound();
        }

        var balances = player.Balances
            .Select(ResourceAndAmountDto.From)
            .ToArray();

        return Results.Ok(balances);
    }

    private static async Task<IResult> GetPlayerHero(
        Guid playerId,
        string heroId,
        BaroDbContext db,
        CancellationToken ct)
    {
        var player = await db.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == playerId, ct);

        if (player is null)
        {
            return Results.NotFound();
        }

        var hero = player.Heroes
            .FirstOrDefault(h => h.HeroId == heroId);

        if (hero is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(PlayerHeroDto.From(hero));
    }

    private static async Task<IResult> GetPlayerHeroes(
        Guid playerId,
        BaroDbContext db,
        CancellationToken ct)
    {
        var player = await db.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == playerId, ct);

        if (player is null)
        {
            return Results.NotFound();
        }

        var heroes = player.Heroes
            .Select(PlayerHeroDto.From)
            .ToArray();

        return Results.Ok(heroes);
    }

    private static async Task<IResult> DevModifyResourceByDelta(
        Guid playerId,
        ResourceAndAmountDto data,
        BaroDbContext db,
        IResourceCatalog resourceCatalog,
        TimeProvider time,
        CancellationToken ct)
    {
        var player = await db.Players
            .FindAsync([playerId], ct);

        if (player is null)
        {
            return Results.NotFound();
        }

        var now = time.GetUtcNow();
        var ledgerEntry = await player.ApplyResourceDelta(data.ResourceId,
            data.Amount, now, resourceCatalog, DevToolContext);

        db.LedgerEntries.Add(ledgerEntry);
        await db.SaveChangesAsync(ct);
        return Results.Ok(LedgerEntryDto.From(ledgerEntry));
    }

    private static async Task<IResult> UnlockHero(
        Guid playerId,
        string heroId,
        IHeroCatalog heroCatalog,
        IResourceCatalog resourceCatalog,
        TimeProvider time,
        BaroDbContext db,
        CancellationToken ct)
    {
        var player = await db.Players
            .FindAsync([playerId], ct);

        if (player is null)
        {
            return Results.NotFound();
        }

        if (player.Heroes.Any(h => h.HeroId == heroId))
        {
            return Results.Conflict($"Player already has hero '{heroId}'.");
        }

        var heroData = await heroCatalog.GetHeroById(heroId);

        if (heroData is null)
        {
            return Results.BadRequest($"Can't unlock non-existing hero '{heroId}'.");
        }

        if (await heroCatalog.HeroIdIsValid(heroId))
        {
            return Results.BadRequest($"Requesting an unlock for non-existent hero: '{heroId}'.");
        }

        var cost = heroData.GetUnlockCost();
        if (!player.CanSpend(cost))
        {
            // TODO: add all unsatisfied to response
            return Results.BadRequest($"Lacking some resource for unlocking '{heroId}'.");
        }

        // Verified, now do the purchase
        var now = time.GetUtcNow();
        var ledger = new List<LedgerEntry>();
        var levelUpHeroResult = await player.BuyUnlockHero(
            heroId, heroCatalog, resourceCatalog, now);

        db.LedgerEntries.AddRange(levelUpHeroResult.LedgerChanges);
        await db.SaveChangesAsync(ct);
        return Results.CreatedAtRoute(
            nameof(GetHeroRoute),
            new { heroId = levelUpHeroResult.PlayerHero.HeroId },
            LevelUpHeroResultDto.From(levelUpHeroResult));
    }

    private static async Task<IResult> LevelUpHero(
        Guid playerId,
        LevelUpHeroDto request,
        IHeroCatalog heroCatalog,
        IResourceCatalog resourceCatalog,
        TimeProvider time,
        BaroDbContext db,
        CancellationToken ct)
    {
        var player = await db.Players
            .FindAsync([playerId], ct);

        if (player is null)
        {
            return Results.NotFound();
        }

        if (!player.Heroes.Any(h => h.HeroId == request.HeroId))
        {
            return Results.Conflict($"Player doesn't have hero '{request.HeroId}'.");
        }

        var heroData = await heroCatalog.GetHeroById(request.HeroId);
        if (heroData is null)
        {
            return Results.BadRequest($"Can't level up non-existing hero '{request.HeroId}'.");
        }

        var currentLevel = player.GetHeroLevel(request.HeroId);

        var cost = heroData.GetLevelUpCost(currentLevel + 1);
        if (!player.CanSpend(cost))
        {
            // TODO: add all unsatisfied to response
            return Results.BadRequest($"Lacking some resource for leveling '{request.HeroId}' up to level {currentLevel + 1}.");
        }

        // Verified, now do the purchase
        var now = time.GetUtcNow();
        var levelUpHeroResult = await player.BuyLevelUpHero(
            request.HeroId, heroCatalog, resourceCatalog, now);

        db.LedgerEntries.AddRange(levelUpHeroResult.LedgerChanges);
        await db.SaveChangesAsync(ct);
        return Results.Ok(LevelUpHeroResultDto.From(levelUpHeroResult));
    }

    private static async Task<IResult> DevUnlockOneHero(
        Guid playerId,
        PlayerHeroDto request,
        BaroDbContext db,
        TimeProvider time,
        IHeroCatalog heroCatalog,
        CancellationToken ct)
    {
        var player = await db.Players
            .FindAsync([playerId], ct);

        if (player is null)
        {
            return Results.NotFound();
        }

        if (request.Level <= 0)
        {
            return Results.BadRequest($"Hero unlock level must be greater than 0");
        }

        if (player.Heroes.Any(h => h.HeroId == request.HeroId))
        {
            return Results.Conflict($"Player already had hero '{request.HeroId}'.");
        }

        if (await heroCatalog.HeroIdIsValid(request.HeroId))
        {
            return Results.BadRequest($"Requesting an unlock for non-existent hero: '{request.HeroId}'.");
        }

        var levelUpHeroResult = await player.DevUnlockHero(
            request.HeroId, request.Level, heroCatalog, time.GetUtcNow());

        db.LedgerEntries.AddRange(levelUpHeroResult.LedgerChanges);
        await db.SaveChangesAsync(ct);
        return Results.CreatedAtRoute(
            nameof(GetHeroRoute),
            new { heroId = levelUpHeroResult.PlayerHero.HeroId },
            LevelUpHeroResultDto.From(levelUpHeroResult));
    }

    private static async Task<IResult> DevUnlockAllHeroes(
        Guid playerId,
        string context,
        BaroDbContext db,
        TimeProvider time,
        IHeroCatalog heroCatalog,
        CancellationToken ct)
    {
        var player = await db.Players
            .FindAsync([playerId], ct);

        if (player is null)
        {
            return Results.NotFound();
        }

        var results = new List<LevelUpHeroResult>();
        var now = time.GetUtcNow();
        foreach(var heroData in await heroCatalog.GetAll())
        {
            if (player.Heroes.Any(h => h.HeroId == heroData.HeroId))
            {
                continue;
            }
            var hero = await player.DevUnlockHero(heroData.HeroId, 1, heroCatalog, now);
            results.Add(hero);
        }

        foreach(var result in results)
        {
            db.LedgerEntries.AddRange(result.LedgerChanges);
        }
        await db.SaveChangesAsync(ct);

        return Results.Ok(results.Select(LevelUpHeroResultDto.From).ToArray());
    }
}