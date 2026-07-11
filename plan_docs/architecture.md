# GapMiner — System Architecture

**Source of truth:** `plan_docs/development-plan.md` (§1 strategic context, §4 repository layout, §5 naming conventions, §7–§12 phase tasks, §16 parallel execution map).
**Status:** Architectural baseline for all implementation tasks.

---

## 1. High-Level Architecture

GapMiner is an **internal intelligence engine** (not a customer-facing product). It is a **.NET Aspire-orchestrated distributed application** composed of cooperating projects sharing a clean-architecture layering.

The platform's mission is a single linear pipeline:

> **Scrape** competitor reviews → **Ingest** into PostgreSQL → **Embed** into pgvector → **Cluster** semantically → **Analyze** with LLMs (map-reduce) → **Surface** ranked feature gaps on a Blazor dashboard.

Aspire's `AppHost` declares the infrastructure (PostgreSQL+pgvector, Redis) and the application projects, wires connection strings, and provides the observability dashboard. `ServiceDefaults` injects OpenTelemetry tracing/logging/metrics and health checks into every project via a single `AddServiceDefaults()` call.

---

## 2. Core Services

| Project | Layer | Responsibility |
|---|---|---|
| `GapMiner.AppHost` | Orchestration | Aspire host. Declares PostgreSQL (with `pgvector`), Redis, and all application projects; wires `.WithReference(...)` connection strings; runs the Aspire dashboard. (T-0.2) |
| `GapMiner.ServiceDefaults` | Shared | Extension `.AddServiceDefaults()` adds OpenTelemetry, health checks (`/health`, `/alive`), and unified logging to every project. (T-0.3) |
| `GapMiner.Domain` | Domain | Pure domain entities (`CompetitorTarget`, `Review`, `FeatureGap`), value objects (`SeverityScore`), enums (`MarketplaceKind`). No EF references — persistence-ignorant. (T-1.1) |
| `GapMiner.Infrastructure` | Infrastructure | EF Core `GapMinerDbContext` + migrations + repositories; Redis job queue; Apify scraping client; Semantic Kernel gap analyzer + prompt library. (T-1.2–T-1.4, T-2.1, T-3.1, T-3.4) |
| `GapMiner.Application` | Application | Use-case command/query handlers (`IngestTarget`, `RunScraper`, `AnalyzeReviews`, `GetFeatureGaps`) and DTOs. CQRS-like separation of commands and queries. |
| `GapMiner.Api` | Presentation (API) | ASP.NET Core Minimal API gateway: `/api/v1/targets`, `/api/v1/gaps`, `/api/v1/jobs`. FluentValidation + Swagger/OpenAPI. (T-4.1) |
| `GapMiner.ScraperWorker` | Worker | Background service hosting the Hangfire `ScrapeReviewsJob` — dequeues scrape commands, drives the Apify actor lifecycle, maps + idempotently inserts reviews. (T-2.3) |
| `GapMiner.AIWorker` | Worker | Background service hosting `EmbedReviewsJob` and `AnalyzeGapsJob` (map-reduce gap analysis). (T-3.2, T-3.4) |
| `GapMiner.Web` | Presentation (UI) | Blazor Web App (Interactive Server) dashboard: Dashboard, Targets, Jobs, Opportunity Matrix pages. (T-5.1–T-5.4) |

**Test projects** mirror the structure: `Domain.Tests`, `Infrastructure.Tests`, `Application.Tests`, `Api.Tests`, and `Integration.Tests` (end-to-end with Testcontainers).

---

## 3. Data Flow

