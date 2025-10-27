var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { success = "yes" }));

app.MapGet("/api/getnumber", () => Results.Ok(new { number = 429 }));



app.Run();