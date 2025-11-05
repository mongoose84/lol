using RiotProxy.Application.Endpoints;
using RiotProxy.Infrastructure;
using RiotProxy.Infrastructure.External.Riot;
using RiotProxy.Infrastructure.Persistence;
using RiotProxy.Utilities;

namespace RiotProxy.Application
{
    public class RiotProxyApplication
    {
        private readonly WebApplication _app;
        private readonly string _apiVersion = "v1.0";
        private readonly string _basePath;
        private readonly IRiotApiClient _riotApi = new RiotApiClient();
        private readonly IList<IEndpoint> _endpoints = new List<IEndpoint>();
        public RiotProxyApplication(WebApplication app)
        {
            _app = app;
            _basePath = "/api/" + _apiVersion;

            var homeEndPoint = new HomeEndpoint(_apiVersion, _basePath);
            _endpoints.Add(homeEndPoint);

            var metricsEndpoint = new MetricsEndpoint(_basePath);
            _endpoints.Add(metricsEndpoint);

            var summonerEndpoint = new SummonerEndpoint(_basePath, _riotApi);
            _endpoints.Add(summonerEndpoint);

            var winrateEndpoint = new WinrateEndpoint(_basePath, _riotApi);
            _endpoints.Add(winrateEndpoint);

            var personEndpoint = new UserEndpoint(_basePath, _riotApi);
            _endpoints.Add(personEndpoint);
        }

        public void ConfigureEndpoints()
        {
            Console.WriteLine("Available endpoints:");
            foreach (var endpoint in _endpoints)
            {
                endpoint.Configure(_app);
                Console.WriteLine(endpoint.Route);
            }
        }
    }
}