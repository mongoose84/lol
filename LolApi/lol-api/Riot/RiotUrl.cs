namespace LolApi.Riot
{
    public static class RiotUrl
    {
        public static string GetAccountUrl(string path, string riotApiKey)
        {
            return $"https://europe.api.riotgames.com/riot{path}?api_key={riotApiKey}";
        }

        public static string GetMatchUrl(string path, string riotApiKey)
        {
            return $"https://europe.api.riotgames.com/lol{path}?api_key={riotApiKey}";
        }

        public static string GetSummonerUrl(string region, string path, string riotApiKey)
        {
            return $"https://{region}.api.riotgames.com/lol{path}?api_key={riotApiKey}";
        }
    }
}