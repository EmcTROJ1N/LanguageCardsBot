using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace LanguageCardsBot.Observability.Extensions;

/// <summary>
/// Методы расширения для регистрации OpenTelemetry distributed tracing.
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Добавить OpenTelemetry distributed tracing с экспортом в Jaeger.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <param name="serviceName">Название микросервиса для идентификации в Jaeger.</param>
    /// <param name="serviceVersion">Версия сервиса.</param>
    /// <param name="additionalActivitySources">Дополнительные Activity Sources для трассировки.</param>
    public static IServiceCollection AddOpenTelemetryTracing(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        string serviceVersion = "1.0.0",
        params string[] additionalActivitySources)
    {
        var jaegerEndpoint = configuration["OpenTelemetry:Jaeger:Endpoint"] ?? "http://localhost:4317";
        var enableConsoleExporter = configuration.GetValue<bool>("OpenTelemetry:EnableConsoleExporter");

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion,
                    serviceInstanceId: Environment.MachineName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;

                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            activity.SetTag("http.method", request.Method);
                            activity.SetTag("http.url", request.Path);
                            activity.SetTag("http.user_agent", request.Headers.UserAgent.ToString());
                        };

                        options.EnrichWithHttpResponse = (activity, response) =>
                        {
                            activity.SetTag("http.status_code", response.StatusCode);
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;

                        options.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                            activity.SetTag("http.request.method", request.Method.ToString());
                            activity.SetTag("http.request.url", request.RequestUri?.ToString());
                        };
                    })
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.EnrichWithIDbCommand = (activity, command) =>
                        {
                            activity.SetTag("db.operation", command.CommandText);
                        };
                    })
                    .AddSource(serviceName);

                foreach (var activitySource in additionalActivitySources)
                {
                    tracing.AddSource(activitySource);
                }

                tracing.AddSource("MassTransit");

                tracing.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(jaegerEndpoint);
                });

                if (enableConsoleExporter)
                    tracing.AddConsoleExporter();
            });

        return services;
    }
}
