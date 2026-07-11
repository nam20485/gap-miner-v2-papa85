using GapMiner.ServiceDefaults;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Shared Aspire service defaults (OpenTelemetry, health checks, etc.).
builder.AddServiceDefaults();

// TODO(T-2.3): register Hangfire, Apify client, repositories, and the
// ScrapeReviewsJob hosted service.

var host = builder.Build();
host.Run();
