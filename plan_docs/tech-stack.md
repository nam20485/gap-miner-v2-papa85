# GapMiner — Technology Stack

**Source of truth:** `plan_docs/development-plan.md` §3 (Technology Stack — Exact Versions, Do Not Deviate) and T-0.1 (`Directory.Packages.props` reference).
**Status:** Authoritative for all implementation tasks. Versions MUST NOT be invented or floated (Agent Rule R2).

---

## 1. Core Stack

| Component | Technology | Version |
|---|---|---|
| Runtime | .NET SDK | 8.0.x (LTS) |
| Orchestration | .NET Aspire | 8.2.x |
| Web UI | Blazor Web App (Interactive Server) | 8.0 |
| API | ASP.NET Core Minimal API | 8.0 |
| Database | PostgreSQL | 16.x |
| Vector Extension | pgvector | 0.7.x |
| ORM | Entity Framework Core | 8.0.x |
| Queue / Cache | Redis (StackExchange.Redis) | 7.x |
| Job Scheduling | Hangfire | 1.8.x |
| AI Orchestration | Microsoft.SemanticKernel | 1.20.x |
| LLM Provider | Azure OpenAI (GPT-4o) **or** Anthropic Claude 3.5 Sonnet | Latest stable |
| Embeddings | `text-embedding-3-large` (OpenAI) or `voyage-3` | Latest stable |
| Scraping | Apify API | v2 REST |
| HTTP Client | Refit | 7.x |
| Validation | FluentValidation | 11.x |
| Testing | xUnit + NSubstitute + Testcontainers | Latest stable |
| Code Quality | SonarAnalyzer.CSharp + StyleCop.Analyzers | Latest stable |

---

## 2. Pinned NuGet Package Versions (Central Package Management)

These are the exact versions declared in the T-0.1 reference excerpt of `development-plan.md`. They MUST be used verbatim in `Directory.Packages.props`.

| Package | Version |
|---|---|
| `Aspire.Hosting` | 8.2.2 |
| `Aspire.Hosting.PostgreSQL` | 8.2.2 |
| `Aspire.Hosting.Redis` | 8.2.2 |
| `Microsoft.SemanticKernel` | 1.20.0 |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 8.0.10 |
| `Pgvector.EntityFrameworkCore` | 0.2.0 |
| `Hangfire.Core` | 1.8.14 |
| `Refit.HttpClientFactory` | 7.2.1 |
| `FluentValidation` | 11.10.0 |

All remaining packages from §1 follow their stated major.minor ranges with Central Package Management enforcing a single declaration site. **Never use floating versions** (Semantic Kernel API changes break compilation — see Risk table in `development-plan.md` §14).

---

## 3. Supporting Libraries (from plan §5, §11, T-5.x)

| Concern | Library |
|---|---|
| UI components | MudBlazor **or** Radzen.Blazor (T-5.1) |
| Charts | `Blazor-ApexCharts` **or** `AntDesign.Charts` (T-5.4 Opportunity Matrix) |
| Resilience | Polly (retry on 429/5xx — T-2.1, T-3.2) |
| API docs | Swashbuckle / Swagger/OpenAPI (T-4.1) |
| Structured logging | Serilog with JSON sink (T-6.2) |
| Telemetry | OpenTelemetry (via Aspire ServiceDefaults — T-0.3) |

---

## 4. .NET SDK Version Constraint — CRITICAL

The development plan pins the **runtime target to .NET SDK 8.0.x (LTS)**. However, the devcontainer image that runs the orchestration tooling ships **.NET SDK 10.0.x** (see `AGENTS.md` → `available_tools`).

Resolution for implementation tasks (Phase 0, T-0.1):

- Create a **`global.json`** at the repository root pinning the SDK to the `8.0.x` band:

  ```json
  {
    "sdk": {
      "version": "8.0.100",
      "rollForward": "latestFeature",
      "allowPrerelease": false
    }
  }
  ```

- `rollForward: latestFeature` lets the build use the newest installed `8.0.*` SDK patch/feature band while refusing to silently upgrade to SDK 10.
- This ensures `dotnet build` / `dotnet test` target the **net8.0** TFM that all packages in §2 are verified against, regardless of which SDKs the host machine has installed.
- If SDK 8.0.x is not present in a given environment, `dotnet` will error explicitly rather than build against an unverified SDK.

> Note: This file is created during Phase 0 (T-0.1 Repository Bootstrap). This document records the decision; it does **not** create the file (planning-only).

---

## 5. Rationale Notes

- **Aspire 8.2.x** provides the `AddPostgres` / `AddRedis` hosting primitives and the dashboard used for observability (T-0.2, T-6.2).
- **pgvector 0.7.x + `Pgvector.EntityFrameworkCore`** enable the `<=>` cosine distance operator used by the semantic clustering query in T-3.3. The `vector(3072)` column width matches `text-embedding-3-large`.
- **Semantic Kernel 1.20.x** abstracts the LLM provider so the codebase can switch between Azure OpenAI and Anthropic via `IConfiguration["AI:Provider"]` (T-3.1) without touching call sites.
- **Refit 7.x** generates the typed Apify HTTP client from an interface, with Polly providing the retry policy required by T-2.1.
- **Hangfire 1.8.x** schedules `ScrapeReviewsJob`, `EmbedReviewsJob`, and `AnalyzeGapsJob` and powers the Jobs dashboard page (T-5.3).
- **Testcontainers** spins up ephemeral PostgreSQL 16 + Redis 7 instances for integration tests (T-1.3, T-1.4, T-6.1), guaranteeing the `pgvector` extension and Redis List semantics are exercised against the real engines.
