namespace RiotProxy.Infrastructure.External.Database.Repositories
{
    public class MatchRepository
    {
        private readonly IDbConnectionFactory _factory;

        public MatchRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }
    }
}