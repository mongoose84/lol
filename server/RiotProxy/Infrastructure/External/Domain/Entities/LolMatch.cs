namespace RiotProxy.External.Domain.Entities
{
    public class LolMatch
    {
        public string MatchId { get; set; } = string.Empty;
        public string Puuid { get; set; } = string.Empty;
        public bool InfoFetched { get; set; } = false;
    }
}