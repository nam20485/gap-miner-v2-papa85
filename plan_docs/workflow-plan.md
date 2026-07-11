# Workflow Execution Plan: `project-setup`

**Repository:** `nam20485/gap-miner-v2-papa85`
**Dynamic Workflow:** `project-setup`
**Working Branch:** `dynamic-workflow-project-setup`
**Date:** 2026-07-11
**Status:** Draft — Pending Approval

---

## 1. Overview

This document is the executable workflow plan for the **`project-setup`** dynamic workflow. It governs the end-to-end initialization of the **Gap Mining Platform** repository — from branch creation and label import through project scaffolding, AGENTS.md creation, and PR merge.

### Project Description

The **Gap Mining Platform** is an internal intelligence engine that scrapes 1-to-3-star competitor app reviews from digital marketplaces (Shopify App Store, Chrome Web Store, G2, Apple App Store), clusters them using LLMs via pgvector semantic similarity, and surfaces monetizable feature gap opportunities via a Blazor dashboard. It does **not** build or ship micro-SaaS applications — it identifies actionable opportunities for downstream teams.

### Workflow Summary

| Property | Value |
|---|---|
| Workflow name | `project-setup` |
| Total assignments | 6 (ordered) |
| Pre-script event | `create-workflow-plan` (this document) |
| Post-assignment events | `validate-assignment-completion` + `report-progress` (after each assignment) |
| Post-script event | Apply `orchestration:plan-approved` label to app plan issue |
| Working branch | `dynamic-workflow-project-setup` |
| Output PR | Created by assignment 1, merged by assignment 6 |

### Assignment Sequence

| # | Assignment | Type | Key Output |
|---|---|---|---|
| 1 | `init-existing-repository` | Infrastructure | Branch, branch protection, GitHub Project, labels, workspace renames, PR |
| 2 | `create-app-plan` | Planning | App plan issue, milestones, tech-stack.md, architecture.md |
| 3 | `create-project-structure` | Scaffolding | .NET solution, all projects, Docker, CI/CD, docs, README |
| 4 | `create-agents-md-file` | Documentation | AGENTS.md at repo root |
| 5 | `debrief-and-document` | Retrospective | Debrief report + execution trace |
| 6 | `pr-approval-and-merge` | Merge | CI green, code review, comment resolution, merge, branch cleanup |

---

## 2. Project Context Summary

### 2.1 Source Documents

| Document | Location | Purpose |
|---|---|---|
| `development-plan.md` | `plan_docs/` | Autonomous agent development plan (18 sections, 6 phases, exact tech stack with pinned versions, task-level acceptance criteria) |
| `Strategic Feasibility and Execution Plan for AI-Accelerated Micro-SaaS Ecosystems.md` | `plan_docs/` | Strategic context: review mining protocol, marketplace analysis, AI-accelerated DevOps, compliance architecture |

### 2.2 Technology Stack (Exact Versions — from development-plan.md §3)

| Component | Technology | Version |
|---|---|---|
| Runtime | .NET SDK | **8.0.x (LTS)** |
| Orchestration | .NET Aspire | 8.2.x |
| Web UI | Blazor Web App (Interactive Server) | 8.0 |
| API | ASP.NET Core Minimal API | 8.0 |
| Database | PostgreSQL | 16.x |
| Vector Extension | pgvector | 0.7.x |
| ORM | Entity Framework Core | 8.0.x |
| Queue / Cache | Redis (StackExchange.Redis) | 7.x |
| Job Scheduling | Hangfire | 1.8.x |
| AI Orchestration | Microsoft.SemanticKernel | 1.20.x |
| LLM Provider | Azure OpenAI (GPT-4o) or Anthropic Claude 3.5 Sonnet | Latest stable |
| Embeddings | text-embedding-3-large or voyage-3 | Latest stable |
| Scraping | Apify API | v2 REST |
| HTTP Client | Refit | 7.x |
| Validation | FluentValidation | 11.x |
| Testing | xUnit + NSubstitute + Testcontainers | Latest stable |
| Code Quality | SonarAnalyzer.CSharp + StyleCop.Analyzers | Latest stable |

### 2.3 Target Repository Layout (from development-plan.md §4)

The project uses a clean-architecture layout with 8 source projects and 5 test projects under the `GapMiner.sln` solution:

