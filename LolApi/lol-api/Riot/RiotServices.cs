using System.Text.Json;
using System.Web;

namespace LolApi.Riot
{
    public class RiotServices
    {

        public async Task<string> GetSummonerAsync(string gameName, string tagLine)
        {
           // Encode each component so special characters are safe in the URL path.
            var encodedGameName = Uri.EscapeDataString(gameName);
            var encodedTagLine = Uri.EscapeDataString(tagLine);

            var puuid = await GetRiotIdAsync(encodedGameName, encodedTagLine);
            string encodedPuuid = HttpUtility.UrlEncode(puuid);
            var summonerUrl = RiotUrl.GetSummonerUrl(encodedTagLine, $"/summoner/v4/summoners/by-puuid/{encodedPuuid}");

            // Perform the GET request.
            using var httpClient = new HttpClient();
            
            var response = await httpClient.GetAsync(summonerUrl);
            response.EnsureSuccessStatusCode();   // Throws if the status is not 2xx.

            // Read the JSON payload.
            var json = await response.Content.ReadAsStringAsync();
            return json;
        }


        public async Task<double> GetWinrateAsync(string region, string puuid)
        {
            var wins = 0;

            var matchHistory = await GetMatchHistoryAsync(region, puuid);
            var totalGames = matchHistory.Count;
            
            foreach (var matchId in matchHistory)
            {
                var matchDetails = await GetMatchAsync(matchId);
                var info = matchDetails.RootElement.GetProperty("info");
                var participants = info.GetProperty("participants").EnumerateArray();
                foreach (var participant in participants)
                {
                    if (participant.GetProperty("puuid").GetString() == puuid)
                    {
                        if (participant.GetProperty("win").GetBoolean())
                        {
                            wins++;
                        }
                        break;
                    }
                }  
            }
            double winrate = (double)wins / totalGames * 100;
            return winrate; // Placeholder value
        }
        
        private async Task<string> GetRiotIdAsync(string gameName, string tagLine)
        {
            // Build the full request URI.
            var path = $"/account/v1/accounts/by-riot-id/{gameName}/{tagLine}";
            var url = RiotUrl.GetAccountUrl(path);
            using var httpClient = new HttpClient();

            // Perform the GET request.
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();   // Throws if the status is not 2xx.

            // Read the JSON payload.
            string json = await response.Content.ReadAsStringAsync();
            
            // Parse the JSON to extract the "puuid" field.
            using JsonDocument doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("puuid", out JsonElement puuidElement))
            {
                var puuid = puuidElement.GetString();
                return puuid ?? throw new InvalidOperationException("The 'puuid' field is null.");
            }

            throw new InvalidOperationException("Response does not contain a 'puuid' field.");
        }

        private async Task<IList<string>> GetMatchHistoryAsync(string region, string puuid)
        {
            var matches = new List<string>();
            string encodedPuuid = HttpUtility.UrlEncode(puuid);
            var matchUrl = RiotUrl.GetMatchUrl($"/match/v5/matches/by-puuid/{encodedPuuid}/ids") + "&start=0&count=10";

            using var httpClient = new HttpClient();
            Console.WriteLine($"Fetching match history from URL: {matchUrl}");
            var response = await httpClient.GetAsync(matchUrl);
            response.EnsureSuccessStatusCode();   // Throws if the status is not 2xx.

            // Read and deserialize the JSON array of strings
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Match history JSON response: {json}");
            matches = JsonSerializer.Deserialize<List<string>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<string>();

            return matches;
        }

        private async Task<JsonDocument> GetMatchAsync(string matchId)
        {
            var matchUrl = RiotUrl.GetMatchUrl($"/match/v5/matches/{matchId}");
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(matchUrl);
            response.EnsureSuccessStatusCode();   // Throws if the status is not 2xx
            var json = await response.Content.ReadAsStringAsync();
            var matchDoc = JsonDocument.Parse(json);
            return matchDoc;
        }
        

    }
}