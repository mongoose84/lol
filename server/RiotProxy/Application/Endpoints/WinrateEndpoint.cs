using RiotProxy.Infrastructure;
using RiotProxy.Infrastructure.External.Riot;
using RiotProxy.Utilities;

namespace RiotProxy.Application
{
    public class WinrateEndpoint : IEndpoint
    {       
        private readonly IRiotApiClient _riotApiClient;

        public string Route { get; }
        public WinrateEndpoint(string basePath, IRiotApiClient riotApiClient)
        {
            _riotApiClient = riotApiClient;
            Route = basePath + "/winrate/{region}/{puuid}";
        }

        public void Configure(WebApplication app)
        {
            app.MapGet(Route, async (string region, string puuid) =>
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

                    var winrate = await _riotApiClient.GetWinrateAsync(region, puuid);

                    return Results.Ok(winrate);
                }
                catch (Exception ex) when (
                    ex is not OutOfMemoryException &&
                    ex is not StackOverflowException &&
                    ex is not ThreadAbortException)
                {
                    return Results.Problem(detail: ex.Message + ex.StackTrace, statusCode: 500);
                }
            });
        }

    }
}