```
GapMiner/
├── GapMiner.sln
├── Directory.Build.props
├── Directory.Packages.props
├── global.json
├── src/
│   ├── GapMiner.AppHost/              # Aspire orchestrator
│   ├── GapMiner.ServiceDefaults/      # Shared Aspire service defaults
│   ├── GapMiner.Domain/               # Pure domain entities
│   ├── GapMiner.Infrastructure/       # EF Core, Redis, Apify, Semantic Kernel
│   ├── GapMiner.Application/          # Use cases / command handlers
│   ├── GapMiner.Api/                  # Minimal API gateway
│   ├── GapMiner.ScraperWorker/        # Background worker: scraping
│   ├── GapMiner.AIWorker/             # Background worker: LLM analysis
│   └── GapMiner.Web/                  # Blazor dashboard
├── tests/
│   ├── GapMiner.Domain.Tests/
│   ├── GapMiner.Infrastructure.Tests/
│   ├── GapMiner.Application.Tests/
│   ├── GapMiner.Api.Tests/
│   └── GapMiner.Integration.Tests/
└── docs/
```

### 2.4 Key Constraints

- **No deviations** from the task boundaries or tech stack versions in development-plan.md without explicit human approval (R1, R2).
- Every public method must have XML doc comments and at least one unit test (R3).
- All async I/O must use `CancellationToken` propagation (R4).
- Never hardcode secrets — use `IConfiguration` or Aspire secret parameters (R5).
- Conventional Commits format: `feat(scope): description` (R7).
- All GitHub Actions pinned by full 40-char SHA (repo coding convention).

### 2.5 Key Risks

| Risk | Severity | Description |
|---|---|---|
| **.NET SDK version mismatch** | **HIGH** | The devcontainer image has **.NET SDK 10** installed, but `development-plan.md` §3 specifies **.NET SDK 8.0.x (LTS)**. This must be resolved via `global.json` pinning with `rollForward: latestFeature`, or stakeholder decision to upgrade the plan to .NET 10. Aspire 8.2.x targets .NET 8 — using SDK 10 may cause package incompatibilities. |
| plan_docs filename mismatch | MEDIUM | `create-app-plan` expects `plan_docs/ai-new-app-template.md`, but the actual file is `plan_docs/development-plan.md`. Agent must adapt to the actual filenames present. |
| Apify actor IDs unverified | MEDIUM | development-plan.md T-2.2 requires human verification of Apify actor IDs against the live Apify Store. Agents must not guess. |
| LLM provider undecided | LOW | development-plan.md allows either Azure OpenAI or Anthropic Claude. Must be resolved before Phase 3 (Intelligence Pipeline). |
| pgvector in container | LOW | Aspire Postgres image must include pgvector extension (`pgvector/pgvector:pg16`). Development environment must verify this. |

---

## 3. Assignment Execution Plan

### Event: `pre-script-begin` → `create-workflow-plan`

| Property | Detail |
|---|---|
| **Goal** | Create this workflow execution plan document and commit it to the repository |
| **Trigger** | Fires before the script begins, before any assignments run |
| **Acceptance Criteria** | This document (`plan_docs/workflow-plan.md`) is created, committed, and pushed to the working branch |
| **Dependencies** | None (first action in the workflow) |
| **Risks** | None significant — this is a documentation-only task |

---

### Assignment 1: `init-existing-repository`

| Property | Detail |
|---|---|
| **Goal** | Initialize the repository with branch, branch protection, GitHub Project, labels, workspace file renames, and create the setup PR |
| **Prerequisites** | GitHub authentication with scopes: `repo`, `project`, `read:project`, `read:user`, `user:email`, `administration: write`. `GH_ORCHESTRATION_AGENT_TOKEN` available. |
| **Dependencies** | None — this is the first assignment |

**Key Acceptance Criteria:**
1. New branch `dynamic-workflow-project-setup` created (must be first)
2. Branch protection ruleset imported from `.github/protected-branches_ruleset.json` (idempotent — skip if exists)
3. GitHub Project created with Board template, linked to repository, columns: Not Started, In Progress, In Review, Done
4. Labels imported from `.github/.labels.json` via `scripts/import-labels.ps1`
5. `.devcontainer/devcontainer.json` `name` property renamed to `gap-miner-v2-papa85-devcontainer`
6. Workspace file renamed to `gap-miner-v2-papa85.code-workspace`
7. PR created from `dynamic-workflow-project-setup` → `main`

