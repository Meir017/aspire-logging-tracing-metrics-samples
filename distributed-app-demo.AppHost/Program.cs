var builder = DistributedApplication.CreateBuilder(args);

var externalApiService = builder.AddProject<Projects.distributed_app_demo_ExternalApiService>("externalapiservice")
    .WithExternalHttpEndpoints()
    .WithHttpsHealthCheck("/health");

var apiService = builder.AddProject<Projects.distributed_app_demo_ApiService>("apiservice")
    .WithHttpsHealthCheck("/health")
    .WithReference(externalApiService);

builder.AddProject<Projects.distributed_app_demo_WorkerService>("workerservice")
    .WithReference(externalApiService);

builder.AddProject<Projects.distributed_app_demo_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpsHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
