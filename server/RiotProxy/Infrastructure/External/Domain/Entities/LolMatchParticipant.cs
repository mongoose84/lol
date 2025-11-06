namespace RiotProxy.External.Domain.Entities
{
    public class LolMatchParticipant
    {
        public string MatchId { get; set; } = string.Empty;
        public string Puuid { get; set; } = string.Empty;
        public bool Win { get; set; }
        public string Role { get; set; } = string.Empty;
        public string ChampionName { get; set; } = string.Empty;
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
    }
}