**Project-Specific Notes:**
- Branch name: `dynamic-workflow-project-setup` (derived from the dynamic workflow name `project-setup`)
- The branch protection ruleset import requires `administration: write` scope — use `GH_ORCHESTRATION_AGENT_TOKEN` (not `GITHUB_TOKEN`)
- Verify permissions first with `./scripts/test-github-permissions.ps1 -Owner nam20485`
- The PR created here stays open until assignment 6 (`pr-approval-and-merge`) — all subsequent assignments commit to this same branch and PR
- The `$pr_num` output from this assignment is consumed by assignment 6

**Risks/Challenges:**
- Branch protection ruleset import may fail if `administration: write` scope is missing — must report exact error and stop
- PR creation may fail with "No commits between main and branch" — ensure at least one commit (from steps 4-5) is pushed first
- GitHub Project creation API may require `project` scope which differs from classic Projects API

**Events:**
- `post-assignment-complete`: → `validate-assignment-completion` → `report-progress`

---

### Assignment 2: `create-app-plan`

| Property | Detail |
|---|---|
| **Goal** | Create a comprehensive application plan as a GitHub Issue, create milestones, and produce planning documents — strictly **PLANNING ONLY**, no code |
| **Prerequisites** | Assignment 1 complete (branch, PR, GitHub Project exist) |
| **Dependencies** | Assignment 1 (needs the GitHub Project for issue linking, milestones via `gh api`) |

**Key Acceptance Criteria:**
1. Application template thoroughly analyzed (development-plan.md + Strategic Feasibility doc)
2. Plan documented using the issue template from `.github/ISSUE_TEMPLATE/application-plan.md`
3. `plan_docs/tech-stack.md` created documenting languages, frameworks, tools, packages
4. `plan_docs/architecture.md` created documenting high-level architecture, components, design decisions
5. Milestones created based on the 6 phases from development-plan.md
6. Plan issue linked to GitHub Project, assigned to "Phase 1: Foundation" milestone
7. Labels applied: `planning`, `documentation`
8. Plan is ready for development and implementation
9. Stakeholder/Orchestrator approval obtained

**Project-Specific Notes:**
- **IMPORTANT:** The `create-app-plan` assignment references `plan_docs/ai-new-app-template.md` as input, but the actual plan_docs contain `development-plan.md` and `Strategic Feasibility...md`. The agent must treat `development-plan.md` as the primary source document (it already contains a comprehensive plan).
- development-plan.md already defines 6 phases with tasks (T-0.1 through T-6.3) — milestones should map to these phases:
  - Milestone 1: "Phase 0: Environment & Foundation" (T-0.1 to T-0.3)
  - Milestone 2: "Phase 1: Domain & Data Layer" (T-1.1 to T-1.4)
  - Milestone 3: "Phase 2: Scraper Pipeline" (T-2.1 to T-2.3)
  - Milestone 4: "Phase 3: Intelligence Pipeline" (T-3.1 to T-3.4)
  - Milestone 5: "Phase 4: API Gateway" (T-4.1)
  - Milestone 6: "Phase 5: Blazor Dashboard" (T-5.1 to T-5.4)
  - Milestone 7: "Phase 6: Integration Testing & Hardening" (T-6.1 to T-6.3)
- The `tech-stack.md` should mirror §3 of development-plan.md
- The `architecture.md` should describe the clean architecture pattern, Aspire orchestration, the map-reduce intelligence pipeline, and the data flow
- **DO NOT apply `orchestration:plan-approved` label** — that is applied by the post-script-complete event

**Risks/Challenges:**
- Input filename mismatch (`ai-new-app-template.md` expected vs `development-plan.md` actual)
- The issue template at `.github/ISSUE_TEMPLATE/application-plan.md` must exist — verify it does or adapt

**Events:**
- `pre-assignment-begin`: → `gather-context`
- `post-assignment-complete`: → `validate-assignment-completion` → `report-progress`
- `on-assignment-failure`: → `recover-from-error`

---

### Assignment 3: `create-project-structure`

