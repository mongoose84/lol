using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RiotProxy.Infrastructure.External.Database.Repositories;
using RiotProxy.Infrastructure;

namespace RiotProxy.Application.Endpoints
{
    public record LoginRequest(string UserName, string Password);

    public class AuthEndpoint : IEndpoint
    {
        public string Route => "/api/v1.0/auth/login";
        public void Configure(WebApplication app)
        {
            app.MapPost(Route, async (
                [FromBody] LoginRequest body,
                UserRepository userRepo,
                UserPasswordRepository pwdRepo) =>
            {
                if (body is null || string.IsNullOrWhiteSpace(body.UserName) || string.IsNullOrWhiteSpace(body.Password))
                    return Results.BadRequest("Missing credentials");

                var user = await userRepo.GetByUserNameAsync(body.UserName);
                if (user is null) return Results.Unauthorized();

                var ok = await pwdRepo.VerifyAsync(user.UserId, body.Password);
                if (!ok) return Results.Unauthorized();

                var keyBytes = Encoding.UTF8.GetBytes(Secrets.JwtKey);
                var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                    new Claim("name", user.UserName)
                };

                var token = new JwtSecurityToken(
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddHours(8),
                    signingCredentials: creds);

                var jwt = new JwtSecurityTokenHandler().WriteToken(token);
                return Results.Ok(new { token = jwt });
            });
        }
    }
}