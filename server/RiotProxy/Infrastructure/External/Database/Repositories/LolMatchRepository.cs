using MySqlConnector;
using RiotProxy.External.Domain.Entities;

namespace RiotProxy.Infrastructure.External.Database.Repositories
{
    public class LolMatchRepository
    {
        private readonly IDbConnectionFactory _factory;

        public LolMatchRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task AddMatchAsync(LolMatch match)
        {
            await using var conn = _factory.CreateConnection();
            await conn.OpenAsync();

            const string sql = "INSERT INTO LolMatch (MatchId, Puuid, InfoFetched, GameMode, GameEndTimestamp) VALUES (@matchId, @puuid, @infoFetched, @gameMode, @endTs)";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@matchId", match.MatchId);
            cmd.Parameters.AddWithValue("@puuid", match.Puuid);
            cmd.Parameters.AddWithValue("@infoFetched", match.InfoFetched);
            cmd.Parameters.AddWithValue("@gameMode", match.GameMode ?? string.Empty);
            
            // Use a sentinel date instead of NULL if column is NOT NULL
            cmd.Parameters.AddWithValue("@endTs", 
                match.GameEndTimestamp == DateTime.MinValue 
                    ? new DateTime(1970, 1, 1) // Or DateTime.UtcNow as placeholder
                    : match.GameEndTimestamp);
            
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateMatchAsync(LolMatch match)
        {
            await using var conn = _factory.CreateConnection();
            await conn.OpenAsync();

            const string sql = "UPDATE LolMatch SET InfoFetched = @infoFetched, GameMode = @gameMode, GameEndTimestamp = @endTs WHERE MatchId = @matchId";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@infoFetched", match.InfoFetched);
            cmd.Parameters.AddWithValue("@matchId", match.MatchId);
            cmd.Parameters.AddWithValue("@gameMode", match.GameMode);
            cmd.Parameters.AddWithValue("@endTs", match.GameEndTimestamp == DateTime.MinValue ? (object)DBNull.Value : match.GameEndTimestamp);
            await cmd.ExecuteNonQueryAsync();
        }

        internal async Task<IList<LolMatch>> GetUnprocessedMatchesAsync()
        {
            var matches = new List<LolMatch>();
            await using var conn = _factory.CreateConnection();
            await conn.OpenAsync();

            const string sql = "SELECT MatchId, Puuid, InfoFetched, GameMode, GameEndTimestamp FROM LolMatch WHERE InfoFetched = FALSE";
            await using var cmd = new MySqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                matches.Add(new LolMatch
                {
                    MatchId = reader.GetString(0),
                    Puuid = reader.GetString(1),
                    InfoFetched = reader.GetBoolean(2),
                    GameMode = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    GameEndTimestamp = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4)
                });
            }
            return matches;
        }

        public async Task<IList<LolMatch>> GetExistingMatchesAsync(IList<string> matchIds)
        {
            if (matchIds.Count == 0) return new List<LolMatch>();

            await using var conn = _factory.CreateConnection();
            await conn.OpenAsync();

            var ids = string.Join(",", matchIds.Select(id => $"'{MySqlHelper.EscapeString(id)}'"));
            var sql = $"SELECT MatchId, Puuid, InfoFetched, GameMode FROM `LolMatch` WHERE MatchId IN ({ids})";
            
            await using var cmd = new MySqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            
            var matches = new List<LolMatch>();
            while (await reader.ReadAsync())
            {
                matches.Add(new LolMatch
                {
                    MatchId = reader.GetString(0),
                    Puuid = reader.GetString(1),
                    InfoFetched = reader.GetBoolean(2),
                    GameMode = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                });
            }
            
            return matches;
        }

        public async Task<bool> MatchExistsAsync(string matchId, string puuid)
        {
            await using var conn = _factory.CreateConnection();
            await conn.OpenAsync();

            const string sql = "SELECT COUNT(*) FROM `LolMatch` WHERE MatchId = @matchId AND Puuid = @puuid";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@matchId", matchId);
            cmd.Parameters.AddWithValue("@puuid", puuid);

            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task AddMatchIfNotExistsAsync(LolMatch match)
        {
            if (await MatchExistsAsync(match.MatchId, match.Puuid))
                return;

            await AddMatchAsync(match);
        }
    }
}