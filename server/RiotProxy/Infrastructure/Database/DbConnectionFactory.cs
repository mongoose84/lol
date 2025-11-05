using MySqlConnector; 

namespace RiotProxy.Infrastructure.Database
{
    public sealed class DbConnectionFactory : IDbConnectionFactory
    {
        public MySqlConnection CreateConnection() => new MySqlConnection(Secrets.DatabaseConnectionString);
    }
}