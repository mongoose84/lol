using MySqlConnector; 

namespace RiotProxy.Infrastructure
{
    public sealed class DbConnectionFactory : IDbConnectionFactory
    {
        public MySqlConnection CreateConnection() => new MySqlConnection(Secrets.DatabaseConnectionString);
    }
}