```
 ┌──────────┐   1. ScrapeTargetCommand    ┌────────────────┐
 │  Web/API │ ─────────────────────────▶  │ Redis Job Queue │
 └──────────┘                              └───────┬────────┘
                                                   │ 2. dequeue
                                                   ▼
                                        ┌─────────────────────┐
                                        │   ScraperWorker     │
                                        │  ScrapeReviewsJob   │
                                        └────────┬────────────┘
                                                 │ 3. Apify v2 actor run
                                                 ▼
                                        ┌─────────────────────┐
                                        │   Apify (external)  │
                                        └────────┬────────────┘
                                                 │ 4. dataset items
                                                 ▼
                                        ┌─────────────────────┐
                                        │  PostgreSQL +       │  5. idempotent bulk insert
                                        │  pgvector           │  ON CONFLICT DO NOTHING
                                        └────────┬────────────┘
                                                 │ 6. AnalyzeReviewsCommand
                                                 ▼
                                        ┌─────────────────────┐
                                        │     AIWorker         │
                                        │  EmbedReviewsJob ────┼─▶ 7. text-embedding-3-large
                                        │  (store vector)      │    → Review.Embedding(3072)
                                        └────────┬────────────┘
                                                 │ 8. k-means + KNN (<=>)
                                                 ▼
                                        ┌─────────────────────┐
                                        │  AnalyzeGapsJob      │
                                        │  Map (per cluster) ──┼─▶ 9. LLM MapReviewsPrompt
                                        │  Reduce (dedupe) ────┼─▶10. LLM ReduceGapsPrompt
                                        └────────┬────────────┘
                                                 │ 11. persist FeatureGap
                                                 ▼
                                        ┌─────────────────────┐
                                        │  Web Dashboard       │ 12. Opportunity Matrix
                                        │  (Blazor)            │     severity × frequency
                                        └─────────────────────┘
```

**Pipeline stage summary:** Scrape → Ingest → Embed → Cluster → Analyze → Surface.

Idempotency is enforced at two points: (a) `Review(CompetitorTargetId, SourceReviewId)` unique composite index with `ON CONFLICT DO NOTHING` (T-1.2), and (b) gap deduplication by title cosine similarity > 0.8 in the reduce phase (T-3.4).

---

## 4. Repository Layout

Verbatim from `development-plan.md` §4:

```
GapMiner/
├── GapMiner.sln
├── Directory.Build.props                  # Shared MSBuild properties
├── Directory.Packages.props               # Central Package Management
├── .editorconfig
├── global.json                            # Pin SDK version
├── README.md
│
├── src/
│   ├── GapMiner.AppHost/                  # Aspire orchestrator
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── GapMiner.ServiceDefaults/          # Shared Aspire service defaults
│   │   └── Extensions.cs
│   │
│   ├── GapMiner.Domain/                   # Pure domain entities (no EF refs)
│   │   ├── Entities/
│   │   │   ├── CompetitorTarget.cs
│   │   │   ├── Review.cs
│   │   │   └── FeatureGap.cs
│   │   ├── ValueObjects/
│   │   │   └── SeverityScore.cs
│   │   └── Enums/
│   │       └── MarketplaceKind.cs
│   │
│   ├── GapMiner.Infrastructure/           # EF Core, Redis, Apify, Semantic Kernel
│   │   ├── Persistence/
│   │   │   ├── GapMinerDbContext.cs
│   │   │   ├── Configurations/
│   │   │   ├── Migrations/
│   │   │   └── Repositories/
│   │   ├── Queueing/
│   │   │   ├── IJobQueue.cs
│   │   │   └── RedisJobQueue.cs
│   │   ├── Scraping/
│   │   │   ├── IApifyClient.cs
│   │   │   └── ApifyClient.cs
│   │   └── AI/
│   │       ├── IGapAnalyzer.cs
│   │       ├── SemanticKernelGapAnalyzer.cs
│   │       └── Prompts/
│   │           ├── EmbeddingPrompt.txt
│   │           ├── MapReviewsPrompt.txt
│   │           └── ReduceGapsPrompt.txt
│   │
│   ├── GapMiner.Application/              # Use cases / command handlers
│   │   ├── Commands/
│   │   │   ├── IngestTarget/
│   │   │   ├── RunScraper/
│   │   │   └── AnalyzeReviews/
│   │   ├── Queries/
│   │   │   └── GetFeatureGaps/
│   │   └── DTOs/
│   │
│   ├── GapMiner.Api/                      # Minimal API gateway
│   │   ├── Program.cs
│   │   ├── Endpoints/
│   │   │   ├── TargetsEndpoints.cs
│   │   │   ├── JobsEndpoints.cs
│   │   │   └── GapsEndpoints.cs
│   │   └── Middleware/
│   │
│   ├── GapMiner.ScraperWorker/            # Background worker: scraping
│   │   ├── Program.cs
│   │   └── Jobs/
│   │       └── ScrapeReviewsJob.cs
│   │
│   ├── GapMiner.AIWorker/                 # Background worker: LLM analysis
│   │   ├── Program.cs
│   │   └── Jobs/
│   │       ├── EmbedReviewsJob.cs
│   │       └── AnalyzeGapsJob.cs
│   │
│   └── GapMiner.Web/                      # Blazor dashboard
│       ├── Program.cs
│       ├── Components/
│       │   ├── Layout/
│       │   ├── Pages/
│       │   │   ├── Dashboard.razor
│       │   │   ├── Targets.razor
│       │   │   ├── Jobs.razor
│       │   │   └── OpportunityMatrix.razor
│       │   └── Shared/
│       └── wwwroot/
│
├── tests/
│   ├── GapMiner.Domain.Tests/
│   ├── GapMiner.Infrastructure.Tests/
│   ├── GapMiner.Application.Tests/
│   ├── GapMiner.Api.Tests/
│   └── GapMiner.Integration.Tests/        # End-to-end with Testcontainers
│
└── docs/
    ├── prompts/                            # Version-controlled prompt library
    └── adr/                                # Architecture Decision Records
```

