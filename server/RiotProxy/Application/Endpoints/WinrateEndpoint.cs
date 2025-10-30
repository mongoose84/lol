using System.Runtime.CompilerServices;
using RiotProxy.Infrastructure;
using RiotProxy.Utilities;

namespace RiotProxy.Application
{
    public class WinrateEndpoint : IEndpoint
    {       
        private RiotServices _riotServices;

        public string Route { get; }
        public WinrateEndpoint(string initialPath, RiotServices riotServices)
        {
            _riotServices = riotServices;
            Route = initialPath + "/winrate/{region}/{puuid}";
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

                    var winrate = await _riotServices.GetWinrateAsync(region, puuid);

                    return Results.Ok(winrate);
                }
                catch (Exception ex)
                {
                    return Results.Problem(detail: ex.Message + ex.StackTrace, statusCode: 500);
                }
            });
        }

    }
}