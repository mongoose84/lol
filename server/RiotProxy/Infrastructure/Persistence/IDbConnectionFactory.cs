using MySqlConnector;

namespace RiotProxy.Infrastructure
{
    public interface IDbConnectionFactory
    {
        MySqlConnection CreateConnection();
    }
}