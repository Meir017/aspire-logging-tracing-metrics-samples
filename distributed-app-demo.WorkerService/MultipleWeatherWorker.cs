using System.Diagnostics;
using System.Net.Http.Json;

namespace distributed_app_demo.WorkerService;

public class MultipleWeatherWorker(IHttpClientFactory httpClientFactory, ILogger<MultipleWeatherWorker> logger) : BackgroundService
{
    private static readonly ActivitySource activitySource = new(ActivitySources.CustomActivities, "1.0.0");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            using (var activity = activitySource.StartActivity("GetMultipleWeatherForecast", ActivityKind.Consumer))
            {
                var count = Random.Shared.Next(1, 7);
                activity?.SetTag("Count", count);
                for (var i = 0; i < count; i++)
                {
                    using (var childActivity = activitySource.StartActivity("GetWeatherForecast", ActivityKind.Consumer))
                    {
                        using var client = httpClientFactory.CreateClient("externalapiservice");
                        var forecast = await client.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast", stoppingToken);
                        if (forecast is null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "No weather forecast data received.");
                            logger.LogWarning("No weather forecast data received.");
                        }
                        else
                        {
                            activity?.SetStatus(ActivityStatusCode.Ok, "Weather forecast data received successfully.");
                            logger.LogInformation("Got weather forecasts: {Count}", forecast.Length);
                        }
                    }
                }
            }
        }
    }

    record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}