using RiotProxy.Infrastructure;
using RiotProxy.Infrastructure.External.Riot;
using RiotProxy.Utilities;

namespace RiotProxy.Application.Endpoints
{
    public class SummonerEndpoint : IEndpoint
    {
        private readonly IRiotApiClient _riotApiClient;

        public string Route { get; }

        public SummonerEndpoint(string basePath, IRiotApiClient riotApiClient)
        {
            _riotApiClient = riotApiClient;
            Route = basePath + "/summoner/{gameName}/{tagLine}";
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

                    var summoner = await _riotApiClient.GetSummonerAsync(gameName, tagLine);

                    return Results.Content(summoner, "application/json");
                }
                catch (ArgumentException argEx)
                {
                    return Results.BadRequest(new { error = argEx.Message });
                }
                catch (System.Net.Http.HttpRequestException httpEx)
                {
                    return Results.Problem(detail: httpEx.Message, statusCode: 502);
                }
                catch (Exception ex) when (!(ex is OutOfMemoryException) && !(ex is StackOverflowException) && !(ex is ThreadAbortException))
                {
                    // Log ex here if logging is available
                    return Results.Problem(detail: "An unexpected error occurred.", statusCode: 500);
                }
            });
        }

    }
}