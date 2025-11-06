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

            const string sql = "INSERT INTO Match (MatchId, Puuid, InfoFetched) VALUES (@matchId, @puuid, @infoFetched)";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@matchId", match.MatchId);
            cmd.Parameters.AddWithValue("@puuid", match.Puuid);
            cmd.Parameters.AddWithValue("@infoFetched", match.InfoFetched);

            await cmd.ExecuteNonQueryAsync();
        }

        internal async Task<IList<LolMatch>> GetUnprocessedMatchesAsync()
        {
            var matches = new List<LolMatch>();

            await using var conn = _factory.CreateConnection();
            await conn.OpenAsync();

            const string sql = "SELECT MatchId, Puuid, InfoFetched FROM Match WHERE InfoFetched = FALSE";
            await using var cmd = new MySqlCommand(sql, conn);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var match = new LolMatch
                {
                    MatchId = reader.GetString("MatchId"),
                    Puuid = reader.GetString("Puuid"),
                    InfoFetched = reader.GetBoolean("InfoFetched")
                };
                matches.Add(match);
            }

            return matches;
        }
    }
}