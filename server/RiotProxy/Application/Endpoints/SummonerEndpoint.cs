using RiotProxy.Infrastructure;
using RiotProxy.Utilities;

namespace RiotProxy.Application
{
    public class SummonerEndpoint : IEndpoint
    {
        private RiotServices _riotServices;

        public string Route { get; }

        public SummonerEndpoint(string initialPath, RiotServices riotServices)
        {
            _riotServices = riotServices;
            Route = initialPath + "/summoner/{gameName}/{tagLine}";
        }

        public void Configure(WebApplication app)
        {
            app.MapGet(Route, async (string gameName, string tagLine) =>
            {
                Metrics.IncrementSummoner();

                try
                {
                    // Basic validation
                    if (string.IsNullOrWhiteSpace(gameName) || string.IsNullOrWhiteSpace(tagLine))
                    {
                        return Results.BadRequest(new { error = "Both gameName and tagLine must be provided." });
                    }

                    var summoner = await _riotServices.GetSummonerAsync(gameName, tagLine);

                    return Results.Content(summoner, "application/json");
                }
                catch (Exception ex)
                {
                    return Results.Problem(detail: ex.Message + ex.StackTrace, statusCode: 500);
                }
            });
        }

    }
}