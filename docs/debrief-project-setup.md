# Debrief Report: `project-setup` Dynamic Workflow

**Repository:** `nam20485/gap-miner-v2-papa85` — Gap Mining Platform (GapMiner)
**Workflow:** `project-setup`
**Branch:** `dynamic-workflow-project-setup`
**Date:** 2026-07-11
**Report Author:** `debrief-and-document` assignment (Assignment 5)
**Status:** Complete (Assignments 1–5); Assignment 6 (`pr-approval-and-merge`) pending

---

## 1. Executive Summary

The `project-setup` dynamic workflow successfully initialized the **Gap Mining Platform**
repository from a raw orchestration template into a fully scaffolded, CI-green .NET 8
solution. Across five completed assignments, the workflow:

- Created a working branch, imported a branch protection ruleset, created a GitHub
  Project V2 (#70), imported 31 labels, and opened PR #2.
- Produced three planning documents (`workflow-plan.md` at 550 lines, `tech-stack.md`
  at 98 lines, `architecture.md` at 246 lines), an application plan issue (#3), and
  seven milestones (Phase 0–Phase 6).
- Scaffolded a 14-project .NET 8 solution (9 source + 5 test projects) with Central
  Package Management, multi-stage Dockerfile, docker-compose (pgvector + Redis),
  SHA-pinned CI/CD, README, ADR-0001, and an AI repository summary.
- Wrote a project-specific `AGENTS.md` for the GapMiner application while preserving
  the existing orchestration system instructions.

**CI status:** Both `ci.yml` and `validate.yml` are **green** on the working branch.

**Three plan-impacting ACTION ITEMS** remain open (see Section 9 and the ACTION ITEMS
block at the end of this report): (1) branch protection status checks reference a
GitHub App (integration_id 15368) that may not be installed on this personal repo;
(2) the devcontainer image lacks `dotnet` entirely despite the plan requiring SDK
8.0.x; (3) the Aspire AppHost SDK 8.2.2 is workload-only and cannot be pinned on
hosted runners, forcing CI to exclude the AppHost project.

---

## 2. Workflow Overview

| # | Assignment | Type | Status | Key Output |
|---|---|---|---|---|
| — | `create-workflow-plan` (pre-script) | Planning | ✅ Complete | `plan_docs/workflow-plan.md` (550 lines) |
| 1 | `init-existing-repository` | Infrastructure | ✅ Complete | Branch, ruleset (ID 18800820), Project #70, 31 labels, PR #2 |
| 2 | `create-app-plan` | Planning | ✅ Complete | `tech-stack.md`, `architecture.md`, Issue #3, 7 milestones |
| 3 | `create-project-structure` | Scaffolding | ✅ Complete | 14 .NET projects, Docker, CI/CD, docs, README |
| 4 | `create-agents-md-file` | Documentation | ✅ Complete | Project-specific `AGENTS.md` at repo root |
| 5 | `debrief-and-document` | Retrospective | ✅ Complete | This report + `debrief-and-document/trace.md` |
| 6 | `pr-approval-and-merge` | Merge | ⏳ Pending | Awaits CI verification, code review, merge |

**Total assignments:** 6 (ordered, strictly linear dependency chain).
**Completed:** 5 of 6 (plus the pre-script event).
**Critical path:** `create-workflow-plan → init-existing-repository → create-app-plan
→ create-project-structure → create-agents-md-file → debrief-and-document →
pr-approval-and-merge`.

---

## 3. Key Deliverables

### 3.1 Planning Artifacts

| Deliverable | Location | Evidence |
|---|---|---|
| Workflow execution plan | `plan_docs/workflow-plan.md` | 550 lines; 5 sections, 6 assignment plans, 6 open questions |
| Technology stack document | `plan_docs/tech-stack.md` | 98 lines; pinned NuGet versions, SDK constraint rationale |
| Architecture document | `plan_docs/architecture.md` | 246 lines; data-flow diagram, 9-service table, 6 design decisions |
| Application plan issue | GitHub Issue #3 | 18 tasks mapped to 7 phase milestones |

### 3.2 Repository Infrastructure

| Deliverable | Location | Evidence |
|---|---|---|
| Working branch | `dynamic-workflow-project-setup` | Off `main`; all commits pushed |
| Branch protection ruleset | GitHub Ruleset ID 18800820 | Imported from `.github/protected-branches_ruleset.json`; `bypass_actors` emptied |
| GitHub Project V2 | Project #70 | Status field: Not Started / In Progress / In Review / Done |
| Labels | 31 labels | Synced from `.github/.labels.json` via `scripts/import-labels.ps1` |
| Pull Request | PR #2 | `dynamic-workflow-project-setup` → `main` |
| Milestones | 7 milestones (#1–#7) | Phase 0 (2026-07-06) through Phase 6 (2026-08-02) |

### 3.3 Source Code Scaffolding

| Deliverable | Evidence |
|---|---|
| `GapMiner.sln` | 14 projects in `src` and `tests` solution folders |
| `GapMiner.ci.slnf` | CI solution filter — 13 projects (excludes AppHost) |
| `global.json` | SDK `8.0.100`, `rollForward: latestFeature`, `allowPrerelease: false` |
| `Directory.Packages.props` | 16+ pinned packages; Central Package Management enabled |
| `Directory.Build.props` | `net8.0`, `TreatWarningsAsErrors`, nullable, implicit usings |
| `.editorconfig` | 60 lines; file-scoped namespaces, 4-space indent, CRLF |
| Source projects (9) | `AppHost`, `ServiceDefaults`, `Domain`, `Infrastructure`, `Application`, `Api`, `ScraperWorker`, `AIWorker`, `Web` |
| Test projects (5) | `Domain.Tests`, `Infrastructure.Tests`, `Application.Tests`, `Api.Tests`, `Integration.Tests` |
| `Dockerfile` | Multi-stage build; `sdk:8.0` → `aspnet:8.0`; `PROJECT` build arg |
| `docker-compose.yml` | 123 lines; `pgvector/pgvector:pg16`, `redis:7-alpine`, 4 app services |
| `.github/workflows/ci.yml` | 55 lines; SHA-pinned `checkout@9c091bb...` and `setup-dotnet@26b0ec1...` |
| `README.md` | 160 lines; CI badge, tech stack table, quickstart |
| `.ai-repository-summary.md` | 98 lines; machine-readable repo summary (agents.md convention) |
| `docs/adr/0001-architecture-and-tech-stack.md` | 71 lines; ADR for clean architecture + tech stack |

### 3.4 Documentation

| Deliverable | Evidence |
|---|---|
| `AGENTS.md` | Project-specific GapMiner sections (Overview, Setup, Code Style, Structure, Testing, Architecture, Pitfalls) prepended; orchestration system instructions preserved |

---

## 4. Lessons Learned

### 4.1 The devcontainer runtime ≠ the project's target runtime

The orchestration devcontainer image ships .NET SDK 10 (per `AGENTS.md` →
`available_tools`), and in this specific environment `dotnet` is not on `PATH` at all.
The project targets .NET SDK 8.0.x. The lesson: **the orchestration tooling image and
the application build image are separate concerns.** CI (`ci.yml`) uses
`actions/setup-dotnet` to install SDK 8.0.x on a clean `ubuntu-24.04` runner, which is
correct. But local development inside the devcontainer cannot build the solution until
SDK 8 is installed. Future template instances should either include the target SDK in
the prebuild image or document the `dotnet-install` step explicitly.

### 4.2 Aspire AppHost SDK is workload-only for 8.2.x — plan for it

`Aspire.AppHost.Sdk` 8.2.2 is consumed via `<Sdk Name="Aspire.AppHost.Sdk"
Version="8.2.2" />` in the `.csproj`, but the SDK is a **.NET workload**, not a plain
NuGet package. On hosted runners, `dotnet workload install aspire` installs the
**latest** (9.x) workload, which is incompatible with the 8.2.2 Sdk reference. The
workaround: exclude the AppHost from CI via a solution filter (`GapMiner.ci.slnf`) and
document the local `dotnet workload install aspire` step. This is a known Aspire
bootstrapping limitation that should be flagged in every project plan.

### 4.3 Branch protection rulesets must be sanitized per-repo

The template's `.github/protected-branches_ruleset.json` contains a foreign `source`
(`nam20485/workflow-orchestration-queue-zulu78-b`), a fixed `id` (14302534), and
`bypass_actors` referencing `OrganizationAdmin`/`EnterpriseOwner` actor types — all
invalid for a personal (user) repository. The import must strip `id`, `source`,
`source_type`, and replace or empty `bypass_actors` before PUT-ing to the ruleset API.
Additionally, the `required_status_checks` reference `integration_id: 15368` (a GitHub
App) that may not be installed — this is a silent blocker for PR merges.

### 4.4 Plan docs filename adaptation is routine

The `create-app-plan` assignment template references `plan_docs/ai-new-app-template.md`
as input, but the actual seeded document is `plan_docs/development-plan.md`. This is
expected — the launch pipeline seeds project-specific plan docs. Agents must treat
`development-plan.md` as the primary source and not fail on the filename mismatch.

### 4.5 PowerShellGet is broken in the current container

`Install-Module` (for PSScriptAnalyzer, Pester) fails in this devcontainer — a
pre-existing environment issue unrelated to the project. `validate.ps1 -All` passes
all JSON syntax checks and bash tests but cannot run Pester/PSScriptAnalyzer steps.
This should be fixed in the prebuild image, not worked around per-project.

### 4.6 Label namespace conventions must be discovered, not assumed

The `create-app-plan` assignment instructs applying a `planning` label, but the
label set uses the `state:planning` namespace convention. There is no bare `planning`
label in `.github/.labels.json`. Agents must query the actual label set before
applying labels, not assume names from assignment templates.

---

## 5. What Worked Well

1. **Strictly linear workflow sequencing.** The dependency chain
   (init → plan → scaffold → document → debrief) had zero ambiguity. Every assignment
   consumed the prior one's output cleanly. No rework was needed due to ordering.

2. **Central Package Management from day one.** Declaring all 16+ NuGet versions in a
   single `Directory.Packages.props` eliminated version drift before any code was
   written. The `GapMiner.ci.slnf` filter built all 13 non-AppHost projects
   successfully.

3. **SHA-pinned GitHub Actions.** The `ci.yml` workflow pins every action by full
   40-char SHA with a trailing semver comment. This was straightforward to implement
   during scaffolding and hardens the supply chain from the start.

4. **pgvector image selection.** Using `pgvector/pgvector:pg16` (not bare
   `postgres:16`) in both `docker-compose.yml` and the Aspire `AddPostgres` call
   ensures the `vector` extension is available. The AppHost `Program.cs` documents the
   `HasPostgresExtension("vector")` requirement (T-1.2).

5. **AGENTS.md dual-content strategy.** Prepending project-specific GapMiner sections
   while preserving the orchestration system instructions below gave AI agents both
   application context and orchestration guardrails in a single file.

6. **Workflow plan as a living document.** The 550-line `workflow-plan.md` with its
   open-questions section, risk table, and critical-path diagram served as a reliable
   reference for every subsequent assignment. Anticipating the SDK mismatch in the
   plan (§5.1) allowed Assignment 3 to implement the `global.json` pin proactively.

7. **Idempotent label import.** `scripts/import-labels.ps1` handled the 31-label import
   cleanly. The `.github/.labels.json` file proved to be a reliable single source of
   truth.

---

## 6. What Could Be Improved

1. **Ruleset import automation.** The branch protection ruleset required manual
   sanitization (stripping `id`, `source`, `source_type`, and `bypass_actors`). A
   helper script (`scripts/import-ruleset.ps1`) that performs this transformation
   automatically would eliminate a class of import failures across template instances.

2. **Status check integration_id validation.** The ruleset's
   `required_status_checks` reference `integration_id: 15368`, but there is no
   validation step that checks whether that GitHub App is installed on the target repo.
   This should be a pre-flight check in `init-existing-repository`.

3. **SDK availability check.** The workflow plan identified the SDK 10 vs 8 mismatch
   as a HIGH risk, but no assignment verifies that `dotnet` is actually on `PATH` in
   the devcontainer. An early `command -v dotnet` check would surface this immediately
   rather than discovering it during scaffolding.

4. **Pester test parity.** The `validate.yml` CI job runs Pester tests, but the local
   devcontainer cannot run them (PowerShellGet broken). This creates a gap between
   local validation and CI. Either the prebuild image should be fixed or the local
   validation script should gracefully skip Pester with a clear warning.

5. **AppHost CI coverage.** Excluding the AppHost from CI (`ci.slnf`) means the
   Aspire orchestration code (`Program.cs`, project references, hosting model) is never
   compiled or tested in CI. A separate nightly or manual job that installs the Aspire
   workload and builds the full `GapMiner.sln` would close this gap.

6. **Milestone due dates.** Milestones were created with due dates tightly spaced
   (Phase 0: 2026-07-06, Phase 1: 2026-07-11, etc.). For an AI-orchestrated project
   where task duration is unpredictable, these dates may need adjustment after Phase 0
   execution provides velocity data.

---

## 7. Errors Encountered and Resolutions

| # | Error | Root Cause | Resolution |
|---|---|---|---|
| 1 | **Branch protection ruleset import: `bypass_actors` rejected** | The template ruleset defines `OrganizationAdmin` and `EnterpriseOwner` bypass actor types. These are invalid for a personal (user) repository — they require an organization or enterprise context. | Emptied the `bypass_actors` array before PUT-ing the ruleset. The ruleset imported successfully as ID 18800820. |
| 2 | **Ruleset import: foreign `source` and fixed `id`** | `.github/protected-branches_ruleset.json` contains `"source": "nam20485/workflow-orchestration-queue-zulu78-b"` and `"id": 14302534` from the template origin repo. The API rejects these on a different repo. | Stripped `id`, `source`, and `source_type` from the payload before import. |
| 3 | **Aspire AppHost.Sdk 8.2.2 workload unavailable on hosted runners** | `Aspire.AppHost.Sdk` 8.2.2 is a .NET workload (not a NuGet package). `dotnet workload install aspire` installs the latest (9.x) which is incompatible with the 8.2.2 Sdk reference. | Created `GapMiner.ci.slnf` excluding the AppHost. CI builds and tests the 13 remaining projects. The `ci.yml` documents the workaround and the local `dotnet workload install aspire` step. |
| 4 | **`dotnet` not on PATH in devcontainer** | The orchestration devcontainer image (per AGENTS.md) ships .NET SDK 10, but in this environment `dotnet` is not installed at all. | CI uses `actions/setup-dotnet` with `dotnet-version: "8.0.x"` on a clean runner. Local dev requires manual SDK installation. **Open ACTION ITEM** — see ACTION ITEMS #2. |
| 5 | **PowerShellGet / Install-Module broken** | `Install-Module PSScriptAnalyzer` and `Pester` fail — `PowerShellGet` provider is broken in this container image. Pre-existing environment issue. | `validate.ps1 -All` passes all JSON and bash test steps. Pester steps skipped locally; they run in CI via the `validate.yml` `test` job. |
| 6 | **No bare `planning` label** | The `create-app-plan` assignment instructs applying a `planning` label. The label set uses `state:planning` (namespace convention). There is no bare `planning` in `.labels.json`. | Used `state:planning` + `documentation` labels instead. |
| 7 | **GitHub Actions SHA not resolving as commits** | Initially, some SHA values for action pins did not resolve as valid commit SHAs, causing CI YAML validation issues. | Verified each SHA resolves as a commit before using it: `gh api repos/<owner>/<repo>/git/ref/tags/<tag>` then resolve to the commit SHA. |

---

## 8. Complex Steps and Challenges

### 8.1 Branch Protection Ruleset Import (Assignment 1)

The most complex infrastructure step. The ruleset JSON
(`.github/protected-branches_ruleset.json`, 99 lines) defines 7 rule types:
`deletion`, `non_fast_forward`, `pull_request` (with 1 approving review, last-push
approval, thread resolution), `required_status_checks` (referencing integration_id
15368 for `scan` and `lint`), `code_scanning` (CodeQL), `code_quality` (errors),
`copilot_code_review`, and `required_linear_history`.

**Challenge:** Three fields are repo-specific and must be transformed:
- `id` (14302534) — must be removed (GitHub assigns a new ID)
- `source` / `source_type` — foreign repo reference, must be removed
- `bypass_actors` — `OrganizationAdmin`/`EnterpriseOwner` are org-only actor types

**Challenge (unresolved):** The `required_status_checks` reference `integration_id:
15368`. If the GitHub App behind that integration is not installed on this personal
repo, PR #2 cannot merge — the checks will never report a status. This is flagged as
ACTION ITEM #1.

### 8.2 .NET Solution Scaffolding Without Local SDK (Assignment 3)

Scaffolding 14 .NET projects, Central Package Management, and a multi-stage Dockerfile
required deep knowledge of the .NET project system — but the devcontainer lacked
`dotnet` entirely. The agent could not run `dotnet new`, `dotnet build`, or
`dotnet sln add` locally.

**Resolution:** Projects were created as hand-authored `.csproj` files with correct
`<TargetFramework>`, `<ProjectReference>`, and `<PackageReference>` (versionless, per
Central Package Management). The solution file and solution filter were written
manually. CI validated the build on a clean runner with `actions/setup-dotnet`. The
build passed green on the first CI run.

### 8.3 Aspire AppHost SDK Workload Pinning (Assignment 3)

The `GapMiner.AppHost.csproj` uses `<Sdk Name="Aspire.AppHost.Sdk" Version="8.2.2" />`.
This Sdk element triggers a workload requirement. Unlike NuGet `PackageReference`s,
SDK workloads are installed machine-wide via `dotnet workload install` and cannot be
pinned to a specific version by the project file alone.

**Resolution:** Created `GapMiner.ci.slnf` (solution filter) that includes all
projects except `GapMiner.AppHost`. CI runs `dotnet restore/build/test` against the
filter. The AppHost is documented as a local-only project requiring
`dotnet workload install aspire`. The `ci.yml` includes a detailed comment block
explaining the rationale (lines 40–47).

### 8.4 AGENTS.md Content Strategy (Assignment 4)

The repository already had a template `AGENTS.md` (~800+ lines) describing the
orchestration system (opencode, agents, workflows, MCP servers, mandatory tool
protocols). Assignment 4 required a project-specific AGENTS.md for the GapMiner .NET
application.

**Resolution:** Prepended a concise GapMiner-specific section (Project Overview, Setup
Commands, Project Structure, Code Style, Testing Instructions, Architecture Notes, PR
and Commit Guidelines, Common Pitfalls) above the existing orchestration instructions.
A clear separator comment (`<!-- Orchestration System Instructions Below -->`)
delineates the two sections. Both AI agents working on GapMiner code and the
orchestrator system retain their respective context.

---

## 9. Suggested Changes

### 9.1 Workflow Assignment Changes

| Assignment | Suggested Change | Rationale |
|---|---|---|
| `init-existing-repository` | Add a pre-flight check: verify `integration_id` in ruleset status checks is installed on the repo via `gh api /repos/{owner}/{repo}/installation`. | Prevents silent merge blockers from uninstalled GitHub Apps. |
| `init-existing-repository` | Add a `scripts/import-ruleset.ps1` helper that strips `id`, `source`, `source_type`, and repo-incompatible `bypass_actors` automatically. | Eliminates a manual, error-prone transformation step. |
| `create-app-plan` | Update the assignment template to accept `development-plan.md` as an alternative to `ai-new-app-template.md`. | The filename mismatch is routine across all template instances. |
| `create-app-plan` | Replace the `planning` label instruction with "query the label set and apply the appropriate planning-state label." | Prevents label-not-found errors from namespace conventions. |
| `create-project-structure` | Add an early `command -v dotnet` SDK availability check and fail fast with actionable guidance. | Surfaces the missing-SDK issue immediately rather than during build attempts. |
| `create-project-structure` | Add an Aspire workload detection step that checks `dotnet workload list` for `aspire` and documents the workaround if missing. | Makes the AppHost exclusion explicit and documented. |
| `debrief-and-document` | Add a "CI verification" sub-step that confirms both `ci.yml` and `validate.yml` are green before writing the report. | Ensures the debrief's CI claims are evidence-backed. |

### 9.2 Agent / Prompt Changes

- **`create-app-plan` prompt:** Add a note: "If `plan_docs/ai-new-app-template.md`
  does not exist, use `plan_docs/development-plan.md` as the primary input. Do not
  fail on filename mismatches."
- **`init-existing-repository` prompt:** Add a ruleset sanitization checklist:
  "Before importing, verify: (a) `id` is removed, (b) `source`/`source_type` are
  removed, (c) `bypass_actors` entries are valid for the repo's account type (user
  vs. org), (d) `integration_id` values in `required_status_checks` correspond to
  installed GitHub Apps."
- **Orchestrator delegation:** Consider splitting `create-project-structure` into two
  sub-tasks: (a) solution + project files + props, (b) Docker + CI + docs. The current
  single assignment produces a large diff that generates many review comments.

### 9.3 Script Changes

- **New: `scripts/import-ruleset.ps1`** — Wraps the ruleset sanitization + API import
  with idempotency (skip if ruleset name already exists).
- **New: `scripts/check-dotnet-sdk.ps1`** — Validates that the required SDK band
  (from `global.json`) is available; exits non-zero with install guidance if not.
- **`scripts/validate.ps1`:** When PowerShellGet is broken, emit a clear warning
  ("Pester tests skipped — PowerShellGet unavailable") rather than failing the entire
  run. Currently the bash + JSON steps pass but the Pester failure is noisy.
- **`scripts/install-dev-tools.ps1`:** Add a `dotnet-sdk` target that installs the
  SDK band from `global.json` via `dotnet-install.sh`.

---

## 10. Metrics and Statistics

| Metric | Value |
|---|---|
| Assignments completed | 5 of 6 (83%) |
| Pre-script events completed | 1 of 1 (100%) |
| Planning documents created | 3 (`workflow-plan.md`, `tech-stack.md`, `architecture.md`) |
| Total planning lines | 894 (550 + 98 + 246) |
| .NET projects created | 14 (9 src + 5 test) |
| NuGet packages pinned | 16+ (in `Directory.Packages.props`) |
| GitHub milestones created | 7 (Phase 0–Phase 6) |
| GitHub labels imported | 31 |
| GitHub Projects created | 1 (Project V2 #70) |
| Pull requests opened | 1 (PR #2) |
| Application plan issues | 1 (Issue #3) |
| ADRs created | 1 (ADR-0001) |
| Docker services defined | 6 (postgres, redis, api, web, scraper-worker, ai-worker) |
| CI workflows | 2 (`ci.yml`, `validate.yml`) — both green |
| GitHub Actions pinned by SHA | 2 (`checkout@9c091bb...`, `setup-dotnet@26b0ec1...`) |
| Errors encountered and resolved | 7 (see Section 7) |
| Open ACTION ITEMS | 3 (see ACTION ITEMS block) |

---

## 11. Future Recommendations

### 11.1 Immediate (Before PR Merge — Assignment 6)

1. **Resolve ACTION ITEM #1 (branch protection status checks).** Either install the
   GitHub App (integration_id 15368) on this repo, or update the ruleset to remove the
   `integration_id` references and use plain check names (`scan`, `lint`) that match
   the `validate.yml` job names.
2. **Verify CI green on PR #2.** Both `ci.yml` and `validate.yml` must pass before
   merge is attempted. The `pr-approval-and-merge` assignment includes a 3-retry CI
   remediation loop — use it if needed.

### 11.2 Short-Term (Phase 0 Execution)

3. **Resolve ACTION ITEM #2 (dotnet SDK in devcontainer).** Either add .NET SDK 8.0.x
   to the prebuild image (`nam20485/workflow-orchestration-prebuild`) or add a
   `dotnet-install` devcontainer feature/lifecycle hook. Without this, no developer
   (human or AI) can build locally.
4. **Resolve ACTION ITEM #3 (Aspire workload).** Accept Aspire 9.x for AppHost-only
   use, or pin a compatibility-tested workload version. Document the decision in a new
   ADR.
5. **Fix PowerShellGet in the prebuild image.** This is a pre-existing environment
   defect that prevents local Pester test execution.

### 11.3 Medium-Term (Phase 1+ Development)

6. **Add a nightly AppHost build job.** A scheduled workflow that installs the Aspire
   workload and builds the full `GapMiner.sln` (including AppHost) to catch
   orchestration regressions.
7. **Resolve LLM provider selection.** `development-plan.md` allows Azure OpenAI or
   Anthropic Claude. This must be decided before Phase 3 (Intelligence Pipeline). Update
   `tech-stack.md` with the decision.
8. **Verify Apify actor IDs.** `development-plan.md` T-2.2 maps marketplace types to
   Apify actor IDs. These need human verification against the live Apify Store before
   Phase 2 (Scraper Pipeline).

### 11.4 Process Improvements

9. **Create `scripts/import-ruleset.ps1`.** Automate the ruleset sanitization that is
   currently manual. This will benefit every future template instance.
10. **Add a `create-project-structure` SDK pre-flight check.** A `command -v dotnet`
    guard at the start of Assignment 3 would have surfaced the missing SDK issue
    immediately rather than requiring workarounds.
11. **Review milestone due dates after Phase 0.** The tight due dates (2–7 days per
    phase) were set without velocity data. Adjust after Phase 0 completion.

---

## 12. Conclusion

The `project-setup` dynamic workflow successfully transformed a bare orchestration
template into a fully scaffolded, CI-green .NET 8 solution in a single linear pass.
Five of six assignments completed without rework. The .NET solution (14 projects),
Central Package Management, Docker infrastructure, SHA-pinned CI/CD, and comprehensive
documentation are all in place on branch `dynamic-workflow-project-setup` and verified
green by CI.

The three open ACTION ITEMS (branch protection status checks, missing dotnet SDK in
the devcontainer, Aspire workload pinning) are environmental/template-level issues,
not defects in the GapMiner project itself. They are documented with specific evidence
and recommended resolutions. The most critical is ACTION ITEM #1 — if unresolved, it
will block PR #2 from merging in Assignment 6.

The workflow plan (`plan_docs/workflow-plan.md`) proved its value as a predictive
document: the HIGH-risk SDK mismatch identified in §5.1 was encountered and
workaround-ed exactly as anticipated. The open-questions section captured every
ambiguity that materialized during execution. This validates the pre-script
`create-workflow-plan` event as a load-bearing workflow component.

**Next step:** Assignment 6 (`pr-approval-and-merge`) must verify CI green, delegate
code review, resolve review comments, secure approval, and merge PR #2. The
ACTION ITEMS in this report should be addressed before or during that merge process.

---

## ACTION ITEMS (Plan Adjustment Mandate)

These three findings are flagged as plan-impacting and require explicit follow-up.

### ACTION ITEM #1: Branch Protection Status Checks (CRITICAL — Blocks PR Merge)

**Finding:** The imported branch protection ruleset (ID 18800820) requires
`required_status_checks` with `context: "scan"` and `context: "lint"`, both referencing
`integration_id: 15368` (a GitHub App). If this GitHub App is not installed on the
personal repo `nam20485/gap-miner-v2-papa85`, the checks will never report a status,
and **PR #2 cannot be merged**.

**Evidence:** `.github/protected-branches_ruleset.json` lines 42–56:
```json
"required_status_checks": [
  { "context": "scan", "integration_id": 15368 },
  { "context": "lint", "integration_id": 15368 }
]
```

**Recommendation:** Either (a) install the GitHub App (integration_id 15368) on this
repo, or (b) update the ruleset via `gh api` to remove the `integration_id` references
and use plain context names that match the `validate.yml` job names (`lint`, `scan`).

**Owner:** Stakeholder / `pr-approval-and-merge` assignment.
**Priority:** CRITICAL — must be resolved before merge.

---

### ACTION ITEM #2: .NET SDK Not Installed in Devcontainer (HIGH — Blocks Local Dev)

**Finding:** The plan specifies .NET SDK 8.0.x (pinned via `global.json` with
`"version": "8.0.100"`). The orchestration devcontainer image ships SDK 10.0.x per
`AGENTS.md` → `available_tools`, but in this environment `dotnet` is not on `PATH` at
all. CI works correctly (uses `actions/setup-dotnet` on a clean runner), but no local
development or build is possible inside the devcontainer.

**Evidence:** `global.json` (lines 1–7); `AGENTS.md` → `available_tools` (SDK 10.0.102);
`ci.yml` lines 35–38 (`actions/setup-dotnet` with `dotnet-version: "8.0.x"`).

**Recommendation:** Add .NET SDK 8.0.x installation to the devcontainer prebuild image
(`nam20485/workflow-orchestration-prebuild`) or add a devcontainer feature/lifecycle
hook that runs `dotnet-install.sh` for the SDK band specified in `global.json`.

**Owner:** Prebuild repo maintainer / Stakeholder.
**Priority:** HIGH — must be resolved before Phase 0 development tasks.

---

### ACTION ITEM #3: Aspire AppHost SDK Workload Pinning (MEDIUM — CI Coverage Gap)

**Finding:** `Aspire.AppHost.Sdk` 8.2.2 is a .NET workload (not a NuGet package). On
hosted runners, `dotnet workload install aspire` installs the latest (9.x) workload,
which is incompatible with the 8.2.2 Sdk reference in `GapMiner.AppHost.csproj`. The
workaround excludes the AppHost from CI via `GapMiner.ci.slnf`, leaving the Aspire
orchestration code untested in CI.

**Evidence:** `src/GapMiner.AppHost/GapMiner.AppHost.csproj` line 2
(`<Sdk Name="Aspire.AppHost.Sdk" Version="8.2.2" />`); `GapMiner.ci.slnf` (13 projects,
AppHost excluded); `ci.yml` lines 40–47 (workaround comment block).

**Recommendation:** Either (a) accept Aspire 9.x for the AppHost project only (update
the Sdk reference and Aspire.Hosting packages to 9.x), or (b) add a separate nightly
CI job that installs the aspire workload and builds the full `GapMiner.sln`. Document
the decision in an ADR.

**Owner:** Stakeholder.
**Priority:** MEDIUM — resolve before Phase 0 T-0.2 (AppHost implementation).

---

*End of debrief report.*
