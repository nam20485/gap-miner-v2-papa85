# ADR-0001: Architecture Overview & Technology Stack

- **Status:** Accepted
- **Date:** 2026-07-11
- **Supersedes:** —
- **References:** `plan_docs/development-plan.md` (§3, §4, §5)

## Context

GapMiner is an internal intelligence engine whose sole mission is to scrape
competitor app reviews, cluster negative reviews with LLMs, and surface
monetizable feature gaps. It is **not** a customer-facing product. The system
must support: asynchronous scraping jobs, LLM-driven map-reduce analysis over
pgvector clusters, and a Blazor dashboard for human strategic review.

## Decision

We adopt a **clean-architecture, service-composed** design orchestrated by
**.NET Aspire**, with the following layering and stack:

### Layering & dependency flow

```
Domain ← Infrastructure ← Application ← { Api, ScraperWorker, AIWorker, Web }
```

- **Domain** — pure entities/value objects, no persistence references.
- **Infrastructure** — EF Core (PostgreSQL + pgvector), Redis, Apify, Semantic Kernel.
- **Application** — use cases / command handlers, FluentValidation.
- **Api / ScraperWorker / AIWorker / Web** — executables referencing
  Application + Infrastructure + Domain + ServiceDefaults.

### Technology stack (pinned versions)

| Concern        | Choice                                           |
|----------------|--------------------------------------------------|
| Runtime        | .NET SDK 8.0.x (LTS), `rollForward: latestFeature` |
| Orchestration  | .NET Aspire 8.2.x (AppHost + ServiceDefaults)    |
| Web UI         | Blazor Web App, Interactive Server               |
| API            | ASP.NET Core Minimal API                         |
| Database       | PostgreSQL 16 with pgvector 0.7 (vector(3072))   |
| Cache / Queue  | Redis 7 (StackExchange.Redis)                    |
| Jobs           | Hangfire 1.8.x                                   |
| AI             | Microsoft.SemanticKernel 1.20.x                  |
| Scraping       | Apify API v2 (Refit client)                      |
| Validation     | FluentValidation 11.x                            |
| Testing        | xUnit + NSubstitute + Testcontainers             |

Versions are pinned centrally in `Directory.Packages.props`. Code-quality
analyzers (`TreatWarningsAsErrors`, nullable reference types, file-scoped
namespaces) are enforced solution-wide via `Directory.Build.props` and
`.editorconfig`.

### CI/CD

A single GitHub Actions workflow (`.github/workflows/ci.yml`) builds and tests
the solution on push/PR. **Every** GitHub Action is pinned by full 40-char SHA
(supply-chain hardening). .NET SDK 8.0.x is provisioned by `setup-dotnet`.

## Consequences

- **Positive:** Central package management + TreatWarningsAsErrors prevents
  version drift and warning regression. Aspire gives local-dev parity with
  deployment (Postgres + Redis + services from one command). The layered design
  keeps domain logic testable in isolation.
- **Negative:** The AppHost + Aspire SDK adds a build-time dependency on the
  `Aspire.AppHost.Sdk`; the exact CPM list pairs with the Aspire 8.2 line, so
  upgrading Aspire requires updating the SDK + hosting packages together.
- **Risks (from plan §14):** pgvector extension must be present in the container
  (we use `pgvector/pgvector:pg16`); LLM JSON output must be schema-validated
  before persistence (handled in T-3.4).
