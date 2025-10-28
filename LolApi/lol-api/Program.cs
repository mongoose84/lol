using LolApi.Riot;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("Reading Riot API Key from file...");
RiotApiKey.Read();


var app = builder.Build();


var riotServices = new RiotServices();

app.MapGet("/", () => Results.Ok(new { success = "yes" }));

app.MapGet("/api/getnumber", () => Results.Ok(new { number = 429 }));

app.MapGet("/api/by-riot-id/{gameName}/{tagLine}", async (string gameName, string tagLine) =>
{
    try
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(gameName) || string.IsNullOrWhiteSpace(tagLine))
        {
            return Results.BadRequest(new { error = "Both gameName and tagLine must be provided." });
        }

        var puuidValue = await riotServices.GetRiotIdAsync(gameName, tagLine);
        
        return Results.Ok(new { puuid = puuidValue });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    } 
});

app.Run();