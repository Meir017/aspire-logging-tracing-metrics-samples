using System.Diagnostics;
using System.Net.Http.Json;

namespace distributed_app_demo.WorkerService;

public class WeatherWorker(IHttpClientFactory httpClientFactory, ILogger<WeatherWorker> logger) : BackgroundService
{
    private static readonly ActivitySource activitySource = new(ActivitySources.CustomActivities, "1.0.0");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            // must be 32 characters long
            var existingTraceId = Guid.NewGuid().ToString("N");

            var context = new ActivityContext(
                traceId: ActivityTraceId.CreateFromString(existingTraceId), 
                spanId: ActivitySpanId.CreateRandom(), 
                traceFlags: ActivityTraceFlags.Recorded,
                isRemote: true);
            using (var activity = activitySource.StartActivity("GetWeatherForecast", ActivityKind.Consumer, context))
            {                
                using var client = httpClientFactory.CreateClient("externalapiservice");
                var forecast = await client.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast", stoppingToken);
                if (forecast is null)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "No weather forecast data received.");
                    logger.LogWarning("No weather forecast data received. {ExistingTraceId}", existingTraceId);
                }
                else
                {
                    activity?.SetStatus(ActivityStatusCode.Ok, "Weather forecast data received successfully.");
                    logger.LogInformation("Got weather forecasts: {Count} {ExistingTraceId}", forecast.Length, existingTraceId);
                }
            }
        }
    }

    record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
