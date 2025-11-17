using RiotProxy.External.Domain.Entities;
using RiotProxy.Infrastructure.External.Database.Repositories;
using System.Text.Json;

namespace RiotProxy.Infrastructure.External.Riot.RateLimiter
{
    public class RiotRateLimitedJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SemaphoreSlim _jobLock = new(1, 1); // Only allow 1 execution at a time

        // Token buckets are set lower for RIOT rate limits  (20 requests/second, 100 requests/2 minutes)
        private readonly TokenBucket _perSecondBucket = new(15, TimeSpan.FromSeconds(1));
        private readonly TokenBucket _perTwoMinuteBucket = new(80, TimeSpan.FromMinutes(2));

        public RiotRateLimitedJob(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Run until the host shuts down
            while (!stoppingToken.IsCancellationRequested)
            {
                var nextRun = DateTime.UtcNow.AddDays(1);

                // Only run the code once a day.
                var delay = nextRun - DateTime.UtcNow;
                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, stoppingToken);

                await RunJobAsync(stoppingToken);
            }
        }

        public async Task RunJobAsync(CancellationToken ct = default)
        {
            // Try to acquire the lock; if already running, skip this execution
            if (!await _jobLock.WaitAsync(0, ct))
            {
                Console.WriteLine("RiotRateLimitedJob is already running. Skipping this execution.");
                return;
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var riotApiClient = scope.ServiceProvider.GetRequiredService<IRiotApiClient>();
                var gamerRepository = scope.ServiceProvider.GetRequiredService<GamerRepository>();
                var matchRepository = scope.ServiceProvider.GetRequiredService<LolMatchRepository>();
                var participantRepository = scope.ServiceProvider.GetRequiredService<LolMatchParticipantRepository>();
                

                Console.WriteLine("RiotRateLimitedJob started.");
                var gamers = await gamerRepository.GetAllGamersAsync();

                Console.WriteLine($"Found {gamers.Count} gamers.");

                // Example: fetch match history for all gamers
                var matchHistory = await GetMatchHistoryFromRiotApi(gamers, riotApiClient, ct);
                Console.WriteLine($"Fetched match history for {matchHistory.Count} matches.");

                await AddMatchHistoryToDb(matchHistory, matchRepository, ct);
                Console.WriteLine("Match history added to DB.");

                var unprocessedMatches = await matchRepository.GetUnprocessedMatchesAsync();
                Console.WriteLine($"Found {unprocessedMatches.Count} unprocessed matches.");

                await AddMatchInfoToDb(unprocessedMatches, gamers, riotApiClient, participantRepository, matchRepository, gamerRepository, ct);

                Console.WriteLine("RiotRateLimitedJob completed.");
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                // Log and handle exceptions as needed
                Console.WriteLine($"Error in RiotRateLimitedJob: {ex.Message}");
            }
            finally
            {
                _jobLock.Release();
            }
        }

        private async Task<IDictionary<Gamer, IList<LolMatch>>> GetMatchHistoryFromRiotApi(IList<Gamer> gamers, IRiotApiClient riotApiClient, CancellationToken ct)
        {
            var gamerHistories = new Dictionary<Gamer, IList<LolMatch>>();

            foreach (var gamer in gamers)
            {
                // Wait for permission from both token buckets
                await _perSecondBucket.WaitAsync(ct);
                await _perTwoMinuteBucket.WaitAsync(ct);

                var matchHistory = await riotApiClient.GetMatchHistoryAsync(gamer.Puuid);
                
                gamerHistories.Add(gamer, matchHistory);
            }

            return gamerHistories;
        }

        private async Task AddMatchHistoryToDb(IDictionary<Gamer, IList<LolMatch>> gamerHistories, LolMatchRepository matchRepository, CancellationToken ct)
        {
            foreach (var kvp in gamerHistories)
            {
                var gamer = kvp.Key;
                var matches = kvp.Value;

                foreach (var match in matches)  
                {

                    try
                    {
                        // Check before inserting - cleaner than catching exceptions
                        var affectedRows = await matchRepository.AddMatchIfNotExistsAsync(match);
                        if (affectedRows == 0)
                        {
                            break; // Stop processing further matches for this gamer
                        }
                    }
                    catch (Exception ex) when (!(ex is OperationCanceledException))
                    {
                        Console.WriteLine($"Error adding match history to DB: {ex.Message}");
                    }
                }
            }
        }

        private async Task AddMatchInfoToDb(IList<LolMatch> matches,
                        IList<Gamer> gamers,
                        IRiotApiClient riotApiClient,
                        LolMatchParticipantRepository participantRepository,
                        LolMatchRepository matchRepository,
                        GamerRepository gamerRepository,
                        CancellationToken ct)
        {
            foreach (var match in matches)
            {
                try
                {
                    // Wait for permission from both token buckets
                    await _perSecondBucket.WaitAsync(ct);
                    await _perTwoMinuteBucket.WaitAsync(ct);

                    var matchInfoJson = await riotApiClient.GetMatchInfoAsync(match.MatchId);
                    var participant = MapToParticipantEntity(matchInfoJson, match);

                    // Check before inserting
                    await participantRepository.AddParticipantIfNotExistsAsync(participant);

                    match.GameMode = GetGameMode(matchInfoJson);
                    match.InfoFetched = true;
                    await matchRepository.UpdateMatchAsync(match);

                    var gamer = gamers.FirstOrDefault(g => g.Puuid == match.Puuid);
                    if (gamer != null && (gamer.LastChecked == DateTime.MinValue || gamer.LastChecked < match.GameEndTimestamp))
                    {
                        gamer.LastChecked = match.GameEndTimestamp;
                        await gamerRepository.UpdateGamerAsync(gamer);
                    }
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    Console.WriteLine($"Error adding match info to DB: {ex.Message}");
                }
            }
            Console.WriteLine($"{matches.Count} match participants added to DB.");
        }

        private string GetGameMode(JsonDocument matchInfo)
        {
            if (matchInfo.RootElement.TryGetProperty("info", out var infoElement) &&
                infoElement.TryGetProperty("gameMode", out var gameModeElement))
            {
                if (gameModeElement.ValueKind == JsonValueKind.String)
                    return gameModeElement.GetString() ?? string.Empty;

                // fallback if API changes type unexpectedly
                return gameModeElement.ToString();
            }
            return string.Empty;
        }

        private static long? GetEpochMilliseconds(JsonElement obj, string propertyName)
        {
            if (!obj.TryGetProperty(propertyName, out var el))
                return null;

            return el.ValueKind switch
            {
                JsonValueKind.Number => el.GetInt64(),
                JsonValueKind.String => long.TryParse(el.GetString(), out var v) ? v : null,
                _ => null
            };
        }

        private LolMatchParticipant MapToParticipantEntity(JsonDocument matchInfo, LolMatch match)
        {
            if (matchInfo.RootElement.TryGetProperty("info", out var infoElement) &&
                infoElement.TryGetProperty("participants", out var participantsElement))
            {
                // gameEndTimestamp is epoch ms; fall back to gameCreation if needed
                var endMs = GetEpochMilliseconds(infoElement, "gameEndTimestamp")
                            ?? GetEpochMilliseconds(infoElement, "gameCreation")
                            ?? 0L;

                if (endMs > 0)
                    match.GameEndTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(endMs).UtcDateTime;
                else
                    match.GameEndTimestamp = DateTime.MinValue;

                foreach (var participant in participantsElement.EnumerateArray())
                {
                    if (participant.TryGetProperty("puuid", out var puuidElement) &&
                        puuidElement.GetString() == match.Puuid)
                    {
                        if (!matchInfo.RootElement.TryGetProperty("metadata", out var metadataElement) ||
                            !metadataElement.TryGetProperty("matchId", out var matchIdElement) ||
                            matchIdElement.ValueKind != JsonValueKind.String)
                            throw new InvalidOperationException("Missing or invalid 'metadata.matchId' property.");
                        if (!participant.TryGetProperty("championName", out var championNameElement) ||
                            championNameElement.ValueKind != JsonValueKind.String)
                            throw new InvalidOperationException("Missing or invalid 'championName' property in participant.");
                        if (!participant.TryGetProperty("win", out var winElement) ||
                            winElement.ValueKind != JsonValueKind.True && winElement.ValueKind != JsonValueKind.False)
                            throw new InvalidOperationException("Missing or invalid 'win' property in participant.");
                        if (!participant.TryGetProperty("role", out var roleElement) ||
                            roleElement.ValueKind != JsonValueKind.String)
                            throw new InvalidOperationException("Missing or invalid 'role' property in participant.");
                        if (!participant.TryGetProperty("kills", out var killsElement) ||
                            killsElement.ValueKind != JsonValueKind.Number)
                            throw new InvalidOperationException("Missing or invalid 'kills' property in participant.");
                        if (!participant.TryGetProperty("deaths", out var deathsElement) ||
                            deathsElement.ValueKind != JsonValueKind.Number)
                            throw new InvalidOperationException("Missing or invalid 'deaths' property in participant.");
                        if (!participant.TryGetProperty("assists", out var assistsElement) ||
                            assistsElement.ValueKind != JsonValueKind.Number)
                            throw new InvalidOperationException("Missing or invalid 'assists' property in participant.");

                        var matchId = matchIdElement.GetString() ??
                            throw new InvalidOperationException("Missing or invalid 'MatchId' property in participant.");
                        var puuidValue = puuidElement.GetString() ??
                            throw new InvalidOperationException("Missing or invalid 'Puuid' property in participant.");
                        var championName = championNameElement.GetString() ??
                            throw new InvalidOperationException("Missing or invalid 'ChampionName' property in participant.");
                        var role = roleElement.GetString() ??
                            throw new InvalidOperationException("Missing or invalid 'Role' property in participant.");

                        return new LolMatchParticipant
                        {
                            MatchId = matchId,
                            Puuid = puuidValue,
                            ChampionName = championName,
                            Win = winElement.GetBoolean(),
                            Role = role,
                            Kills = killsElement.GetInt32(),
                            Deaths = deathsElement.GetInt32(),
                            Assists = assistsElement.GetInt32(),
                        };
                    }
                }
            }

            throw new InvalidOperationException("Participant not found.");
        }
    }
}