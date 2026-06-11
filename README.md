# JobFinderNet — Service-Oriented Job Search & Recruitment Platform

A .NET 10 Web API powering job listings, applications, AI-powered resume parsing, smart candidate matching, and user management with a clean service-oriented architecture.

## Project Structure

```
JobFinderNet/
├── src/
│   ├── JobFinderNet.Core/                # Domain models, interfaces, DTOs
│   │   ├── Models/                       # Entity models (Job, Application, UserProfile, etc.)
│   │   ├── Interfaces/
│   │   │   ├── Repositories/             # Repository contracts
│   │   │   └── Services/                 # Service contracts
│   │   └── DTOs/                         # API data transfer objects
│   ├── JobFinderNet.Infrastructure/      # Data access, external services
│   │   ├── Data/                         # DbContext, seeding
│   │   ├── Repositories/                 # EF Core repository implementations
│   │   └── Services/                     # Business logic + background services
│   ├── JobFinderNet.Api/                 # ASP.NET Core Web API
│   │   ├── Controllers/                  # REST API controllers
│   │   ├── Middleware/                    # Error handling middleware
│   │   ├── Helpers/                      # Claims helper extensions
│   │   └── Program.cs                    # App entry point & DI config
│   └── JobFinderNet.Tests/               # xUnit tests
│       ├── Controllers/                  # Controller unit tests
│       ├── Services/                     # Service unit tests
│       ├── Repositories/                 # Repository unit tests (InMemory EF)
│       ├── Integration/                  # Full pipeline integration tests
│       └── Helpers/                      # Test factories and mocks
├── client/                               # React 19 + TypeScript + Vite frontend
│   └── src/
│       ├── api/                          # Axios instance + API wrappers
│       ├── components/                   # Reusable UI components
│       ├── pages/                        # Route pages
│       └── context/                      # AppContext (global state)
├── database/                             # PostgreSQL seed scripts
├── nginx/                                # Reverse proxy config + TLS
├── docs/                                 # Mermaid architecture diagrams
├── .github/workflows/deploy.yml          # CI/CD pipeline
├── docker-compose.yml                    # Development stack
├── docker-compose.prod.yml               # Production overrides
├── Dockerfile                            # API multi-stage build
├── Dockerfile.test                       # Test runner container
└── README.md
```

## Tech Stack

| Component | Technology |
|---|---|
| Framework | .NET 10.0, ASP.NET Core |
| Database | PostgreSQL 16 (via Npgsql) |
| ORM | Entity Framework Core 10.0 |
| Authentication | Clerk (JWT Bearer) |
| Frontend | React 19 + TypeScript 6 + Vite 8 |
| Caching | Redis 7 (StackExchange.Redis) |
| AI Integration | NVIDIA API (kimi-k2.6 LLM) |
| External Jobs | JSearch API v2 |
| Email | SMTP (Mailpit dev) / Mailjet (prod) |
| Containerization | Docker + Docker Compose |
| Reverse Proxy | Nginx (Alpine) with TLS |
| Testing | xUnit, Moq, EF Core InMemory |
| CI/CD | GitHub Actions |
| Cloud | Microsoft Azure (App Service) |

## Architecture — Service-Oriented

```
┌─────────────────┐     ┌──────────────────┐     ┌──────────────┐     ┌──────────┐
│   React 19 SPA  │────▶│   Nginx Proxy    │────▶│  ASP.NET Core│────▶│PostgreSQL│
│   (TypeScript)  │     │  TLS + Rate      │     │  Web API     │     │  16      │
│   Clerk Auth    │     │  Limiting        │     │  .NET 10     │     └──────────┘
└─────────────────┘     └──────────────────┘     │              │     ┌──────────┐
                                                  │ Controllers  │────▶│  Redis 7 │
                                                  │     │        │     │  Cache   │
                                                  │ Services     │     └──────────┘
                                                  │     │        │
                                                  │ Repositories │     ┌──────────┐
                                                  │     │        │────▶│ NVIDIA   │
                                                  │ EF Core 10   │     │ LLM API  │
                                                  └──────────────┘     └──────────┘
```

**Layers:**
- **Core** — Contains only domain models, interfaces, and DTOs. No infrastructure dependencies.
- **Infrastructure** — Implements Core interfaces. Handles data access, external services, background workers, and seeding.
- **Api** — Presentation layer. Controllers handle HTTP, delegate business logic to services.
- **Tests** — Unit and integration tests using mocks, in-memory database, and WebApplicationFactory.

**Design Patterns:** Repository Pattern, Decorator Pattern (CachedJobRepository), Dependency Injection, DTO Pattern, Background Service Pattern, Channel\<T\> Queue.

## API Endpoints

### Authentication
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/auth/me` | Get current user (auto-provisions Clerk users) | JWT |

### Jobs
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/jobs` | List jobs (paginated) | None |
| GET | `/api/jobs/{id}` | Get job details | None |
| GET | `/api/jobs/search?query=` | Search jobs | None |
| GET | `/api/jobs/{id}/similar` | Similar jobs by industry/tech | None |
| POST | `/api/jobs` | Create job | Employer/Admin |
| PUT | `/api/jobs/{id}` | Update job | Employer/Admin |
| DELETE | `/api/jobs/{id}` | Delete job | Employer/Admin |
| POST | `/api/jobs/{id}/toggle` | Toggle active status | Employer/Admin |
| GET | `/api/jobs/employer` | Employer's jobs | Employer |
| GET | `/api/jobs/{jobId}/applications` | View applicants | Employer/Admin |
| POST | `/api/jobs/sync` | Sync from JSearch API | Admin |
| POST | `/api/jobs/populate-techs` | Extract technologies | Admin |

