# JobFinderNet — AI-Powered Job Search & Recruitment Platform

A full-stack job board with AI-powered resume parsing, smart job matching, and cover letter generation. Built with .NET 10, Clerk auth, NVIDIA AI, and React 19.

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 10, ASP.NET Core, Entity Framework Core |
| Frontend | React 19, TypeScript, Vite, React Router v7 |
| Database | PostgreSQL 16 (Npgsql) |
| Cache | Redis 7 (StackExchange.Redis) |
| Auth | Clerk (JWT Bearer, external IdP) |
| AI | NVIDIA API (Kimi K2.6 model) |
| Job Data | JSearch API (real job listings) |
| Email | SMTP via background queue + Mailpit (dev) |
| Infrastructure | Docker Compose, Nginx reverse proxy |
| Testing | xUnit, Moq, EF Core InMemory |

## Features

- **Clerk Authentication** — Sign up/in with Google, email, etc. Auto-provisions users with role-based access (Applicant, Employer, Admin)
- **AI Resume Parsing** — Upload a PDF/image or paste text; extracts skills, seniority, education via NVIDIA Kimi K2.6 multimodal
- **Smart Job Matching** — Adaptive scoring algorithm (tech overlap 40%, seniority 20%, salary 15%, location 15%, job type 10%) with detailed breakdowns
- **AI Cover Letter Generation** — Generate tailored cover letters with tone selection, persisted across sessions
- **Job Listings** — 960+ real jobs synced from JSearch API with cursor-based pagination, search, filters
- **Employer Dashboard** — Post jobs, manage applications, track hiring pipeline with Kanban board
- **Application Tracking** — Drag-and-drop Kanban (Applied → Screening → Interview → Offer → Rejected)
- **Saved Jobs & Searches** — Bookmark jobs, save search filters with configurable email alerts
- **Company Profiles** — Employers claim and manage company pages
- **Email Notifications** — Application confirmations, status changes, job matches, daily/weekly digests
- **Admin Panel** — Job sync, tech extraction, platform statistics
- **Responsive Design** — Mobile-first with hamburger nav, skeleton loading, error boundaries

## Architecture

```
┌─────────────┐     ┌─────────────┐     ┌──────────────┐
│  React SPA  │────▶│   Nginx     │────▶│  .NET API    │
│  (Clerk)    │     │  (reverse   │     │              │
│             │     │   proxy)    │     │  Controllers │
└─────────────┘     └─────────────┘     │      │       │
                                        │  Services    │
                                        │      │       │
                                        │  Repositories│
                                        │      │       │
                                        │  EF Core     │
                                        └──────┬───────┘
                                               │
                            ┌──────────────────┼──────────────────┐
                            │                  │                  │
                       ┌────▼────┐       ┌─────▼─────┐     ┌─────▼─────┐
                       │PostgreSQL│       │   Redis   │     │  NVIDIA   │
                       │   DB    │       │  (cache)  │     │  (AI API) │
                       └─────────┘       └───────────┘     └───────────┘
```

## Project Structure

```
JobFinderNet/
├── src/
│   ├── JobFinderNet.Core/                  # Domain models, interfaces, DTOs
│   │   ├── Models/                         # Job, Application, UserProfile, etc.
│   │   ├── Interfaces/
│   │   │   ├── Repositories/               # Repository contracts
│   │   │   └── Services/                   # Service contracts
│   │   └── DTOs/                           # API data transfer objects
│   ├── JobFinderNet.Infrastructure/        # Data access, external services
│   │   ├── Data/                           # DbContext, seeding
│   │   ├── Repositories/                   # EF Core repository implementations
│   │   └── Services/                       # Business logic (AI, matching, email, etc.)
│   ├── JobFinderNet.Api/                   # ASP.NET Core Web API
│   │   ├── Controllers/                    # REST API controllers (9 total)
│   │   ├── Helpers/                        # ClaimsHelper for auth
│   │   ├── Middleware/                      # Error handling
│   │   └── Program.cs                      # App entry point & DI config
│   ├── JobFinderNet.Tests/                 # xUnit tests
│   │   ├── Controllers/                    # Unit tests
│   │   ├── Services/                       # Service tests
│   │   ├── Repositories/                   # Repository tests
│   │   ├── Integration/                    # Integration tests
│   │   └── Helpers/                        # TestWebApplicationFactory, MockAiService
│   └── .env                                # Environment variables (not committed)
├── client/                                 # React SPA
│   ├── src/
│   │   ├── pages/                          # 14 page components
│   │   ├── components/                     # 8 reusable components
│   │   ├── api/                            # Axios client + typed API helpers
│   │   └── context/                        # React context for app state
│   └── package.json
├── database/
│   └── seed.sql                            # Full DB schema + seed data
├── nginx/                                  # Nginx reverse proxy config
├── docker-compose.yml                      # Dev stack (PostgreSQL, Redis, Mailpit, API, Client)
├── Dockerfile                              # API production image
├── Dockerfile.test                         # Test runner image
└── README.md
```

## API Endpoints

