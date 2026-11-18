using System.Timers;

using BaroBackend.Api.Metagame.EndpointDtos;
using BaroBackend.Domain.Metagame;
using BaroBackend.Domain.Metagame.Data;
using BaroBackend.Infrastructure.Catalog;
using BaroBackend.Infrastructure.Persistence;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Npgsql;

public static class PlayerEndpoints
{
    public const string BasePath = "/players";
    public const string PlayerIdParameter = "{playerId:guid}";
    public const string GetPlayerRoute = "/" + PlayerIdParameter;

    public const string BalanceSuffix = "/balances";
    public const string GetBalancesRoute = GetPlayerRoute + BalanceSuffix;
    public const string ResourceIdParameter = "{resourceId:minlength(1)}";
    public const string GetBalanceRoute = GetBalancesRoute + "/" + ResourceIdParameter;
    public const string DevModifySuffix = "/dev-modify";

    public const string HeroesSuffix = "/heroes";
    public const string GetHeroesRoute = GetPlayerRoute + HeroesSuffix;
    public const string HeroIdParameter = "{heroId:minlength(1)}";
    public const string GetHeroRoute = GetHeroesRoute + "/" + HeroIdParameter;
    public const string UnlockSuffix = "/unlock";
    public const string GetUnlockHeroRoute = GetHeroRoute + UnlockSuffix;
    public const string LevelUpSuffix = "/level-up";
    public const string GetLevelUpHeroRoute = GetHeroRoute + LevelUpSuffix;
    public const string DevUnlockOneSuffix = "/dev-unlock";
    public const string DevUnlockAllSuffix = "/dev-unlockall";

    public const string DevToolContext = "dev_tool";

    #region ResolveXRoute

    public static string ResolvePlayerRoute(Guid playerId)
    {
        return ResolvePlayerRoute(playerId.ToString());
    }
    public static string ResolvePlayerRoute(string playerId)
    {
        return $"{BasePath}/{playerId}";
    }
    public static string ResolvePlayerBalancesRoute(Guid playerId)
    {
        return ResolvePlayerBalancesRoute(playerId.ToString());
    }

    public static string ResolvePlayerBalancesRoute(string playerId)
    {
        return $"{ResolvePlayerRoute(playerId)}{BalanceSuffix}";
    }

    public static string ResolvePlayerBalanceRoute(Guid playerId, string resourceId)
    {
        return ResolvePlayerBalanceRoute(playerId.ToString(), resourceId);
    }

    public static string ResolvePlayerBalanceRoute(string playerId, string resourceId)
    {
        return $"{ResolvePlayerBalancesRoute(playerId)}/{resourceId}";
    }

    public static string ResolvePlayerHeroesRoute(Guid playerId)
    {
        return ResolvePlayerHeroesRoute(playerId.ToString());
    }
    public static string ResolvePlayerHeroesRoute(string playerId)
    {
        return $"{ResolvePlayerRoute(playerId)}{HeroesSuffix}";
    }


    public static string ResolvePlayerHeroRoute(Guid playerId, string heroId)
    {
        return ResolvePlayerHeroRoute(playerId.ToString(), heroId);
    }
    public static string ResolvePlayerHeroRoute(string playerId, string heroId)
    {
        return $"{ResolvePlayerHeroesRoute(playerId)}/{heroId}";
    }

    public static string ResolvePlayerBuyUnlockHeroRoute(Guid playerId, string heroId)
    {
        return ResolvePlayerBuyUnlockHeroRoute(playerId.ToString(), heroId);
    }
    public static string ResolvePlayerBuyUnlockHeroRoute(string playerId, string heroId)
    {
        return $"{ResolvePlayerHeroRoute(playerId, heroId)}{UnlockSuffix}";
    }

    public static string ResolvePlayerBuyLevelUpHeroRoute(Guid playerId, string heroId)
    {
        return ResolvePlayerBuyLevelUpHeroRoute(playerId.ToString(), heroId);
    }
    public static string ResolvePlayerBuyLevelUpHeroRoute(string playerId, string heroId)
    {
        return $"{ResolvePlayerHeroRoute(playerId, heroId)}{LevelUpSuffix}";
    }

    public static string ResolvePlayerBalanceDevModifyRoute(Guid playerId)
    {
        return ResolvePlayerBalanceDevModifyRoute(playerId.ToString());
    }
    public static string ResolvePlayerBalanceDevModifyRoute(string playerId)
    {
        return $"{ResolvePlayerRoute(playerId)}{BalanceSuffix}{DevModifySuffix}";
    }

    public static string ResolvePlayerHeroesDevUnlockAllRoute(Guid playerId)
    {
        return ResolvePlayerHeroesDevUnlockAllRoute(playerId.ToString());
    }
    public static string ResolvePlayerHeroesDevUnlockAllRoute(string playerId)
    {
        return $"{ResolvePlayerHeroesRoute(playerId)}{DevUnlockAllSuffix}";
    }

