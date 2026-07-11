# GapMiner

> Internal intelligence engine that scrapes competitor app reviews, clusters
> them with LLMs, and surfaces **monetizable feature-gap opportunities** via a
> Blazor dashboard.

[![ci](https://github.com/nam20485/gap-miner-v2-papa85/actions/workflows/ci.yml/badge.svg)](https://github.com/nam20485/gap-miner-v2-papa85/actions/workflows/ci.yml)

---

## Overview

GapMiner is **not** a customer-facing product. It is an internal strategic tool
that:

1. **Scrapes** 1–3 star reviews of competitor apps from digital marketplaces
   (Shopify App Store, Chrome Web Store, G2, Apple App Store) via [Apify](https://apify.com/).
2. **Analyzes** negative reviews using LLMs (Semantic Kernel + Azure OpenAI /
   Anthropic) to identify substantial, monetizable feature gaps.
3. **Surfaces** ranked opportunities through an internal Blazor dashboard for
   human strategic review.

The platform does **not** build, ship, or monetize micro-SaaS applications —
that is a downstream activity.

---

## Tech Stack

| Component        | Technology                         | Version |
|------------------|------------------------------------|---------|
| Runtime          | .NET SDK                           | 8.0.x (LTS) |
| Orchestration    | .NET Aspire                        | 8.2.x   |
| Web UI           | Blazor Web App (Interactive Server)| 8.0     |
| API              | ASP.NET Core Minimal API           | 8.0     |
| Database         | PostgreSQL                         | 16.x    |
| Vector Extension | pgvector                           | 0.7.x   |
| ORM              | Entity Framework Core              | 8.0.x   |
| Cache / Queue    | Redis (StackExchange.Redis)        | 7.x     |
| Job Scheduling   | Hangfire                           | 1.8.x   |
| AI Orchestration | Microsoft.SemanticKernel           | 1.20.x  |
| Scraping         | Apify API                          | v2      |
| HTTP Client      | Refit                              | 7.x     |
| Validation       | FluentValidation                   | 11.x    |
| Testing          | xUnit + NSubstitute + Testcontainers | latest |

All package versions are pinned centrally in [`Directory.Packages.props`](Directory.Packages.props).

---

## Solution Architecture

```
GapMiner.sln
├── src/
│   ├── GapMiner.AppHost/           # Aspire orchestrator (Postgres + Redis + services)
│   ├── GapMiner.ServiceDefaults/   # Shared Aspire defaults (telemetry, health checks)
│   ├── GapMiner.Domain/            # Pure domain entities & value objects
│   ├── GapMiner.Infrastructure/    # EF Core, Redis, Apify, Semantic Kernel
│   ├── GapMiner.Application/       # Use cases / command handlers
│   ├── GapMiner.Api/               # Minimal API gateway
│   ├── GapMiner.ScraperWorker/     # Background worker: scraping
│   ├── GapMiner.AIWorker/          # Background worker: LLM analysis
│   └── GapMiner.Web/               # Blazor dashboard
└── tests/
    ├── GapMiner.Domain.Tests/
    ├── GapMiner.Infrastructure.Tests/
    ├── GapMiner.Application.Tests/
    ├── GapMiner.Api.Tests/
    └── GapMiner.Integration.Tests/ # End-to-end with Testcontainers
```

The solution follows a clean-architecture dependency flow:

```
Domain ← Infrastructure ← Application ← { Api, ScraperWorker, AIWorker, Web }
                                          └── all reference ServiceDefaults
```

---

## Getting Started

### Prerequisites

- [.NET SDK 8.0.x](https://dotnet.microsoft.com/download/dotnet/8.0) (`global.json` pins `8.0.x` with `rollForward: latestFeature`)
- [Docker](https://www.docker.com/) (for the compose stack / Testcontainers)
- An Apify account + API token (for scraping)
- An LLM provider key (Azure OpenAI **or** Anthropic)

### Local development (recommended: Aspire)

The [AppHost](src/GapMiner.AppHost) orchestrates PostgreSQL (with pgvector),
Redis, and all application services automatically:

```bash
# 1. Configure secrets (Apify token, LLM keys). Never commit secrets.
cp .env.example .env
#   edit .env, then export:  set -a; source .env; set +a

# 2. Run the Aspire AppHost (launches Postgres + Redis + all services)
dotnet run --project src/GapMiner.AppHost
```

The Aspire dashboard is available at the URL printed in the console
(typically `https://localhost:18888`).

### Standalone (Docker Compose)

```bash
cp .env.example .env   # fill in API keys
docker compose up --build
```

- API:        <http://localhost:8080>
- Web:        <http://localhost:8081>
- PostgreSQL: `localhost:5432`
- Redis:      `localhost:6379`

### Build & test

```bash
dotnet restore GapMiner.sln
dotnet build GapMiner.sln --configuration Release
dotnet test  GapMiner.sln --configuration Release
```

---

## Configuration

| Variable                    | Description                          | Required |
|-----------------------------|--------------------------------------|----------|
| `APIFY_TOKEN`               | Apify API bearer token (scraping)    | yes      |
| `AI__Provider`              | `azureopenai` or `anthropic`         | yes      |
| `AI__AzureOpenAI__Endpoint` | Azure OpenAI endpoint                | provider-dependent |
| `AI__AzureOpenAI__ApiKey`   | Azure OpenAI key                     | provider-dependent |
| `POSTGRES_PASSWORD`         | PostgreSQL password (compose only)   | compose  |

See [`.env.example`](.env.example) for the full template. **Secrets are never
hardcoded** — they are sourced from environment variables / Aspire secret
parameters.

---

## Project Status

This repository is at **Phase 0 — Environment & Foundation**. The solution
scaffold, central package management, CI pipeline, and Docker setup are in place.
Domain entities, data access, and the scraper/intelligence pipelines are
implemented incrementally per the task breakdown in
[`plan_docs/development-plan.md`](plan_docs/development-plan.md).

Architecture decisions are recorded in [`docs/adr/`](docs/adr/).

---

## License

Copyright (c) GapMiner. All rights reserved. Internal use only.
