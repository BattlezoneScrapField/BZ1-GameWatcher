using BZAPI.Configuration;
using BZAPI.Steam;
using BZAPI.Storage;
using BZAPI.Websocket;
using Microsoft.AspNetCore.HttpOverrides;

const string CorsPolicyName = "AllowGameWatcherClients";

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BattlezoneOptions>(builder.Configuration.GetSection(BattlezoneOptions.SectionName));
builder.Services.Configure<SteamOptions>(builder.Configuration.GetSection(SteamOptions.SectionName));

// The API sits behind nginx on the same origin in production, so this list is normally only used
// by local development. Configure it under "Cors:AllowedOrigins" rather than hard-coding it.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            return;
        }

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// nginx terminates TLS and forwards the client address; without this the app sees the proxy's
// address and scheme. KnownProxies/KnownNetworks are cleared because the proxy's container IP is
// not fixed — safe here only because the API is not published outside the compose network.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ILobbyStore, LobbyStore>();
builder.Services.AddSingleton<ISteamAvatarProvider, SteamAvatarProvider>();
builder.Services.AddHostedService<BZ98LobbyWatcher>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}
else
{
    app.UseExceptionHandler();
}

app.UseRouting();

app.UseCors(CorsPolicyName);

app.MapControllers();

// Lets the container (and anyone debugging a stale lobby list) see whether the watcher is
// actually receiving updates.
app.MapGet("/api/health", (ILobbyStore store) =>
{
    var snapshot = store.Current;

    return Results.Ok(new
    {
        status = "ok",
        lobbyCount = snapshot.Lobbies.Count,
        lastUpdatedUtc = snapshot.LastUpdatedUtc
    });
});

app.Run();
