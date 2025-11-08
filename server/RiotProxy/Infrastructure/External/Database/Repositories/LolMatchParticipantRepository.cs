using MySqlConnector;
using RiotProxy.External.Domain.Entities;

namespace RiotProxy.Infrastructure.External.Database.Repositories
{
    public class LolMatchParticipantRepository
    {
        private readonly IDbConnectionFactory _factory;

        public LolMatchParticipantRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task AddParticipantAsync(LolMatchParticipant participant)
        {
            await using var conn = _factory.CreateConnection();
            await conn.OpenAsync();

            const string sql = "INSERT INTO LolMatchParticipant (MatchId, Puuid, Win, Role, ChampionName, Kills, Deaths, Assists) " +
                               "VALUES (@matchId, @puuid, @win, @role, @championName, @kills, @deaths, @assists)";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@matchId", participant.MatchId);
            cmd.Parameters.AddWithValue("@puuid", participant.Puuid);
            cmd.Parameters.AddWithValue("@win", participant.Win);
            cmd.Parameters.AddWithValue("@role", participant.Role ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@championName", participant.ChampionName);
            cmd.Parameters.AddWithValue("@kills", participant.Kills);
            cmd.Parameters.AddWithValue("@deaths", participant.Deaths);
            cmd.Parameters.AddWithValue("@assists", participant.Assists);

            await cmd.ExecuteNonQueryAsync();
        } 

        public async Task<bool> ParticipantExistsAsync(string matchId, string puuid)
        {
            await using var conn = _factory.CreateConnection();
            await conn.OpenAsync();

            const string sql = "SELECT COUNT(*) FROM LolMatchParticipant WHERE MatchId = @matchId AND Puuid = @puuid";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@matchId", matchId);
            cmd.Parameters.AddWithValue("@puuid", puuid);

            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task AddParticipantIfNotExistsAsync(LolMatchParticipant participant)
        {
            if (await ParticipantExistsAsync(participant.MatchId, participant.Puuid))
                return;

            await AddParticipantAsync(participant);
        }  
    }
}