| Property | Detail |
|---|---|
| **Goal** | Create the actual .NET solution structure, all 8 source projects, test projects, Docker, CI/CD, and documentation — the real scaffolding |
| **Prerequisites** | Assignment 2 complete (approved app plan exists as issue and/or APP_PLAN.md) |
| **Dependencies** | Assignment 2 (needs the approved plan to determine structure) |

**Key Acceptance Criteria:**
1. `GapMiner.sln` solution created with `src` and `tests` solution folders
2. `global.json` pins SDK `8.0.x` with `rollForward: latestFeature`
3. `Directory.Packages.props` enables Central Package Management with ALL versions from development-plan.md §3
4. `Directory.Build.props` enables `TreatWarningsAsErrors`, nullable reference types, implicit usings
5. `.editorconfig` enforces file-scoped namespaces, 4-space indentation, CRLF
6. All 8 source projects created with correct project references
7. All 5 test projects created
8. Dockerfile + docker-compose.yml for local development
9. CI/CD workflow foundation with SHA-pinned actions
10. README.md + docs/ structure
11. `.ai-repository-summary.md` created and linked from README
12. `dotnet build` succeeds with exit code 0
13. All GitHub Actions pinned to full 40-char SHA

**Project-Specific Notes:**
- **CRITICAL — SDK Version:** The devcontainer has .NET SDK 10. `global.json` must pin to 8.0.x. If SDK 8 is not available in the container, `rollForward: latestFeature` will allow SDK 10 to build .NET 8 targeting projects, but this must be validated. See Open Question #1.
- The AppHost project should reference Aspire Hosting 8.2.2, Aspire.Hosting.PostgreSQL 8.2.2, Aspire.Hosting.Redis 8.2.2
- The AppHost Program.cs must declare: PostgreSQL (with pgvector), Redis, and project references for Api, Web, ScraperWorker, AIWorker
- docker-compose healthchecks must NOT use `curl` — use Python stdlib or other available tools
- Solution file: development-plan.md references `GapMiner.sln` but the `create-project-structure` assignment example shows `.slnx` format for .NET. Use standard `.sln` as specified in the plan.
- Project naming follows development-plan.md §5: namespace `GapMiner.{Layer}.{Feature}`, kebab-case API routes, snake_case DB columns

**Risks/Challenges:**
- **.NET SDK 10 vs 8.0.x mismatch** — global.json pinning should resolve this, but package compatibility (especially Aspire 8.2.x) must be verified
- pgvector PostgreSQL container image must be `pgvector/pgvector:pg16` not standard `postgres:16`
- Central Package Management requires all versions declared in Directory.Packages.props — any missing version will break the build
- The `.ai-repository-summary.md` requires following the `create-repository-summary.md` instructions from the canonical repo

**Events:**
- `post-assignment-complete`: → `validate-assignment-completion` → `report-progress`

---

### Assignment 4: `create-agents-md-file`

| Property | Detail |
|---|---|
| **Goal** | Create a comprehensive `AGENTS.md` at the repository root providing AI coding agents with project context, build/test commands, conventions, and structure |
| **Prerequisites** | Assignments 1-3 complete (repo initialized, app plan exists, project structure created) |
| **Dependencies** | Assignment 3 (needs the actual project structure and validated build/test commands) |

**Key Acceptance Criteria:**
1. `AGENTS.md` exists at repository root
2. Contains project overview (purpose, tech stack)
3. Contains setup/build/test commands — **all verified to work**
4. Contains code style and conventions section
5. Contains project structure / directory layout section
6. Contains testing instructions
7. Contains PR/commit guidelines
8. Written in standard Markdown with clear, agent-focused language
9. Commands listed have been validated by running them
10. File committed and pushed to working branch
11. Stakeholder approval obtained

**Project-Specific Notes:**
- The build/test commands should reference the .NET CLI: `dotnet build`, `dotnet test`, `dotnet run --project src/GapMiner.AppHost`
- Code style section should reference `.editorconfig`, `Directory.Build.props` settings, and the naming conventions from development-plan.md §5
- Architecture notes should describe: clean architecture layers, Aspire orchestration, map-reduce intelligence pipeline, Redis job queues, Hangfire scheduling
- PR guidelines should reference Conventional Commits (`feat(scope): description`), the Definition of Done from development-plan.md §15
- **NOTE:** This project already has a template `AGENTS.md` (from the orchestrator-service template). The new AGENTS.md should be project-specific for the Gap Mining Platform, replacing the template content. The existing AGENTS.md describes the orchestration system — the new one should describe the GapMiner .NET application.

