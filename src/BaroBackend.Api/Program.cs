using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar/v1"))
       .ExcludeFromDescription();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/ping", (TimeProvider time) =>
    {
        var utc = time.GetUtcNow();
        var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
        return Results.Ok(new {
            status = "ok",
            utcNow = utc.DateTime,
            localTime = utc.ToOffset(localTimeZone.BaseUtcOffset).DateTime,
        });
    });

app.MapGet("/version", (TimeProvider time) =>
    Results.Ok(new {
        name = "BaroBackend",
        version = "0.1"
    }));

app.Run();