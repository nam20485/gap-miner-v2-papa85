using GapMiner.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Shared Aspire service defaults (OpenTelemetry, health checks, etc.).
builder.AddServiceDefaults();

// TODO(T-4.1): register Application/Infrastructure services, FluentValidation,
// endpoint definitions (Targets, Jobs, Gaps), and Swagger/OpenAPI.

WebApplication app = builder.Build();

app.Run();