**Risks/Challenges:**
- Must not duplicate README.md content — complement it instead
- Must validate all commands actually work before documenting them
- The existing template AGENTS.md is very long and orchestration-focused — the replacement must be concise and project-focused

**Events:**
- `post-assignment-complete`: → `validate-assignment-completion` → `report-progress`

---

### Assignment 5: `debrief-and-document`

| Property | Detail |
|---|---|
| **Goal** | Produce a comprehensive debriefing report capturing learnings, insights, deviations, and improvement recommendations from the project-setup workflow |
| **Prerequisites** | Assignments 1-4 complete |
| **Dependencies** | Assignments 1-4 (needs all prior work to debrief on) |

**Key Acceptance Criteria:**
1. Detailed report created following the 12-section structured template
2. Report saved as `.md` file in the repository
3. Execution trace saved as `debrief-and-document/trace.md`
4. All deviations from assignments documented
5. Report reviewed and approved by stakeholders
6. Report committed and pushed
7. Plan-impacting findings flagged as ACTION ITEMS with recommended follow-up (file issue or update plan)

**Project-Specific Notes:**
- The debrief should specifically address: the .NET SDK version mismatch resolution, any Apify actor ID verification blockers, the plan_docs filename adaptation, and the template AGENTS.md replacement
- ACTION ITEMS should address any changes needed to development-plan.md phases based on discoveries during scaffolding
- The execution trace should capture all `gh` API calls, `dotnet` commands, and file operations performed across assignments 1-4

**Risks/Challenges:**
- If any assignment had partial completion or deviations, these must be honestly documented
- The debrief must review upcoming development-plan.md phases for continued validity

**Events:**
- `post-assignment-complete`: → `validate-assignment-completion` → `report-progress`

---

### Assignment 6: `pr-approval-and-merge`

| Property | Detail |
|---|---|
| **Goal** | Complete the full PR lifecycle: CI verification, code review, comment resolution, stakeholder approval, merge, and post-merge cleanup |
| **Prerequisites** | Assignments 1-5 complete, `$pr_num` available from assignment 1 |
| **Dependencies** | Assignment 1 (provides `$pr_num`), all other assignments (commits pushed to the PR branch) |

**Key Acceptance Criteria:**
1. All required CI/CD status checks pass (CI remediation loop up to 3 attempts)
2. Code review delegated to `code-reviewer` subagent (NOT self-review)
3. Auto-reviewer comments (Copilot, CodeQL, etc.) waited for and resolved
4. All review threads resolved via GraphQL `resolveReviewThread` mutation
5. `pr-unresolved-threads.json` verified empty
6. PR-wide summary comment posted enumerating all threads and outcomes
7. Stakeholder approval obtained
8. Merge executed successfully
9. Source branch deleted (if policy allows)
10. Related issues closed or updated
11. `result` output set to `"merged"`, `"pending"`, or `"failed"`

**Project-Specific Notes:**
- `$pr_num` comes from assignment 1's PR creation
- The PR contains ALL commits from assignments 1-5 (branch protection, labels, workspace renames, plan issue artifacts, project structure, AGENTS.md, debrief report)
- CI checks from `.github/workflows/validate.yml` must pass: lint (actionlint, gitleaks, markdownlint), scan (gitleaks), test (bash + Pester)
- **CRITICAL:** All local changes must be committed and pushed BEFORE merge — uncommitted work is lost when the branch is deleted
- The `ai-pr-comment-protocol.md` must be read and acknowledged before beginning
- Repository preferred merge strategy should be confirmed (squash recommended for clean history)

**Risks/Challenges:**
- CI may fail due to markdownlint on new documentation files — ensure plan_docs are excluded from linting per template constraints
- gitleaks may flag synthetic test values — ensure all test secrets use `FAKE-KEY-FOR-TESTING-` prefix
- Branch protection ruleset (imported in assignment 1) may require specific review approvals before merge
- Large PR (all scaffolding + docs) may generate many review comments — allocate time for thorough resolution

