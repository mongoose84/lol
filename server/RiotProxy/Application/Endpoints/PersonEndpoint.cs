using RiotProxy.Domain;
using RiotProxy.Infrastructure.Persistence;

namespace RiotProxy.Application
{
    public class PersonEndpoint : IEndpoint
    {
        
        public string Route { get; }

        public PersonEndpoint(string basePath)
        {
            Route = basePath + "/user/{userName}";
        }

        public void Configure(WebApplication app)
        {

            app.MapGet(Route, async (string userName, PersonRepository repo) =>
            {
                try
                {
                    var person = new Person();
                    person.UserId = 2;
                    person.UserName = userName;
                    await repo.AddAsync(person);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    return Results.BadRequest("Error when adding person");
                }

                return Results.Ok("Username: " + userName);
            });
        }
    }
}