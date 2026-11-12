using BaroBackend.Api.Metagame.EndpointDtos;
using BaroBackend.Domain.Metagame;
using BaroBackend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

public static class PlayerEndpoints
{
    private const string GetPlayerRoute = "/{id:guid}";

    public static void MapPlayerEndpoints(
        IEndpointRouteBuilder app,
        IWebHostEnvironment environment)
    {
        var group = app.MapGroup("/players").WithTags("Players");
        group.MapPost("/", CreatePlayer);
        group.MapPost(GetPlayerRoute, GetPlayer);

        if (environment.IsDevelopment())
        {

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
            Results.Ok(PlayerResponse.FromPlayer(player)) :
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
            PlayerResponse.FromPlayer(newPlayer));
    }
}