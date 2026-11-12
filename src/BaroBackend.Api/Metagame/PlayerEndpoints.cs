using System.Timers;

using BaroBackend.Api.Metagame.EndpointDtos;
using BaroBackend.Domain.Metagame;
using BaroBackend.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using static System.Runtime.InteropServices.JavaScript.JSType;

public static class PlayerEndpoints
{
    private const string GetPlayerRoute = "/{id:guid}";
    private const string GetPlayerBalanceRoute = "/{id:guid}/balances";
    private const string DevToolContext = "dev_tool";

    public static void MapPlayerEndpoints(
        IEndpointRouteBuilder app,
        IWebHostEnvironment environment)
    {
        var group = app.MapGroup("/players").WithTags("Players");

        group.MapGet(GetPlayerRoute, GetPlayer);
        group.MapPost("/", CreatePlayer);

        group.MapGet(GetPlayerBalanceRoute, GetPlayerBalance);
        if (environment.IsDevelopment())
        {
            group.MapPost(GetPlayerBalanceRoute + "/grants",
                (Guid playerId,
                ResourceAndAmount data,
                BaroDbContext db,
                TimeProvider time,
                CancellationToken ct) =>
                ModifyResourceByDelta(playerId, data, DevToolContext,
                    db, time, ct));
        }
    }

    private static async Task<IResult> GetPlayer(
        Guid playerId,
        BaroDbContext db,
        CancellationToken ct)
    {
        var player = await db.Players.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == playerId, ct);
        return player != null ?
            Results.Ok(PlayerResponse.From(player)) :
            Results.NotFound();
    }

    private static async Task<IResult> CreatePlayer(
        CreatePlayerRequest request,
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
            GetPlayerRoute,
            new {id = newPlayer.Id },
            PlayerResponse.From(newPlayer));
    }

    private static async Task<IResult> GetPlayerBalance(
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
            .Select(ResourceBalanceResponse.From)
            .ToArray();

        return Results.Ok(balances);
    }

    private static async Task<IResult> ModifyResourceByDelta(
        Guid playerId,
        ResourceAndAmount data,
        string context,
        BaroDbContext db,
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
        var ledgerEntry = player.ApplyResourceDelta(data.ResourceId,
            data.Amount, now, context);

        db.LedgerEntries.Add(ledgerEntry);
        await db.SaveChangesAsync(ct);
        return Results.Ok(LedgerEntryResponse.From(ledgerEntry));
    }
}