**Events:**
- `post-assignment-complete`: → `validate-assignment-completion` → `report-progress`

---

### Post-Script Event: Apply `orchestration:plan-approved` Label

| Property | Detail |
|---|---|
| **Goal** | Signal that the project-setup workflow is complete and the application plan is approved |
| **Trigger** | Fires after all 6 assignments complete successfully |
| **Action** | Apply `orchestration:plan-approved` label to the app plan issue (created in assignment 2) |
| **Dependencies** | All 6 assignments complete |

---

## 4. Sequencing

### 4.1 Linear Dependency Flow

```
pre-script-begin: create-workflow-plan (THIS TASK)
    │
    ▼
┌─────────────────────────────────────────────────────────────────┐
│ Assignment 1: init-existing-repository                          │
│   ├── Create branch: dynamic-workflow-project-setup             │
│   ├── Import branch protection ruleset                          │
│   ├── Create GitHub Project (Board)                             │
│   ├── Import labels                                             │
│   ├── Rename workspace files                                    │
│   └── Create PR ($pr_num output)                                │
│   EVENTS: validate-assignment-completion → report-progress      │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ Assignment 2: create-app-plan                                   │
│   ├── Analyze development-plan.md + Strategic Feasibility doc   │
│   ├── Create tech-stack.md, architecture.md                     │
│   ├── Create app plan issue (from template)                     │
│   ├── Create milestones (7 phases)                              │
│   └── Link issue to project + milestone                         │
│   EVENTS: gather-context (pre) → validate → report-progress     │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ Assignment 3: create-project-structure                          │
│   ├── Create GapMiner.sln + global.json + props files           │
│   ├── Create 8 source projects + 5 test projects                │
│   ├── Create Docker + docker-compose                            │
│   ├── Create CI/CD workflow foundation                          │
│   ├── Create README + docs structure                            │
│   ├── Create .ai-repository-summary.md                          │
│   └── Verify: dotnet build succeeds                             │
│   EVENTS: validate-assignment-completion → report-progress      │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ Assignment 4: create-agents-md-file                             │
│   ├── Gather project context (README, structure, plan)          │
│   ├── Validate build/test commands                              │
│   ├── Draft AGENTS.md (project-specific for GapMiner)           │
│   └── Commit and push                                           │
│   EVENTS: validate-assignment-completion → report-progress      │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ Assignment 5: debrief-and-document                              │
│   ├── Create 12-section debrief report                          │
│   ├── Create execution trace (debrief-and-document/trace.md)    │
│   ├── Document all deviations and ACTION ITEMS                  │
│   └── Commit and push                                           │
│   EVENTS: validate-assignment-completion → report-progress      │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ Assignment 6: pr-approval-and-merge                             │
│   ├── Phase 0: Pre-flight (PR number, auth, snapshot)           │
│   ├── Phase 0.5: CI verification & remediation (max 3 retries)  │
│   ├── Phase 0.75: Code review delegation + auto-reviewer wait   │
│   ├── Phase 1: Resolve review comments (pr-review-comments)     │
│   ├── Phase 2: Secure approval                                  │
│   └── Phase 3: Merge + branch cleanup                           │
│   EVENTS: validate-assignment-completion → report-progress      │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
post-script-complete: Apply orchestration:plan-approved label
```

### 4.2 Critical Path

The critical path is **strictly linear** — every assignment depends on the prior one's output:

```
create-workflow-plan → init-existing-repository → create-app-plan
  → create-project-structure → create-agents-md-file
  → debrief-and-document → pr-approval-and-merge
```

There are **no parallelizable assignments** in this workflow. Each must complete and pass validation before the next begins.

### 4.3 Data Handoffs

| From | To | Data | Mechanism |
|---|---|---|---|
| Assignment 1 | Assignment 6 | `$pr_num` | Variable passthrough in the workflow script |
| Assignment 1 | Assignments 2-6 | Working branch `dynamic-workflow-project-setup`, open PR | Git branch (shared) |
| Assignment 1 | Assignment 2 | GitHub Project ID | `gh` API / project link |
| Assignment 2 | Assignment 3 | Approved app plan (issue + plan_docs) | Issue body, `tech-stack.md`, `architecture.md` |
| Assignment 3 | Assignment 4 | Validated build/test commands, project structure | Filesystem + dotnet CLI |
| Assignments 1-5 | Assignment 5 | All work performed | Execution trace, git log |
| Assignments 1-5 | Assignment 6 | All commits on PR branch | Git history on PR |

