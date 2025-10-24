var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api/getnumber", () => Results.Ok(new { number = 429 }));

app.Run();