using RiotProxy.Infrastructure;

namespace LolApi.Riot
{
    public static class RiotUrl
    {
        private static readonly Dictionary<string, string> _regionMapping = new Dictionary<string, string>
        {
            { "NA", "na1" },
            { "EUW", "euw1" },
            { "EUNE", "eun1" },
            { "KR", "kr" },
            { "JP", "jp1" },
            { "BR", "br1" },
            { "LAN", "la1" },
            { "LAS", "la2" },
            { "OCE", "oc1" },
            { "RU", "ru" },
            { "TR", "tr1" },
            { "9765", "eun1" } // spicial case for Doend
        };

        public static string GetAccountUrl(string path)
        {
            return $"https://europe.api.riotgames.com/riot{path}?api_key={ApiKey.Value}";
        }

        public static string GetMatchUrl(string path)
        {
            return $"https://europe.api.riotgames.com/lol{path}?api_key={ApiKey.Value}";
        }

        public static string GetSummonerUrl(string region, string path)
        {
            var regionUpper = region.ToUpper();
            var regionCode = _regionMapping.ContainsKey(regionUpper) ? _regionMapping[regionUpper] : throw new ArgumentException($"Invalid region code: {region}");

            return $"https://{regionCode}.api.riotgames.com/lol{path}?api_key={ApiKey.Value}";
        }
    }
}