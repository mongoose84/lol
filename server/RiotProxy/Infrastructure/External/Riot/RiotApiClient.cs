using System.Text.Json;
using System.Web;
using RiotProxy.External.Domain.Entities;
using RiotProxy.Utilities;

namespace RiotProxy.Infrastructure.External.Riot
{
    public class RiotApiClient : IRiotApiClient
    {
        // Token buckets are set lower for RIOT rate limits  (20 requests/second, 100 requests/2 minutes)
        private readonly RiotTokenBucket _perSecondBucket = new(15, TimeSpan.FromSeconds(1));
        private readonly RiotTokenBucket _perTwoMinuteBucket = new(80, TimeSpan.FromMinutes(2));

        public async Task<string> GetSummonerAsync(string gameName, string tagLine, CancellationToken ct = default)
        {
           // Encode each component so special characters are safe in the URL path.
            var encodedGameName = Uri.EscapeDataString(gameName);
            var encodedTagLine = Uri.EscapeDataString(tagLine);

            var puuid = await GetPuuidAsync(encodedGameName, encodedTagLine);
            string encodedPuuid = HttpUtility.UrlEncode(puuid);
            var summonerUrl = RiotUrlBuilder.GetSummonerUrl(encodedTagLine, $"/summoner/v4/summoners/by-puuid/{encodedPuuid}");
            Metrics.SetLastUrlCalled("RiotServices.cs ln 19" + summonerUrl);
            
            // Perform the GET request.
            using var httpClient = new HttpClient();
            
            await _perSecondBucket.WaitAsync(ct);
            await _perTwoMinuteBucket.WaitAsync(ct);

            var response = await httpClient.GetAsync(summonerUrl);
            response.EnsureSuccessStatusCode();   // Throws if the status is not 2xx.

            // Read the JSON payload.
            var json = await response.Content.ReadAsStringAsync();
            return json;
        }


        public async Task<double> GetWinrateAsync(string puuid)
        {
            var wins = 0;

            var matchHistory = await GetMatchHistoryAsync(puuid);
            var totalGames = matchHistory.Count;
            
            foreach (var match in matchHistory)
            {
                var matchDetails = await GetMatchInfoAsync(match.MatchId);
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

        public async Task<string> GetPuuidAsync(string gameName, string tagLine, CancellationToken ct = default)
        {
            // Build the full request URI.
            var path = $"/account/v1/accounts/by-riot-id/{gameName}/{tagLine}";
            var url = RiotUrlBuilder.GetAccountUrl(path);
            using var httpClient = new HttpClient();
            Metrics.SetLastUrlCalled("RiotServices.cs ln 67" + url);

            await _perSecondBucket.WaitAsync(ct);
            await _perTwoMinuteBucket.WaitAsync(ct);

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

        public async Task<IList<LolMatch>> GetMatchHistoryAsync(string puuid, CancellationToken ct = default)
        {
            string encodedPuuid = HttpUtility.UrlEncode(puuid);
            var matchUrl = RiotUrlBuilder.GetMatchUrl($"/match/v5/matches/by-puuid/{encodedPuuid}/ids") + "&start=0&count=100";
            Metrics.SetLastUrlCalled("RiotServices.cs ln 92" + matchUrl);

            using var httpClient = new HttpClient();

            await _perSecondBucket.WaitAsync(ct);
            await _perTwoMinuteBucket.WaitAsync(ct);

            var response = await httpClient.GetAsync(matchUrl);
            response.EnsureSuccessStatusCode();   // Throws if the status is not 2xx.

            // Read and deserialize the JSON array of strings
            var json = await response.Content.ReadAsStringAsync();
            var matchesAsJson = JsonSerializer.Deserialize<List<string>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<string>();

            // Use Select to map matchId strings to LolMatch objects
            var matches = matchesAsJson
                .Select(matchId => new LolMatch
                {
                    MatchId = matchId,
                    Puuid = puuid
                })
                .ToList();

            return matches;
        }

        public async Task<JsonDocument> GetMatchInfoAsync(string matchId, CancellationToken ct = default)
        {
            var matchUrl = RiotUrlBuilder.GetMatchUrl($"/match/v5/matches/{matchId}");
            using var httpClient = new HttpClient();
            Metrics.SetLastUrlCalled("RiotServices.cs ln 110" + matchUrl);

            await _perSecondBucket.WaitAsync(ct);
            await _perTwoMinuteBucket.WaitAsync(ct);

            var response = await httpClient.GetAsync(matchUrl);
            response.EnsureSuccessStatusCode();   // Throws if the status is not 2xx
            var json = await response.Content.ReadAsStringAsync();
            var matchDoc = JsonDocument.Parse(json);
            return matchDoc;
        }
    }
}