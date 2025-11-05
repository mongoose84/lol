using System.Text.Json;

namespace RiotProxy.Infrastructure.External.Riot
{
    public interface IRiotApiClient
    {
        Task<string> GetSummonerAsync(string gameName, string tagLine);
        Task<double> GetWinrateAsync(string puuid);
        Task<string> GetPuuidAsync(string region, string summonerId);
        Task<IList<string>> GetMatchHistoryAsync(string puuid);
        Task<JsonDocument> GetMatchAsync(string matchId);
    }
}