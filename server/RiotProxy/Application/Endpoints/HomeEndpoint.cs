using System.Runtime.CompilerServices;
using RiotProxy.Utilities;

namespace RiotProxy.Application
{
    public class HomeEndpoint : IEndpoint
    {
        private readonly string _apiVersion;
        private readonly string _initialPath;
        public string Route { get; } = "/";
        public HomeEndpoint(string apiVersion, string initialPath)
        {
            _apiVersion = apiVersion;
            _initialPath = initialPath;
        }

        public void Configure(WebApplication app)
        {
            app.MapGet(Route, () =>
            {
                Metrics.IncrementHome();

                var sitemap = $@"{{  ""Description"": ""Welcome to the League of Legends API. Below are the available endpoints."",  
                                    ""ApiVersion"": ""{_apiVersion}"",
                                    ""{_initialPath}/Metrics"": ""Metrics available for this API."", 
                                    ""{_initialPath}/Summoner"": ""Retrieve summoner information by game name and tag line."",
                                    ""{_initialPath}/Winrate"": ""Retrieve summoner winrate by region and puuid""
                                }}";

                return Results.Content(sitemap, "application/json");
            });
        }

    }
}