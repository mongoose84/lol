using RiotProxy.Domain;
using MySqlConnector;

namespace RiotProxy.Infrastructure.Persistence
{
    public class UserRepository
    {
        private readonly IDbConnectionFactory _factory;

    public UserRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

        public async Task<User?> GetByUserNameAsync(string userName)
        {
            await using var conn = _factory.CreateConnection();

            await conn.OpenAsync();

            const string sql = "SELECT UserId, UserName FROM User WHERE UserName = @userName";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userName", userName);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new User
            {
                UserId    = reader.GetInt32(0),
                UserName  = reader.GetString(1)
            };
        }
    }
}