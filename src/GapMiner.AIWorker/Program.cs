using GapMiner.ServiceDefaults;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Shared Aspire service defaults (OpenTelemetry, health checks, etc.).
builder.AddServiceDefaults();

// TODO(T-3.1): register Semantic Kernel, embedding service, and the
// EmbedReviewsJob / AnalyzeGapsJob hosted services.

var host = builder.Build();
host.Run();
