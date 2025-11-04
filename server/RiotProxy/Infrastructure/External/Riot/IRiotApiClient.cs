namespace RiotProxy.Infrastructure.External.Riot
{
    public interface IRiotApiClient
    {
        Task<string> GetSummonerAsync(string gameName, string tagLine);
        Task<double> GetWinrateAsync(string region, string puuid);

        Task<string> GetPuuidAsync(string region, string summonerId);
    }
}