---

## 5. Open Questions

### 5.1 .NET SDK Version Mismatch (CRITICAL)

**Question:** The devcontainer image has .NET SDK 10 installed, but `development-plan.md` §3 specifies .NET SDK 8.0.x (LTS). Which should the project target?

**Options:**
- **(A) Pin to .NET 8.0.x:** Use `global.json` with `"version": "8.0.x"` and `"rollForward": "latestFeature"`. SDK 10 can build .NET 8 targets. Aspire 8.2.x and all packages in the plan target .NET 8. This is the safest option — it follows the plan exactly.
- **(B) Upgrade to .NET 10:** Update development-plan.md to target .NET 10. Requires upgrading to Aspire 10.x-compatible packages, verifying EF Core 10 compatibility, and updating all version pins. Higher risk of breaking changes but leverages the installed SDK natively.

**Recommendation:** Option (A) — pin to .NET 8.0.x via `global.json`. The development plan was written with .NET 8 LTS and Aspire 8.2.x specifically. SDK 10's `rollForward` will handle compilation. Verify the build succeeds during assignment 3.

**Needs decision from:** Stakeholder / Supervising Engineer

---

### 5.2 Plan Docs Filename Discrepancy

**Question:** The `create-app-plan` assignment expects input from `plan_docs/ai-new-app-template.md`, but the actual plan_docs contain `development-plan.md` and `Strategic Feasibility and Execution Plan....md`. Should the agent treat `development-plan.md` as the de facto app template?

**Recommendation:** Yes — `development-plan.md` already serves as a comprehensive application plan (18 sections, 6 phases, exact tech stack, task-level acceptance criteria). The agent should use it as the primary input and note the filename adaptation in the debrief.

**Needs decision from:** Orchestrator (can likely be resolved by the executing agent with a note)

---

### 5.3 Apify Actor IDs

**Question:** `development-plan.md` T-2.2 maps MarketplaceKind values to Apify actor IDs (e.g., `apify/shopify-scraper`). These need human verification against the live Apify Store. When should this verification occur?

**Recommendation:** This is not blocking for the `project-setup` workflow (which only scaffolds). However, the app plan issue (assignment 2) should flag this as a prerequisite for Phase 2 (Scraper Pipeline). Mark actor IDs as `Status = NeedsVerification` in the code.

**Needs decision from:** Stakeholder (before Phase 2 development begins)

---

### 5.4 LLM Provider Selection

**Question:** `development-plan.md` allows either Azure OpenAI (GPT-4o) or Anthropic Claude 3.5 Sonnet. Which provider should the project use?

**Recommendation:** Not blocking for `project-setup`. Document both options in `tech-stack.md` (assignment 2). Resolve before Phase 3 (Intelligence Pipeline) development.

**Needs decision from:** Stakeholder (before Phase 3 development begins)

---

### 5.5 Template AGENTS.md Replacement Strategy

**Question:** The repository already contains a template `AGENTS.md` describing the orchestrator-service system. Assignment 4 creates a project-specific AGENTS.md for the Gap Mining Platform. Should the template AGENTS.md be completely replaced, or should both coexist?

**Recommendation:** The project-specific AGENTS.md should replace the template content. Once the project scaffolding (assignment 3) is in place, the repository's primary identity is the Gap Mining Platform, not the orchestrator template. The orchestrator infrastructure (`.opencode/`, workflows) still functions without the AGENTS.md describing it — those instructions live in the orchestrator agent definitions.

**Needs decision from:** Orchestrator (can likely be resolved by the executing agent)

---

### 5.6 Issue Template Availability

**Question:** Assignment 2 (`create-app-plan`) references an issue template at `.github/ISSUE_TEMPLATE/application-plan.md`. Does this file exist in the repository, or must it be created?

**Action:** Verify during assignment 2. If missing, the agent should adapt the Appendix A template from the `create-app-plan.md` assignment instructions.

**Needs decision from:** Executing agent (verify and adapt)

---

*End of workflow execution plan. Execution begins with Assignment 1: `init-existing-repository`.*
