using LolApi.Riot;

var builder = WebApplication.CreateBuilder(args);

RiotApiKey.Read();

builder.Services.AddCors(options =>
{
    // Give the policy a name so you can refer to it later
    options.AddPolicy("VueClientPolicy", policy =>
    {
        // 👉 Replace the origin(s) with the exact URL(s) where your Vue app runs.
        // You can list several origins with .WithOrigins("http://localhost:8080", "http://127.0.0.1:5173")
        policy.WithOrigins("http://localhost:5173")   // <-- Vue dev server
              .AllowAnyHeader()                      // allow all custom headers (Content-Type, Authorization, etc.)
              .AllowAnyMethod()                      // GET, POST, PUT, DELETE, OPTIONS…
              .AllowCredentials();                   // if you need cookies / Authorization header
              
        // If you want to allow *any* origin (only for development!), use:
        // policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// 2️⃣ Apply the CORS policy globally
app.UseCors("VueClientPolicy");

var riotServices = new RiotServices();

app.MapGet("/", () => Results.Ok(new { success = "yes" }));

app.MapGet("/api/getnumber", () => Results.Ok(new { number = 429 }));

app.MapGet("/api/summoner/{gameName}/{tagLine}", async (string gameName, string tagLine) =>
{
    try
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(gameName) || string.IsNullOrWhiteSpace(tagLine))
        {
            return Results.BadRequest(new { error = "Both gameName and tagLine must be provided." });
        }

        var puuidValue = await riotServices.GetSummonerAsync(gameName, tagLine);

        return Results.Ok(puuidValue);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
});

app.MapGet("/api/winrate/{region}/{puuid}", async (string region, string puuid) =>
{
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

        Console.WriteLine($"Calculating winrate for: {region}, puuid: {puuid}");
        var winrate = await riotServices.GetWinrateAsync(region, puuid);

        return Results.Ok(winrate);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
});

foreach (var ep in app.Services.GetRequiredService<EndpointDataSource>().Endpoints)
{
    Console.WriteLine(ep.DisplayName);
}

app.Run();