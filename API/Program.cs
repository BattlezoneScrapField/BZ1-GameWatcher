using BZAPI.Websocket;

var builder = WebApplication.CreateBuilder(args);

// Configure services for CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        builder =>
        {
            builder.WithOrigins("http://localhost", "http://localhost:4200")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();

builder.Services.AddScoped<BZ98WebsocketClient>();

var app = builder.Build();

app.UseCors("AllowAngularApp");

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
    app.UseExceptionHandler("/Home/Error");
}

app.UseForwardedHeaders();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Start();

var serviceScopeFactory = app.Services.GetService<IServiceScopeFactory>();

if (serviceScopeFactory != null)
{
    using (var scope = serviceScopeFactory.CreateScope())
    {
        var bz98WebSocketClientService = scope.ServiceProvider.GetRequiredService<BZ98WebsocketClient>();

        if (bz98WebSocketClientService != null)
        {
            await bz98WebSocketClientService.StartWebsocketClientAsync();
        }
    }
}