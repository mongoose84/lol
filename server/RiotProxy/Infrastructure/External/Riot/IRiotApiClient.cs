using System.Text.Json;
using RiotProxy.External.Domain.Entities;

namespace RiotProxy.Infrastructure.External.Riot
{
    public interface IRiotApiClient
    {
        Task<string> GetSummonerAsync(string gameName, string tagLine);
        Task<double> GetWinrateAsync(string puuid);
        Task<string> GetPuuidAsync(string region, string summonerId);
        Task<IList<LolMatch>> GetMatchHistoryAsync(string puuid);
        Task<JsonDocument> GetMatchInfoAsync(string matchId);
    }
}