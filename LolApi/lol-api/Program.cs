using LolApi.Riot;
using Microsoft.Extensions.Options;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<RiotOptions>>().Value);

var app = builder.Build();



// Expose the root provider for the static wrapper.
// Do NOT use this for regular DI; it's only for the static façade.
CuntyMcCuntface.ServiceProvider = app.Services;

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