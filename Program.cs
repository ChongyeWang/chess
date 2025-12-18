using MongoDB.Driver;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var mongoConnectionString = "mongodb+srv://wangchongye125:test123456@cluster0.of7cz.mongodb.net/";
var databaseName = "chess_game";

builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = null;
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(origin => true);
    });
});

builder.Services.AddSingleton(new MongoDbService(mongoConnectionString, databaseName));
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<GameManager>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseSession();

app.MapPost("/api/register", async (HttpContext context, AuthService authService) =>
{
    var data = await context.Request.ReadFromJsonAsync<Dictionary<string, string>>();
    var username = data["username"];
    var password = data["password"];
    var email = data["email"];

    var user = await authService.Register(username, password, email);
    if (user == null)
    {
        return Results.BadRequest(new { message = "Username already exists" });
    }

    context.Session.SetString("UserId", user.Id);
    context.Session.SetString("Username", user.Username);

    return Results.Ok(new
    {
        username = user.Username,
        rating = user.Rating,
        wins = user.Wins,
        losses = user.Losses
    });
});

app.MapPost("/api/login", async (HttpContext context, AuthService authService) =>
{
    var data = await context.Request.ReadFromJsonAsync<Dictionary<string, string>>();
    var username = data["username"];
    var password = data["password"];

    var user = await authService.Login(username, password);
    if (user == null)
    {
        return Results.Unauthorized();
    }

    context.Session.SetString("UserId", user.Id);
    context.Session.SetString("Username", user.Username);

    return Results.Ok(new
    {
        username = user.Username,
        rating = user.Rating,
        wins = user.Wins,
        losses = user.Losses
    });
});

app.MapGet("/api/logout", (HttpContext context) =>
{
    context.Session.Clear();
    return Results.Ok(new { message = "Logged out" });
});

app.MapGet("/api/me", (HttpContext context) =>
{
    var userId = context.Session.GetString("UserId");
    var username = context.Session.GetString("Username");

    if (string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new { userId, username });
});

app.MapGet("/api/history", async (HttpContext context, MongoDbService mongoService) =>
{
    var userId = context.Session.GetString("UserId");
    if (string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    var games = await mongoService.GameHistories
        .Find(g => g.WhitePlayerId == userId || g.BlackPlayerId == userId)
        .SortByDescending(g => g.EndTime)
        .ToListAsync();

    return Results.Ok(games);
});

app.MapGet("/api/history/{gameId}", async (string gameId, MongoDbService mongoService) =>
{
    var game = await mongoService.GameHistories
        .Find(g => g.Id == gameId)
        .FirstOrDefaultAsync();

    if (game == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(game);
});

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChessHub>("/Chesshub");
});

app.Run();

