# aspire-logging-tracing-metrics-samples

> **Custom Activities in Tracing**
>
> This sample demonstrates how to add custom activities to tracing using a dedicated `ActivitySource`.  
> The `distributed-app-demo.ServiceDefaults.ActivitySources` class defines a constant `CustomActivities` which is used as the name for the custom `ActivitySource`.  
> When you create activities using this source, they will appear in the traces and logs, allowing you to track custom operations in your application.
>
> Example:
> ```csharp
> using var activitySource = new ActivitySource(ActivitySources.CustomActivities);
> using var activity = activitySource.StartActivity("MyCustomOperation");
> // ... your code ...
> ```
> See [`ActivitySources.cs`](distributed-app-demo.ServiceDefaults/ActivitySources.cs) for the definition.

Examples using aspire's dashboard:

Multiple activities from a single process:

Traces:

![multiple-activities-tracing](assets/multiple-activities-tracing.png)

Logs:
![multiple-activities-logs](assets/multiple-activities-logs.png)