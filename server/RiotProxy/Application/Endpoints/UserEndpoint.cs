using Microsoft.AspNetCore.Mvc;
using RiotProxy.Application.DTOs;
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
            ConfigurePost(app);
            ConfigureGet(app);
        }
        
        private void ConfigureGet(WebApplication app)
        {
            app.MapGet(Route, async (string userName, [FromBody] CreateUserRequest body, [FromServices] UserRepository repo) =>
            {
                try
                {
                    var user = await repo.GetByUserNameAsync(userName);
                    if (user is null)
                    {
                        return Results.NotFound("User not found");
                    }

                    Console.WriteLine($"Created user: {user.UserName} with ID: {user.UserId} ");

                    return Results.Content(user.ToJson(), "application/json");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    return Results.BadRequest("Error when getting user");
                }
            });
        }

        private void ConfigurePost(WebApplication app)
        {
            app.MapPost(Route, async (
                string userName,
                [FromBody] CreateUserRequest body,
                [FromServices] UserRepository userRepo
                ) =>
            {
                ValidateBody(body);

                try
                {

                    var user = await userRepo.CreateUserAsync(userName);
                    if (user is null)
                    {
                        return Results.NotFound("User not found");
                    }

                    Console.WriteLine($"Created user: {user.UserName} with ID: {user.UserId} ");


                    return Results.Content(user.ToJson(), "application/json");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    return Results.BadRequest("Invalid operation when getting user");
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    return Results.BadRequest("Invalid argument when getting user");
                }
                catch (Exception ex) when (
                    !(ex is OutOfMemoryException) &&
                    !(ex is StackOverflowException) &&
                    !(ex is ThreadAbortException)
                )
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    return Results.BadRequest("Error when getting user");
                }
            });
        }

        private void ValidateBody(CreateUserRequest body)
        {
            if (body == null)
            {
                throw new ArgumentException("Request body is null");
            }

            if (body.Accounts == null || body.Accounts.Count == 0)
            {
                throw new ArgumentException("Accounts list is null or empty");
            }

            foreach (var account in body.Accounts)
            {
                if (string.IsNullOrWhiteSpace(account.GameName))
                {
                    throw new ArgumentException("GameName is null or empty");
                }

                if (string.IsNullOrWhiteSpace(account.TagLine))
                {
                    throw new ArgumentException("TagLine is null or empty");
                }
            }
        }
    }
}