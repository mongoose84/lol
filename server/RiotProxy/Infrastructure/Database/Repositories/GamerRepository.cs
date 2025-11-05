using MySqlConnector;
using RiotProxy.Domain.Entities;

namespace RiotProxy.Infrastructure.Database.Repositories
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

            return new Gamer();
        }

        public async Task<bool> CreateGamerAsync(int userId, string puuid, string gamerName, string tagLine)
        {
            Console.WriteLine($"Creating gamer {gamerName}#{tagLine} in database...");

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
            cmd.Parameters.AddWithValue("@gamerName", gamerName);
            cmd.Parameters.AddWithValue("@tagLine", tagLine);
            var result = await cmd.ExecuteScalarAsync();

            if (result == null || result == DBNull.Value)
                return false;


            return true;
        }
    }
}