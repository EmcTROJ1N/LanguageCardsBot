using LanguageCardsBot.Observability.Extensions;
using Prometheus;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetryTracing(builder.Configuration, "ApiGateway");
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
var app = builder.Build();
app.UseHttpMetrics();
app.MapReverseProxy();
app.MapMetrics();

app.Run();
