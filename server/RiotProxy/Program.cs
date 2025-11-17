using RiotProxy.Utilities;
using RiotProxy.Infrastructure;
using RiotProxy.Application;
using RiotProxy.Infrastructure.External.Database;
using RiotProxy.Infrastructure.External.Database.Repositories;
using RiotProxy.Infrastructure.External.Riot;
using RiotProxy.Infrastructure.External.Riot.RateLimiter;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

// Read secrets needed for the program
Secrets.Initialize();

var jwtKey = Encoding.UTF8.GetBytes(Secrets.JwtKey);

builder.Services.AddSingleton<IRiotApiClient, RiotApiClient>();
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserPasswordRepository>();
builder.Services.AddScoped<GamerRepository>();
builder.Services.AddScoped<LolMatchRepository>();
builder.Services.AddScoped<LolMatchParticipantRepository>();



// Register RiotRateLimitedJob as singleton AND hosted service
builder.Services.AddSingleton<RiotRateLimitedJob>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<RiotRateLimitedJob>());

builder.Services.AddCors(options =>
{
    // Give the policy a name so you can refer to it later
    options.AddPolicy("VueClientPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173", // <-- Vue dev server
                "http://lol.agileastronaut.com",
                "https://lol.agileastronaut.com",
                "http://www.lol.agileastronaut.com",
                "https://www.lol.agileastronaut.com"
               )
              .AllowAnyHeader()                      // allow all custom headers (Content-Type, Authorization, etc.)
              .AllowAnyMethod()                      // GET, POST, PUT, DELETE, OPTIONS…
              .AllowCredentials();                   // if you need cookies / Authorization header

        // If you want to allow *any* origin (only for development!), use:
        // policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services
  .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(opts =>
  {
      opts.TokenValidationParameters = new()
      {
          ValidateIssuer = false,
          ValidateAudience = false,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
      };
  });
  
builder.Services.AddAuthorization();

var app = builder.Build();

// Apply the CORS policy globally
app.UseCors("VueClientPolicy");
app.UseAuthentication();
app.UseAuthorization(); 

// Enable routing and map endpoints
var riotProxyApplication = new RiotProxyApplication(app);
riotProxyApplication.ConfigureEndpoints();

app.Run();

