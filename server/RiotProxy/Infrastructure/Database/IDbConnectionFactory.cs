using MySqlConnector;

namespace RiotProxy.Infrastructure.Database
{
    public interface IDbConnectionFactory
    {
        MySqlConnection CreateConnection();
    }
}