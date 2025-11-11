using MySqlConnector;
using System.Security.Cryptography;

namespace RiotProxy.Infrastructure.External.Database.Repositories
{
    public class UserPasswordRepository
    {
        private readonly IDbConnectionFactory _factory;
        public UserPasswordRepository(IDbConnectionFactory factory) => _factory = factory;

        public async Task StoreAsync(int userId, string password)
        {
            // Generate salt
            var salt = RandomNumberGenerator.GetBytes(16);
            // Derive hash (PBKDF2)
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 64);

            await using var conn = _factory.CreateConnection();
            await conn.OpenAsync();
            const string sql = @"INSERT INTO UserPassword (UserId, PasswordHash, PasswordSalt) VALUES (@uid, @hash, @salt)
                                 ON DUPLICATE KEY UPDATE PasswordHash=@hash, PasswordSalt=@salt;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@hash", hash);
            cmd.Parameters.AddWithValue("@salt", salt);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> VerifyAsync(int userId, string password)
        {
            await using var conn = _factory.CreateConnection();
            await conn.OpenAsync();
            const string sql = "SELECT PasswordHash, PasswordSalt FROM UserPassword WHERE UserId=@uid";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return false;

            var storedHash = (byte[])reader["PasswordHash"];
            var storedSalt = (byte[])reader["PasswordSalt"];
            var testHash = Rfc2898DeriveBytes.Pbkdf2(password, storedSalt, 100_000, HashAlgorithmName.SHA256, 64);
            return CryptographicOperations.FixedTimeEquals(storedHash, testHash);
        }
    }
}