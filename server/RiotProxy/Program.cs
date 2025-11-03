using RiotProxy.Utilities;
using RiotProxy.Infrastructure;
using RiotProxy.Application;
using RiotProxy.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Read secrets needed for the program
Secrets.Initialize();

builder.Services.AddCors(options =>
{
    // Give the policy a name so you can refer to it later
    options.AddPolicy("VueClientPolicy", policy =>
    {
        // 👉 Replace the origin(s) with the exact URL(s) where your Vue app runs.
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

// register EF Core DbContext and repository
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(Secrets.DatabaseConnectionString);
});

builder.Services.AddScoped<PersonRepository>();

var app = builder.Build();

// Apply the CORS policy globally
app.UseCors("VueClientPolicy");

var riotProxyApplication = new RiotProxyApplication(app);

riotProxyApplication.Configure();




app.Run();

