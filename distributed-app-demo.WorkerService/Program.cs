using distributed_app_demo.WorkerService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHttpClient("externalapiservice", http => http.BaseAddress = new Uri("https://externalapiservice"));

builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<WeatherWorker>();

var host = builder.Build();

host.Run();
