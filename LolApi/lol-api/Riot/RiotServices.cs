using System.Text.Json;

namespace LolApi.Riot
{
    public class RiotServices
    {


        private readonly Dictionary<string, string> _regionMapping = new Dictionary<string, string>
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
            { "TR", "tr1" }
        };

        public RiotServices()
        {
        }

        public async Task<string> GetRiotIdAsync(string gameName, string tagLine)
        {
            // Encode each component so special characters are safe in the URL path.
            string encodedGameName = Uri.EscapeDataString(gameName);
            string encodedTagLine = Uri.EscapeDataString(tagLine);

            // Build the full request URI.
            var path = $"/account/v1/accounts/by-riot-id/{encodedGameName}/{encodedTagLine}";
            var url = RiotUrl.GetAccountUrl(path, RiotApiKey.Value);
            using var httpClient = new HttpClient();

            // Perform the GET request.
            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();   // Throws if the status is not 2xx.

            // Read the JSON payload.
            string json = await response.Content.ReadAsStringAsync();

            // Parse the JSON to extract the "puuid" field.
            using JsonDocument doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("puuid", out JsonElement puuidElement ))
            {
                var puuid = puuidElement.GetString();
                return puuid ?? throw new InvalidOperationException("The 'puuid' field is null.");
            }

            throw new InvalidOperationException("Response does not contain a 'puuid' field.");
        }
    }
}