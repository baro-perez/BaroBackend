using BaroBackend.Api.Metagame.EndpointDtos;
using BaroBackend.Domain.Metagame;
using BaroBackend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

public static class PlayerEndpoints
{
    private const string GetPlayerRoute = "/{id:guid}";
    private const string GetPlayerBalanceRoute = "/{id:guid}/balances";

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
            //group.MapPost(GetPlayerBalanceRoute + "/grants", GrantBalanceToPlayer);
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
        return Results.Forbid();
        /*var player = await db.Players
            .AsNoTracking()
            .Where(p => p.Id == playerId)
            .FirstOrDefaultAsync(ct);

        if (player == null)
        {
            return Results.NotFound();
        }

        var balances = await player.Balances
            .Select(ResourceBalanceResponse.From)
            .ToListAsync(ct);

        return player != null ?
            Results.Ok(balances) :
            Results.NotFound();*/
    }
}