    public static string ResolvePlayerHeroesDevUnlockOneRoute(Guid playerId)
    {
        return ResolvePlayerHeroesDevUnlockOneRoute(playerId.ToString());
    }
    public static string ResolvePlayerHeroesDevUnlockOneRoute(string playerId)
    {
        return $"{ResolvePlayerHeroesRoute(playerId)}{DevUnlockOneSuffix}";
    }

    #endregion

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
        [FromServices] BaroDbContext db,
        CancellationToken ct)
    {
        var player = await db.Players.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == playerId, ct);
        return player != null ?
            Results.Ok(PlayerDto.From(player)) :
            Results.NotFound($"Player {playerId} doesn't exist");
    }

    private static async Task<IResult> CreatePlayer(
        [FromBody] CreatePlayerDto request,
        [FromServices] BaroDbContext db,
        [FromServices] TimeProvider time,
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
        [FromRoute] Guid playerId,
        [FromRoute] string resourceId,
        [FromServices] BaroDbContext db,
        CancellationToken ct)
    {
        var player = await db.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == playerId, ct);

        if (player is null)
        {
            return Results.NotFound($"Player {playerId} doesn't exist");
        }

        var balance = player.Balances
            .FirstOrDefault(h => h.ResourceId == resourceId);

        if (balance is null)
        {
            return Results.NotFound($"Player {playerId}'s balance doesn't exist");
        }

        return Results.Ok(ResourceAndAmountDto.From(balance));
    }

    private static async Task<IResult> GetPlayerBalances(
        [FromRoute] Guid playerId,
        [FromServices]BaroDbContext db,
        CancellationToken ct)
    {
        var player = await db.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == playerId, ct);

        if (player is null)
        {
            return Results.NotFound($"Player {playerId} doesn't exist");
        }

        var balances = player.Balances
            .Select(ResourceAndAmountDto.From)
            .ToArray();

        return Results.Ok(balances);
    }

    private static async Task<IResult> GetPlayerHero(
        [FromRoute] Guid playerId,
        [FromRoute] string heroId,
        [FromServices] BaroDbContext db,
        CancellationToken ct)
    {
        var player = await db.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == playerId, ct);

        if (player is null)
        {
            return Results.NotFound($"Player {playerId} doesn't exist");
        }

        var hero = player.Heroes
            .FirstOrDefault(h => h.HeroId == heroId);

        if (hero is null)
        {
            return Results.NotFound($"Hero {heroId} in player {playerId} doesn't exist");
        }

        return Results.Ok(PlayerHeroDto.From(hero));
    }

    private static async Task<IResult> GetPlayerHeroes(
        [FromRoute] Guid playerId,
        [FromServices] BaroDbContext db,
        CancellationToken ct)
    {
        var player = await db.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == playerId, ct);

        if (player is null)
        {
            return Results.NotFound($"Player {playerId} doesn't exist");
        }

        var heroes = player.Heroes
            .Select(PlayerHeroDto.From)
            .ToArray();

        return Results.Ok(heroes);
    }

    private static async Task<IResult> DevModifyResourceByDelta(
        [FromRoute] Guid playerId,
        [FromBody] ResourceAndAmountDto data,
        [FromServices] BaroDbContext db,
        [FromServices] IResourceCatalogProvider resourceCatalogProvider,
        [FromServices] TimeProvider time,
        CancellationToken ct)
    {
        var player = await db.Players
            .FindAsync([playerId], ct);

        if (player is null)
        {
            return Results.NotFound($"Player {playerId} doesn't exist");
        }

        var now = time.GetUtcNow();
        var resourceCatalog = resourceCatalogProvider.GetObject();
        var ledgerEntry = player.ApplyResourceDelta(data.ResourceId,
            data.Amount, now, resourceCatalog, DevToolContext);

        db.LedgerEntries.Add(ledgerEntry);
        await db.SaveChangesAsync(ct);
        return Results.Ok(LedgerEntryDto.From(ledgerEntry));
    }

    private static async Task<IResult> UnlockHero(
        [FromRoute] Guid playerId,
        [FromRoute] string heroId,
        [FromServices] IHeroCatalogProvider heroCatalogProvider,
        [FromServices] IResourceCatalogProvider resourceCatalogProvider,
        [FromServices] TimeProvider time,
        [FromServices] BaroDbContext db,
        CancellationToken ct)
    {
        var player = await db.Players
            .FindAsync([playerId], ct);

        if (player is null)
        {
            return Results.NotFound($"Player {playerId} doesn't exist");
        }

        if (player.Heroes.Any(h => h.HeroId == heroId))
        {
            return Results.Conflict($"Player already has hero '{heroId}'.");
        }

        var heroCatalog = heroCatalogProvider.GetObject();

        if (!heroCatalog.HeroIdIsValid(heroId))
        {
            return Results.BadRequest($"Requesting an unlock for non-existent hero: '{heroId}'.");
        }

        var heroData = heroCatalog.GetHeroById(heroId);
        if (heroData is null)
        {
            return Results.InternalServerError($"Can't unlock non-existing hero '{heroId}'.");
        }

        var cost = heroData.GetUnlockCost();
        if (!player.CanSpend(cost))
        {
            // TODO: add all unsatisfied to response
            return Results.UnprocessableEntity($"Lacking some resource for unlocking '{heroId}'.");
        }

        // Verified, now do the purchase
        var now = time.GetUtcNow();
        var ledger = new List<LedgerEntry>();
        var resourceCatalog = resourceCatalogProvider.GetObject();
        var levelUpHeroResult = player.BuyUnlockHero(
            heroId, heroCatalog, resourceCatalog, now);

        db.LedgerEntries.AddRange(levelUpHeroResult.LedgerChanges);
        await db.SaveChangesAsync(ct);
        return Results.CreatedAtRoute(
            nameof(GetHeroRoute),
            new { heroId = levelUpHeroResult.PlayerHero.HeroId },
            LevelUpHeroResultDto.From(levelUpHeroResult));
    }

    private static async Task<IResult> LevelUpHero(
        [FromRoute] Guid playerId,
        [FromRoute] string heroId,
        [FromServices] IHeroCatalogProvider heroCatalogProvider,
        [FromServices] IResourceCatalogProvider resourceCatalogProvider,
        [FromServices] TimeProvider time,
        [FromServices] BaroDbContext db,
        CancellationToken ct)
    {
        var player = await db.Players
            .FindAsync([playerId], ct);

        if (player is null)
        {
            return Results.NotFound($"Player {playerId} doesn't exist");
        }

        if (!player.Heroes.Any(h => h.HeroId == heroId))
        {
            return Results.NotFound($"Player doesn't have hero '{heroId}'.");
        }

        var heroCatalog = heroCatalogProvider.GetObject();
        var heroData = heroCatalog.GetHeroById(heroId);
        if (heroData is null)
        {
            return Results.InternalServerError($"Can't level up non-existing hero '{heroId}'.");
        }

        var currentLevel = player.GetHeroLevel(heroId);

        var cost = heroData.GetLevelUpCost(currentLevel);
        if (!player.CanSpend(cost))
        {
            // TODO: add all unsatisfied to response
            return Results.UnprocessableEntity($"Lacking some resource for leveling '{heroId}' up to level {currentLevel + 1}.");
        }

        // Verified, now do the purchase
        var now = time.GetUtcNow();
        var resourceCatalog = resourceCatalogProvider.GetObject();
        var levelUpHeroResult = player.BuyLevelUpHero(
            heroId, heroCatalog, resourceCatalog, now);

        db.LedgerEntries.AddRange(levelUpHeroResult.LedgerChanges);
        await db.SaveChangesAsync(ct);
        return Results.Ok(LevelUpHeroResultDto.From(levelUpHeroResult));
    }

    private static async Task<IResult> DevUnlockOneHero(
        [FromRoute] Guid playerId,
        [FromBody] PlayerHeroDto request,
        [FromServices] BaroDbContext db,
        [FromServices] TimeProvider time,
        [FromServices] IHeroCatalogProvider heroCatalogProvider,
        CancellationToken ct)
    {
        var player = await db.Players
            .FindAsync([playerId], ct);

        if (player is null)
        {
            return Results.NotFound($"Player {playerId} doesn't exist");
        }

        if (request.Level <= 0)
        {
            return Results.BadRequest($"Hero unlock level must be greater than 0");
        }

        if (player.Heroes.Any(h => h.HeroId == request.HeroId))
        {
            return Results.Conflict($"Player already had hero '{request.HeroId}'.");
        }

        var heroCatalog = heroCatalogProvider.GetObject();
        if (!heroCatalog.HeroIdIsValid(request.HeroId))
        {
            return Results.BadRequest($"Requesting an unlock for non-existent hero: '{request.HeroId}'.");
        }

        var levelUpHeroResult = player.DevUnlockHero(
            request.HeroId, request.Level, heroCatalog, time.GetUtcNow());

        db.LedgerEntries.AddRange(levelUpHeroResult.LedgerChanges);
        await db.SaveChangesAsync(ct);
        return Results.CreatedAtRoute(
            nameof(GetHeroRoute),
            new { heroId = levelUpHeroResult.PlayerHero.HeroId },
            LevelUpHeroResultDto.From(levelUpHeroResult));
    }

    private static async Task<IResult> DevUnlockAllHeroes(
        [FromRoute] Guid playerId,
        [FromServices] BaroDbContext db,
        [FromServices] TimeProvider time,
        [FromServices] IHeroCatalogProvider heroCatalogProvider,
        CancellationToken ct)
    {
        var player = await db.Players
            .FindAsync([playerId], ct);

        if (player is null)
        {
            return Results.NotFound($"Player {playerId} doesn't exist");
        }

        var results = new List<LevelUpHeroResult>();
        var now = time.GetUtcNow();
        var heroCatalog = heroCatalogProvider.GetObject();
        foreach (var heroData in heroCatalog.GetAll())
        {
            if (player.Heroes.Any(h => h.HeroId == heroData.HeroId))
            {
                continue;
            }
            var hero = player.DevUnlockHero(heroData.HeroId, 1, heroCatalog, now);
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