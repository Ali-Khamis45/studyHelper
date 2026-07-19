# AI Study OS — Mentor

A production-grade AI Study Operating System: Clean Architecture / DDD backend (ASP.NET Core)
+ Next.js 15 frontend, driven by a provider-agnostic AI Kernel (OpenAI, Anthropic, Gemini, Ollama)
and eight specialized agents (Supervisor, Planner, Tutor, Examiner, Memory, Analytics, Focus,
Recommendation).

Phase 1 scope: Authentication, Dashboard, Goals, Study Planner (daily loop), AI Mentor, Quiz
Engine, and basic Analytics. See the architecture document for the full design and milestone
sequence (M0–M11).

- Mentor persona & agent prompt source of truth: [docs/Readme.md](docs/Readme.md)

## Repo layout

```
frontend/     Next.js 15 app
backend/      ASP.NET Core solution (Clean Architecture: Domain / Application / Infrastructure / Api)
infra/        docker-compose (Postgres + Redis)
docs/         Mentor persona / agent prompt source of truth
```

## Local development

Prerequisites: .NET SDK 10+, Node 20+, Docker Desktop, [Ollama](https://ollama.com) (local model,
e.g. `ollama pull llama3.1`).

```bash
cp .env.example .env
cp frontend/.env.local.example frontend/.env.local

docker compose -f infra/docker-compose.yml up -d   # Postgres + Redis

cd backend
dotnet run --project src/Api/AiStudyOS.Api          # http://localhost:5246/scalar

cd ../frontend
npm install
npm run dev                                          # http://localhost:3000
```
