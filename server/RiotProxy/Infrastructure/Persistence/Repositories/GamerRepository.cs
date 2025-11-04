using MySqlConnector;

namespace RiotProxy.Infrastructure.Persistence
{
    public class GamerRepository
    {
        private readonly IDbConnectionFactory _factory;

        public GamerRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<Gamer?> GetByGamerTagAsync(string gamerTag)
        {
            await using var conn = _factory.CreateConnection();

            await conn.OpenAsync();

            const string sql = "SELECT GamerId, GamerTag FROM Gamer WHERE GamerTag = @gamerTag";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@gamerTag", gamerTag);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new Gamer
            {
                GamerId = reader.GetInt32(0),
                GamerTag = reader.GetString(1)
            };
        }

        public async Task<Gamer?> CreateGamerAsync(int userId, string puuid, string gamerName, string tagLine)
        {
            await using var conn = _factory.CreateConnection();
            await conn.OpenAsync();

            const string sql = @"
                INSERT INTO Gamer (UserId, Puuid, GamerName, TagLine)          
                VALUES (@userId, @puuid, @gamerName, @tagLine);
                SELECT LAST_INSERT_ID();             
            ";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@puuid", puuid);
            cmd.Parameters.AddWithValue("@gamerTag", gamerName);
            cmd.Parameters.AddWithValue("@tagLine", tagLine);
            var result = await cmd.ExecuteScalarAsync();

            if (result == null || result == DBNull.Value)
                return null;

            var newGamer = new Gamer
            {
                GamerId = Convert.ToInt32(result),
                GamerTag = gamerName
            };

            return newGamer;
        }
    }
}