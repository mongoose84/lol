using System.Net.Http.Metrics;
using LolApi.Riot;
using LolApi.Metrics;


var builder = WebApplication.CreateBuilder(args);

RiotApiKey.Read();

builder.Services.AddCors(options =>
{
    // Give the policy a name so you can refer to it later
    options.AddPolicy("VueClientPolicy", policy =>
    {
        // 👉 Replace the origin(s) with the exact URL(s) where your Vue app runs.
        policy.WithOrigins(
                "http://localhost:5173", // <-- Vue dev server
                "http://lol.agileastronaut.com",
                "https://lol.agileastronaut.com",
                "http://www.lol.agileastronaut.com",
                "https://www.lol.agileastronaut.com"
               )   
              .AllowAnyHeader()                      // allow all custom headers (Content-Type, Authorization, etc.)
              .AllowAnyMethod()                      // GET, POST, PUT, DELETE, OPTIONS…
              .AllowCredentials();                   // if you need cookies / Authorization header
              
        // If you want to allow *any* origin (only for development!), use:
        // policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();
var initialPath = $"/api/{Metrics.ApiVersion}";

// 2️⃣ Apply the CORS policy globally
app.UseCors("VueClientPolicy");

var riotServices = new RiotServices();


app.MapGet($"{initialPath}/", () =>
{
    Metrics.IncrementHome();
    return Results.Ok(new { success = "yes" });
});



app.MapGet($"{initialPath}/metrics", () =>
{
    Metrics.IncrementMetrics();
    var metrics = Metrics.GetMetricsJson();
    return Results.Content(metrics, "application/json");
});

app.MapGet(initialPath + "/summoner/{gameName}/{tagLine}", async (string gameName, string tagLine) =>
{
    Metrics.IncrementSummoner();

    try
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(gameName) || string.IsNullOrWhiteSpace(tagLine))
        {
            return Results.BadRequest(new { error = "Both gameName and tagLine must be provided." });
        }

        var summoner = await riotServices.GetSummonerAsync(gameName, tagLine);

        return Results.Content(summoner, "application/json");
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
});

app.MapGet(initialPath + "/winrate/{region}/{puuid}", async (string region, string puuid) =>
{
    Metrics.IncrementWinrate();

    try
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(region))
        {
            return Results.BadRequest(new { error = "Region is null or whitespace" });
        }

        if (string.IsNullOrWhiteSpace(puuid))
        {
            return Results.BadRequest(new { error = "Puuid is null or whitespace" });
        }

        var winrate = await riotServices.GetWinrateAsync(region, puuid);

        return Results.Ok(winrate);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
});

app.Run();

