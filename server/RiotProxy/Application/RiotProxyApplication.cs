using RiotProxy.Infrastructure;
using RiotProxy.Utilities;

namespace RiotProxy.Application
{
    public class RiotProxyApplication
    {
        private readonly WebApplication _app;
        private readonly string _apiVersion = "v1.0";
        private readonly string _initialPath;
        private readonly RiotServices _riotServices = new RiotServices();
        private readonly IList<IEndpoint> _endpoints = new List<IEndpoint>();
        public RiotProxyApplication(WebApplication app)
        {
            _app = app;
            _initialPath = "/api/" + _apiVersion;

            var homeEndPoint = new HomeEndpoint(_apiVersion, _initialPath);
            _endpoints.Add(homeEndPoint);

            var metricsEndpoint = new MetricsEndpoint(_initialPath);
            _endpoints.Add(metricsEndpoint);

            var summonerEndpoint = new SummonerEndpoint(_initialPath, _riotServices);
            _endpoints.Add(summonerEndpoint);

            var winrateEndpoint = new WinrateEndpoint(_initialPath, _riotServices);
            _endpoints.Add(winrateEndpoint);
        }

        public void Configure()
        {
            // Read the Riot API key from file
            ApiKey.Read();

            foreach (var endpoint in _endpoints)
            {
                endpoint.Configure(_app);
            }
        }


    }

}