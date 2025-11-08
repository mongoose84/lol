using MySqlConnector;
using RiotProxy.External.Domain.Entities;

namespace RiotProxy.Infrastructure.External.Database.Repositories
{
    public class GamerRepository
    {
        private readonly IDbConnectionFactory _factory;

        public GamerRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }
        public async Task<IList<Gamer>> GetAllGamersAsync()
        {
            var gamers = new List<Gamer>();
            await using var conn = _factory.CreateConnection();
            await conn.OpenAsync();

            const string sql = "SELECT Puuid, UserId, GamerName, TagLine, Wins, Losses, LastChecked FROM Gamer";
            await using var cmd = new MySqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                gamers.Add(new Gamer
                {
                    Puuid = reader.GetString(0),
                    UserId = reader.GetInt32(1),
                    GamerName = reader.GetString(2),
                    Tagline = reader.GetString(3),
                    Wins = reader.GetInt32(4),
                    Losses = reader.GetInt32(5),
                    LastChecked = reader.IsDBNull(6) ? DateTime.MinValue : reader.GetDateTime(6)
                });
            }
            return gamers;
        }

        public async Task<bool> CreateGamerAsync(int userId, string puuid, string gamerName, string tagLine)
        {
            Console.WriteLine($"Creating gamer {gamerName}#{tagLine} in database...");

            await using var conn = _factory.CreateConnection();
            await conn.OpenAsync();

            const string sql = @"
                INSERT INTO Gamer (UserId, Puuid, GamerName, TagLine, LastChecked)          
                VALUES (@userId, @puuid, @gamerName, @tagLine, @lastChecked);
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

        public async Task<bool> UpdateGamerAsync(Gamer gamer)
        {
            await using var conn = _factory.CreateConnection();
            await conn.OpenAsync();

            const string sql = @"UPDATE Gamer 
                                 SET Wins = @wins, Losses = @losses, LastChecked = @lastChecked
                                 WHERE Puuid = @puuid";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@wins", gamer.Wins);
            cmd.Parameters.AddWithValue("@losses", gamer.Losses);
            cmd.Parameters.AddWithValue("@lastChecked", gamer.LastChecked);
            cmd.Parameters.AddWithValue("@puuid", gamer.Puuid);
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
    }
}