---

## 5. Naming Conventions

Verbatim from `development-plan.md` §5:

| Element | Convention | Example |
|---|---|---|
| Namespace | `GapMiner.{Layer}.{Feature}` | `GapMiner.Infrastructure.Persistence` |
| Entity class | PascalCase, singular | `CompetitorTarget` |
| Repository | `I{Entity}Repository` / `{Entity}Repository` | `ICompetitorTargetRepository` |
| Command | `{Verb}{Noun}Command` | `IngestTargetCommand` |
| Command Handler | `{Verb}{Noun}Handler` | `IngestTargetHandler` |
| Endpoint route | kebab-case, plural nouns | `/api/v1/targets` |
| Database column | snake_case (EF mapping) | `marketplace_url` |
| Redis queue key | `gapminer:{domain}:{action}` | `gapminer:scrape:pending` |

---

## 6. Key Design Decisions

### 6.1 Clean Architecture Layering

Dependency direction flows inward: `Domain` depends on nothing; `Infrastructure` and `Application` depend on `Domain`; `Api`, `Web`, and the Workers depend on `Application`. This keeps the domain persistence-ignorant and makes every outer layer substitutable for testing.

### 6.2 CQRS-like Command/Query Separation

The `Application` project splits write-side **Commands** (`IngestTarget`, `RunScraper`, `AnalyzeReviews`) from read-side **Queries** (`GetFeatureGaps`). This is a lightweight pattern (no full MediatR pipeline mandated) that keeps use cases atomic and independently testable — each fits within a single agent context window (Agent Rule: ≤ 8 files per task).

### 6.3 Idempotent Operations

Every pipeline stage is safe to retry:

- **Scrape:** `Review(CompetitorTargetId, SourceReviewId)` unique composite index + `ON CONFLICT DO NOTHING` (T-1.2) prevents duplicate reviews on re-scrape.
- **Embed:** `GetUnembeddedAsync` filters `Embedding IS NULL`, so re-runs only process missing vectors.
- **Analyze:** Reduce-phase deduplication by title cosine similarity > 0.8 collapses duplicate gaps (T-3.4).
- **Jobs:** Every Hangfire job carries `MaxRetryAttempts` (default 3); failures route to a Redis dead-letter queue (T-6.3).

### 6.4 Map-Reduce Gap Analysis (T-3.4)

LLM cost and context-window limits are managed by a two-phase strategy:

- **Map:** each review cluster (max 8,000 tokens) is summarized independently into candidate gaps.
- **Reduce:** candidates are aggregated, deduplicated, and re-ranked into a final ≤ 10-item list with strict JSON schema validation and one retry-with-stricter-prompt before dead-lettering.

### 6.5 Observability by Default

`ServiceDefaults` injects OpenTelemetry into every project (T-0.3). Custom metrics — `gapminer.reviews.scraped`, `gapminer.gaps.identified`, `gapminer.llm.tokens.consumed` (T-6.2) — surface in the Aspire dashboard. Serilog scrubs `*Password*`, `*Token*`, `*Key*` properties to prevent secret leakage (§14 risk table).

### 6.6 Parallel Execution Map

Per §16, independent task groups may proceed concurrently with explicit merge gates:
- **Gate 1 (Day 8):** Data (T-0.x → T-1.3), Queue (T-1.4), AI setup (T-3.1) merge.
- **Gate 2 (Day 22):** Scrape (T-2.x) and Analyze (T-3.2 → T-3.4) merge.
- **Gate 3 (Day 28):** API (T-4.1) and UI (T-5.x) merge → hardening (T-6.x).
