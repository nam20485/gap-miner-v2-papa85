using GapMiner.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Shared Aspire service defaults (OpenTelemetry, health checks, etc.).
builder.AddServiceDefaults();

// TODO(T-5.1): add Blazor Server services (Razor Components, Interactive Server
// render mode), layout, and the component library (MudBlazor/Radzen).

WebApplication app = builder.Build();

app.Run();