### Applications
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/applications/{jobId}` | Apply to job | Applicant |
| GET | `/api/applications/my` | My applications | User |
| PUT | `/api/applications/{id}/status` | Update status | Employer/Admin |
| GET | `/api/applications/{id}/notes` | Get application notes | Employer/Admin |
| POST | `/api/applications/{id}/notes` | Add note | Employer/Admin |

### User Profile & Matching
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/profile` | Get/create profile | User |
| PUT | `/api/profile` | Update preferences | User |
| GET | `/api/profile/matched` | Get matched jobs | User |
| GET | `/api/profile/matched/detailed` | Matched jobs with score breakdown | User |
| GET | `/api/profile/skills` | Available skills list (200+) | User |

### Saved Jobs & Searches
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/savedjobs` | Get saved jobs | User |
| POST | `/api/savedjobs/{jobId}` | Save a job | User |
| DELETE | `/api/savedjobs/{jobId}` | Unsave a job | User |
| GET | `/api/savedjobs/ids` | Get saved job IDs | User |
| GET | `/api/savedsearches` | Get saved searches | User |
| POST | `/api/savedsearches` | Create saved search | User |
| PUT | `/api/savedsearches/{id}` | Update saved search | User |
| DELETE | `/api/savedsearches/{id}` | Delete saved search | User |
| POST | `/api/savedsearches/{id}/run` | Run saved search | User |

### Company Profiles
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/companyprofiles/{id}` | Get company profile | None |
| GET | `/api/companyprofiles?q=` | Search companies | None |
| GET | `/api/companyprofiles/my` | My claimed company | User |
| POST | `/api/companyprofiles/claim` | Claim/create company | Employer |
| PUT | `/api/companyprofiles/{id}` | Update company | Owner |

### Resume & AI
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/resume/parse` | Parse resume (text/image/PDF) | Applicant |
| POST | `/api/resume/recommendations` | Parse + get job recommendations | Applicant |
| POST | `/api/resume/recommendations/from-skills` | Recommendations from skills | Applicant |
| POST | `/api/resume/cover-letter` | AI-generated cover letter | Applicant |

### System
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/statistics` | Dashboard statistics | None |
| GET | `/api/health` | Health check | None |
| GET | `/api/ready` | Readiness check (DB) | None |

## Smart Matching Algorithm

The matching engine scores jobs against user profiles using five weighted factors:

| Factor | Weight | Method |
|---|---|---|
| Technology overlap | 40% | Jaccard similarity of skills vs required/preferred tech |
| Seniority match | 20% | Proximity of seniority levels (Junior→Director mapped to 0→5) |
| Salary range | 15% | Intersection of desired vs offered salary ranges |
| Location / Remote | 15% | Remote compatibility + city/state/country matching |
| Job type | 10% | Full-time / Part-time / Contract / Internship match |

Each factor returns a score (0–1) and a human-readable reason. Results are exposed via `/api/profile/matched/detailed` so users understand why a job was recommended.

## Docker Compose (Development)

```bash
docker compose up
```

| Service | Port | Purpose |
|---|---|---|
| postgres | 5433 | PostgreSQL 16 database |
| redis | 6380 | Distributed cache |
| mailpit | 8025 | Email testing UI |
| api | 8080 | .NET backend |
| client | 5173 | Vite dev server |
| nginx | 80, 443 | Reverse proxy + TLS |
| tests | — | Run `docker compose --profile test run tests` |

## Setup Instructions

### Prerequisites
- .NET 10 SDK
- PostgreSQL (local or via Docker)
- Node.js 22+ (for frontend)

### Quick Start (Docker)
```bash
git clone https://github.com/Sam1rShaban1/JobFinderNet.git
cd JobFinderNet
docker compose up
```

### Manual Setup
```bash
# Backend
export PATH="$HOME/.dotnet10:$PATH"
dotnet restore
dotnet build
dotnet run --project src/JobFinderNet.Api

# Frontend (separate terminal)
cd client
npm install
npm run dev
```

### Run Tests
```bash
dotnet test
# or via Docker
docker compose --profile test run tests
```

## Default Accounts (seeded)

| Role | Email | Password |
|---|---|---|
| Admin | admin@jobfinder.net | Admin123! |
| Employer | employer@jobfinder.net | Employer123! |
| Applicant | applicant@jobfinder.net | Applicant123! |

## Documentation

See the [`docs/`](docs/) folder for Mermaid architecture diagrams:
- `architecture.mmd` — Full layered architecture
- `er-diagram.mmd` — Entity-Relationship diagram
- `auth-flow.mmd` — Clerk authentication sequence
- `job-matching.mmd` — 5-factor matching algorithm flowchart
- `application-flow.mmd` — Job application sequence
- `resume-parsing.mmd` — AI resume parsing sequence
- `system-overview.mmd` — High-level system map
- `docker-architecture.mmd` — Docker + CI/CD pipeline
- `notification-flow.mmd` — Email notification pipeline
- `data-access-layer.mmd` — Repository pattern + DI

## Team

- **Samir Shabani**
- **Muhamed Idrizi**
- **Menan Sali**

South East European University — Service Oriented Architecture, 2026
