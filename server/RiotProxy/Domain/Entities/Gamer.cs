namespace RiotProxy.Domain.Entities
{
    public class Gamer
    {
        public string Puuid { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string GamerName { get; set; } = string.Empty;
        public string Tagline { get; set; } = string.Empty;
        public int Wins { get; set; }
        public int Losses { get; set; }
    }
}