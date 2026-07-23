# AI Study OS — Mentor

A full-stack AI-native study platform: goals, an energy-aware daily planner, an AI mentor chat,
an adaptive quiz engine, and real data-driven analytics — all backed by one shared, provider-agnostic
AI pipeline instead of scattered ad-hoc LLM calls.

Built with a Clean Architecture / DDD backend (**ASP.NET Core 10**) and a **Next.js 16** frontend,
running entirely on a local, self-hosted AI stack ([Ollama](https://ollama.com) + Llama 3.1) — no
API keys, no per-token billing, no data leaving your machine.

**Status: Phase 1 (M0–M10) complete.** Every module below is real, tested, and runnable — no mock
data, no placeholder screens.

---

## What's in it

| Module | What it does |
|---|---|
| **Auth** | Email/password registration and login, JWT access tokens with rotating, reuse-detecting refresh tokens, account lockout, rate limiting, security headers. |
| **Goals** | Create and track study goals by category, priority, and target date. |
| **Planner** | An AI-generated daily plan grounded in your real goals and task history — energy-aware task sequencing, smart rescheduling, streaks, daily focus score, weekly workload balancing. |
| **Mentor** | A ChatGPT-style AI chat with real conversation persistence, streaming responses, and intent-based routing to specialist agents (Tutor, Planner, Analytics, Examiner) — never one hardcoded prompt. |
| **Quiz** | AI-generated quizzes (multiple-choice, true/false, short-answer, fill-in-the-blank) with automatic grading, weighted-moving-average topic mastery tracking, and weak-topic-driven review quizzes. |
| **Analytics** | Server-computed metrics over real data — study time, streaks, quiz trends, mastery evolution, planner effectiveness, AI usage — plus an AI-generated weekly/monthly insights report, and PDF/CSV export. |
| **Dashboard** | A real home page composed of independently-loading widgets (today's plan, streak, goal progress, weak topics, mastery chart, weekly activity, AI insights) — one widget failing never blocks the rest. |

## The AI pipeline

Every AI-backed feature in this app — the planner recommendation, the mentor chat, quiz
generation, analytics insights — goes through the exact same pipeline. No feature calls a model
provider directly, and no feature has its own bespoke prompt-building or JSON-parsing logic:

```
User request
    │
    ▼
Supervisor  ──▶  Intent Classifier  ──▶  Agent Registry  ──▶  Context Builder  ──▶  Prompt Library
                                                                                          │
                                                                                          ▼
                                                                                      IAiKernel
                                                                                          │
                                                                                          ▼
                                                                                       Provider
                                                                                     (Ollama)
                                                                                          │
                                                              ┌───────────────────────────┤
                                                              ▼                           ▼
                                                         Telemetry                   Persistence
```

- **Agent Registry** — each specialist agent (Recommendation, Tutor, Planner-chat, Analytics,
  Examiner, Quiz Generator, Insights) declares its own prompt, context providers, output schema,
  and retry policy — routing is data, not a switch statement full of prompts.
- **Context Builder** — assembles only the context an agent actually needs (goals, tasks, mastery,
  conversation history, memory) under a token budget, dropping lowest-priority fragments first.
- **IAiKernel** — the single component that ever talks to a provider adapter: shared JSON
  serialization, retry-with-repair on malformed output, circuit breaker, health caching, streaming
  and non-streaming through one identical code path, and per-request telemetry (latency, tokens,
  success/failure, correlation ID) persisted for every call.
- **Provider adapter** — currently [Ollama](https://ollama.com) (`llama3.1`), chosen so the whole
  app runs locally with zero API costs. The adapter boundary (`IAiChatClient`) is the intended
  extension point for a hosted provider later.

## Tech stack

**Backend** — ASP.NET Core 10 · EF Core 10 (Npgsql) · PostgreSQL 16 · [Mediator](https://github.com/martinothamar/Mediator)
(CQRS) · FluentValidation · Polly (circuit breaker) · QuestPDF · xUnit + FluentAssertions +
Testcontainers

**Frontend** — Next.js 16 (App Router, Turbopack) · React 19 · TanStack Query · Tailwind CSS v4 ·
shadcn/ui (Base UI) · Recharts · react-markdown

**Infrastructure** — Docker Compose (PostgreSQL + Redis¹) · Ollama (local inference)

<sub>¹ Redis is provisioned in `infra/docker-compose.yml` for future caching/session use — the app
doesn't read or write to it yet.</sub>

## Architecture

Clean Architecture, dependencies pointing inward:

```
Domain            Aggregates, entities, enums. No dependencies on anything else.
   ▲
Application       CQRS commands/queries, DTOs, AI orchestration, validation. Depends only on Domain.
   ▲
Infrastructure    EF Core, Postgres, the Ollama adapter, prompt files, DI wiring.
   ▲
Api               Minimal API endpoints, middleware, auth.
```

Every feature module (Goals, Planner, Mentor, Quiz, Analytics) follows the identical shape end to
end: a Domain aggregate → Application commands/queries → an EF configuration → a Minimal API
endpoint group → a typed frontend API client → a TanStack Query hook → a page. New features extend
this shape rather than inventing a new one.

## Getting started

**Prerequisites:** [.NET SDK 10](https://dotnet.microsoft.com/download), [Node.js 20+](https://nodejs.org),
[Docker Desktop](https://www.docker.com/products/docker-desktop/), [Ollama](https://ollama.com).

```bash
# 1. Pull the model the app is configured to use
ollama pull llama3.1

# 2. Environment files
cp .env.example .env
cp frontend/.env.local.example frontend/.env.local

# 3. Postgres + Redis
docker compose -f infra/docker-compose.yml up -d

# 4. Backend — apply migrations, then run the API
cd backend
dotnet ef database update --project src/Infrastructure/AiStudyOS.Infrastructure --startup-project src/Api/AiStudyOS.Api
dotnet run --project src/Api/AiStudyOS.Api        # http://localhost:5246 — Scalar docs at /scalar

# 5. Frontend
cd ../frontend
npm install
npm run dev                                        # http://localhost:3000
```

Open `http://localhost:3000`, register an account, and everything — goals, planner, mentor, quiz,
analytics — is live against real Ollama generation from the first request.

## Repo layout

```
backend/
  src/
    Domain/            Aggregates & entities — Goals, Planner, Mentor, Quiz, Analytics, Identity
    Application/        CQRS handlers, DTOs, AI orchestration (agents, context, prompts, kernel)
    Infrastructure/     EF Core + Postgres, Ollama adapter, prompt templates, DI composition
    Api/                Minimal API endpoints, middleware, Program.cs
  tests/
    *.UnitTests/        Domain + Application unit tests
    *.IntegrationTests/ Real-Postgres (Testcontainers) + real-Ollama endpoint tests
frontend/
  app/                  Next.js App Router pages (route groups: (auth), (app))
  components/           UI, grouped by feature module
  lib/                  Typed API clients, TanStack Query hooks, types, stores
infra/                  docker-compose.yml (Postgres + Redis)
docs/                   Mentor persona source of truth, security audit notes
```

## Testing

```bash
cd backend
dotnet test              # 182 tests: Domain, Application, Infrastructure, and Api.IntegrationTests
```

Integration tests spin up a real PostgreSQL instance via Testcontainers and, for the AI-generation
paths (recommendation, mentor chat, quiz generation, insights), call real Ollama — they are
correctness tests against the actual pipeline, not mocks. A handful of AI-dependent tests retry
once to absorb the ordinary non-determinism of a small local model; this never masks a real
failure, only a single bad generation.

```bash
cd frontend
npx tsc --noEmit
npm run build
npm run lint
```

## Milestones

| | Milestone | |
|---|---|---|
| ✅ | M0 | Clean Architecture scaffolding |
| ✅ | M1 | Authentication (register/login/refresh, lockout, rate limiting, security headers) |
| ✅ | M3 | Goals |
| ✅ | M5–M6 | Study Planner + Daily Loop Intelligence (energy-aware planning, streaks, smart reschedule) |
| ✅ | M7 | AI Mentor (streaming chat, intent routing, persistence) |
| ✅ | M8 | Quiz Engine (generation, grading, topic mastery) |
| ✅ | M9 | Analytics (real computed metrics, AI insights, PDF/CSV export) |
| ✅ | M10 | Dashboard (independent, real-data widgets) |

Mentor persona and agent prompt source of truth: [`docs/Readme.md`](docs/Readme.md). Security audit
notes: [`docs/security/m1-auth-hardening.md`](docs/security/m1-auth-hardening.md).
