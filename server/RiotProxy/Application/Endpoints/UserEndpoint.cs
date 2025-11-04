using Microsoft.AspNetCore.Mvc;
using RiotProxy.Domain;
using RiotProxy.Infrastructure.Persistence;

namespace RiotProxy.Application
{
    public class UserEndpoint : IEndpoint
    {
        public string Route { get; }

        public UserEndpoint(string basePath)
        {
            Route = basePath + "/user/{userName}";
        }

        public void Configure(WebApplication app)
        {

            app.MapGet(Route, async (string userName, [FromServices] UserRepository repo) =>
            {
                try
                {
                    var user = await repo.CreateUserAsync(userName);
                    if (user is null)
                    {
                        return Results.NotFound("User not found");
                    }
                    return Results.Content(user.ToJson(), "application/json");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    return Results.BadRequest("Error when getting user");
                }

                
            });
        }
    }
}