using System.Runtime.CompilerServices;
using RiotProxy.Infrastructure;
using RiotProxy.Utilities;

namespace RiotProxy.Application
{
    public class MetricsEndpoint : IEndpoint
    {
        public string Route { get; }
        public MetricsEndpoint(string initialPath)
        {
            Route = $"{initialPath}/metrics";
        }

        public void Configure(WebApplication app)
        {
            app.MapGet(Route, () =>
            {
                Metrics.IncrementMetrics();
                var metrics = Metrics.GetMetricsJson();
                return Results.Content(metrics, "application/json");
            });
        }

    }
}