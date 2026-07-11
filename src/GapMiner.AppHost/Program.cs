using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL 16 with pgvector. The "gapminer" database is what the
// Infrastructure DbContext targets (see T-1.2: HasPostgresExtension("vector")).
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("gapminer");

// Redis 7 cache + job queue backend.
var redis = builder.AddRedis("redis");

// Minimal API gateway.
builder.AddProject<Projects.GapMiner_Api>("api")
    .WithReference(postgres)
    .WithReference(redis);

// Background worker: competitor review scraping (Apify).
builder.AddProject<Projects.GapMiner_ScraperWorker>("scraper-worker")
    .WithReference(postgres)
    .WithReference(redis);

// Background worker: LLM embedding + gap analysis (Semantic Kernel).
builder.AddProject<Projects.GapMiner_AIWorker>("ai-worker")
    .WithReference(postgres)
    .WithReference(redis);

// Blazor dashboard.
builder.AddProject<Projects.GapMiner_Web>("web")
    .WithReference(postgres)
    .WithReference(redis);

builder.Build().Run();
