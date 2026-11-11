using System;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();
var appVersion = "0.1";

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    var docsUi = builder.Configuration["ApiDocs:Ui"];
    
    var integrateAllUis = bool.TryParse(
        builder.Configuration["ApiDocs:IntegrateAllUis"], out var integrateAllUisParsed) &&
        integrateAllUisParsed;

    if (integrateAllUis)
    {
        IntegrateSwagger(app, appVersion);
        app.MapScalarApiReference();
    }

    switch (docsUi?.ToLowerInvariant())
    {
        default:
        case "swagger":
            {
                if (!integrateAllUis)
                {
                    IntegrateSwagger(app, appVersion);
                }
                app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
            }
            break;
        case "scalar":
            {
                if (!integrateAllUis)
                {
                    app.MapScalarApiReference();
                }
                app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
            }
            break;
    }
}

app.UseHttpsRedirection();

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
        version = appVersion
    }));

app.Run();

static void IntegrateSwagger(WebApplication app, string version)
{
    app.UseSwaggerUI(o => o.SwaggerEndpoint(
        "/openapi/v1.json",
        $"BaroBackend {version}"));
}