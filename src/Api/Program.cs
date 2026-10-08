var app = WebApplication.CreateBuilder(args).Build();
app.MapGet("/", () => "Hello from minimal-api");
app.MapGet("/health/ready", () => Results.Ok("ready"));
app.Run();