### Authentication
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/auth/me` | Get current user info | JWT |

### Jobs
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/jobs` | List jobs (paginated) | Public |
| GET | `/api/jobs/{id}` | Get job details | Public |
| GET | `/api/jobs/search?query=` | Search jobs | Public |
| GET | `/api/jobs/{id}/similar` | Get similar jobs | Public |
| POST | `/api/jobs` | Create job | Employer/Admin |
| PUT | `/api/jobs/{id}` | Update job | Employer/Admin |
| DELETE | `/api/jobs/{id}` | Delete job | Employer/Admin |
| POST | `/api/jobs/{id}/toggle` | Toggle active status | Employer/Admin |
| GET | `/api/jobs/employer` | Employer's own jobs | Employer |
| GET | `/api/jobs/{jobId}/applications` | View applicants | Employer/Admin |
| POST | `/api/jobs/populate-techs` | Extract tech keywords | Admin |
| POST | `/api/jobs/sync` | Sync jobs from JSearch | Admin |

### Applications
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/applications/{jobId}` | Apply to job | Applicant |
| GET | `/api/applications/my` | My applications | User |
| PUT | `/api/applications/{id}/status` | Update status | Employer/Admin |
| GET | `/api/applications/{id}/notes` | Get notes | Employer/Admin |
| POST | `/api/applications/{id}/notes` | Add note | Employer/Admin |

### Profile
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/profile` | Get profile | User |
| PUT | `/api/profile` | Update profile | User |
| GET | `/api/profile/matched` | Matched jobs (summary) | User |
| GET | `/api/profile/matched/detailed` | Matched jobs (breakdown) | User |
| GET | `/api/profile/skills` | Available skills list | User |

### Resume & AI
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/resume/parse` | Parse resume (text/image) | Applicant |
| POST | `/api/resume/recommendations` | Get recommendations from resume | Applicant |
| POST | `/api/resume/recommendations/from-skills` | Recommendations from skills | Applicant |
| POST | `/api/resume/cover-letter` | Generate cover letter | Applicant |

### Saved Jobs
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/savedjobs` | Get saved jobs | User |
| POST | `/api/savedjobs/{jobId}` | Save job | User |
| DELETE | `/api/savedjobs/{jobId}` | Unsave job | User |
| GET | `/api/savedjobs/ids` | Get saved job IDs | User |

### Saved Searches
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/savedsearches` | List saved searches | User |
| POST | `/api/savedsearches` | Create saved search | User |
| PUT | `/api/savedsearches/{id}` | Update saved search | User |
| DELETE | `/api/savedsearches/{id}` | Delete saved search | User |
| POST | `/api/savedsearches/{id}/run` | Run saved search | User |

### Company Profiles
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/companyprofiles/{id}` | Company detail | Public |
| GET | `/api/companyprofiles` | Search companies | Public |
| GET | `/api/companyprofiles/my` | My claimed company | User |
| POST | `/api/companyprofiles/claim` | Claim/create company | Employer |
| PUT | `/api/companyprofiles/{id}` | Update company | Owner |

### Statistics
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/statistics` | Platform stats | Public |

### Health
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/health` | Health check | Public |
| GET | `/api/ready` | Readiness check | Public |

## Setup

### Docker (Recommended)

```bash
# Clone and start
git clone https://github.com/Sam1rShaban1/JobFinderNet.git
cd JobFinderNet

# Create src/.env with your keys (see Environment Variables below)
cp client/.env.example client/.env  # then edit with your keys
cp src/.env.example src/.env  # then edit with your keys

# Start the full stack
docker compose up -d

# Run database migrations and seed
docker compose exec api dotnet ef database update
```

The app will be available at:
- **Frontend:** http://localhost (via Nginx)
- **API:** http://localhost:8080
- **Mailpit (email testing):** http://localhost:8025
- **PostgreSQL:** localhost:5433
- **Redis:** localhost:6380

### Local Development

```bash
# Prerequisites
# - .NET 10 SDK
# - PostgreSQL 16
# - Redis 7
# - Node.js 20+

# Backend
cd src
dotnet restore
dotnet run --project JobFinderNet.Api

# Frontend
cd client
npm install
npm run dev
```

### Run Tests

```bash
# Local
dotnet test src/JobFinderNet.Tests/

# Docker
docker compose --profile test run --rm tests
```

## Environment Variables

Create `src/.env`:

```env
# Clerk Authentication
Clerk__Authority=https://your-clerk-instance.clerk.accounts.dev

# JSearch API (job listings)
JSearch__ApiKey=your-jsearch-api-key

# NVIDIA AI (resume parsing, cover letters)
Nvidia__ApiKey=your-nvidia-api-key

# SMTP (email notifications)
Smtp__Host=mailpit
Smtp__Port=1025
Smtp__Username=
Smtp__Password=
Smtp__From=noreply@jobfinder.local

# PostgreSQL
ConnectionStrings__DefaultConnection=Host=postgres;Database=JobFinderDb;Username=postgres;Password=postgres

# Redis
ConnectionStrings__Redis=redis:6379
```

## Database Schema

Key tables (9 total plus Identity tables):

| Table | Description |
|---|---|
| `jobs` | Job listings with technologies, salary, location |
| `applications` | Job applications with status workflow |
| `user_profiles` | User preferences (skills, seniority, salary, remote) |
| `saved_jobs` | Bookmarked jobs |
| `saved_searches` | Saved search filters (jsonb) |
| `company_profiles` | Employer company pages |
| `application_notes` | Employer notes on applications |
| `pending_digests` | Queued digest emails |

## Default Accounts (Seeded)

| Role | Email | Password |
|---|---|---|
| Admin | admin@jobfinder.net | Admin123! |
| Employer | employer@jobfinder.net | Employer123! |
| Applicant | applicant@jobfinder.net | Applicant123! |

## Team

- **Samir Shabani**
- **Muhamed Idrizi**
- **Menan Sali**

South East European University — Service Oriented Architecture, 2026
