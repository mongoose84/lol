using System.Text.Json;
using RiotProxy.External.Domain.Entities;

namespace RiotProxy.Infrastructure.External.Riot
{
    public interface IRiotApiClient
    {
        Task<string> GetSummonerAsync(string gameName, string tagLine, CancellationToken ct = default);
        Task<double> GetWinrateAsync(string puuid);
        Task<string> GetPuuidAsync(string region, string summonerId, CancellationToken ct = default);
        Task<IList<LolMatch>> GetMatchHistoryAsync(string puuid, CancellationToken ct = default);
        Task<JsonDocument> GetMatchInfoAsync(string matchId, CancellationToken ct = default);
    }
}