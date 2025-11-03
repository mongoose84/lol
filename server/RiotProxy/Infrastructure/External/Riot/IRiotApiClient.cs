using System.Threading.Tasks;

namespace RiotProxy.Infrastructure
{
    public interface IRiotApiClient
    {
        Task<string> GetSummonerAsync(string gameName, string tagLine);
        Task<double> GetWinrateAsync(string region, string puuid);
    }
}