using RiotProxy.Infrastructure;

namespace RiotProxy.Application
{
    public class RiotProxyApplication
    {
        private readonly WebApplication _app;
        private readonly string _apiVersion = "v1.0";

        public RiotProxyApplication(WebApplication app)
        {
            _app = app;
        }

        public void Configure()
        {
            // Read the Riot API key from file
            ApiKey.Read();
        }
    }

}