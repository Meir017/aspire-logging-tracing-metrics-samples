using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddHttpClient("externalapiservice", http => http.BaseAddress = new Uri("https://externalapiservice"));

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/weatherforecast", async (
    [FromServices] IMemoryCache cache, 
    [FromServices] IHttpClientFactory httpClientFactory,
    [FromServices] ILogger<Program> logger) =>
{
    const string cacheKey = "weatherforecast";

    return await cache.GetOrCreateAsync(cacheKey, async entry =>
    {
        logger.LogInformation("Cache miss for {CacheKey}, fetching data from external API", cacheKey);
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(15);
        var forecast = await httpClientFactory.CreateClient("externalapiservice").GetFromJsonAsync<WeatherForecast[]>("/weatherforecast");
        return forecast;
    });
})
.WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
