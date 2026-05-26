# JobFinderNet — Service-Oriented Job Search & Recruitment Platform

A .NET 10 Web API powering job listings, applications, messaging, and user management with a clean service-oriented architecture.

## Project Structure

```
JobFinderNet/
├── src/
│   ├── JobFinderNet.Core/              # Domain models, interfaces, DTOs
│   │   ├── Models/                     # Entity models (Job, Application, ApplicationUser)
│   │   ├── Interfaces/
│   │   │   ├── Repositories/           # Repository contracts
│   │   │   └── Services/               # Service contracts
│   │   └── DTOs/                       # API data transfer objects
│   ├── JobFinderNet.Infrastructure/    # Data access, external services
│   │   ├── Data/                       # DbContext, migrations, seeding
│   │   ├── Repositories/               # EF Core repository implementations
│   │   ├── Services/                   # Business logic service implementations
│   │   └── Factories/                  # Test data generators (Bogus)
│   ├── JobFinderNet.Api/               # ASP.NET Core Web API
│   │   ├── Controllers/                # REST API controllers
│   │   ├── Middleware/                  # Error handling middleware
│   │   ├── Auth/                       # JWT token service
│   │   └── Program.cs                  # App entry point & DI config
│   └── JobFinderNet.Tests/             # xUnit tests
│       ├── Repositories/               # Repository tests (InMemory EF)
│       ├── Services/                   # Service tests (Moq)
│       └── Controllers/                # Controller tests (Moq)
├── .github/workflows/deploy.yml        # CI/CD pipeline
├── infra/main.bicep                    # Azure infrastructure templates
└── README.md
```

## Tech Stack

| Component | Technology |
|---|---|
| Framework | .NET 10.0, ASP.NET Core |
| Database | PostgreSQL (via Npgsql) |
| ORM | Entity Framework Core 10.0 |
| Auth | JWT Bearer + ASP.NET Core Identity |
| Testing | xUnit, Moq, EF Core InMemory |
| CI/CD | GitHub Actions |

## Architecture — Service-Oriented

```
┌─────────────┐     ┌─────────────────┐     ┌──────────────────┐
│  React SPA  │────▶│  JobFinderNet   │────▶│   PostgreSQL     │
│  (optional) │     │  Web API        │     │   Database       │
└─────────────┘     │                 │     └──────────────────┘
                    │  Controllers    │
                    │       │         │
                    │  Services       │
                    │       │         │
                    │  Repositories   │
                    │       │         │
                    │  EF Core        │
                    └─────────────────┘
```

**Layers:**
- **Core** — Contains only domain models, interfaces, and DTOs. No infrastructure dependencies.
- **Infrastructure** — Implements Core interfaces. Handles data access, external services, and seeding.
- **Api** — Presentation layer. Controllers handle HTTP, delegate business logic to services.
- **Tests** — Unit tests for all layers using mocks and in-memory database.

## API Endpoints

### Authentication
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/auth/register` | Register new user | None |
| POST | `/api/auth/login` | Login, returns JWT | None |
| GET | `/api/auth/me` | Get current user | JWT |

### Jobs
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/jobs` | List jobs (paginated) | None |
| GET | `/api/jobs/{id}` | Get job details | None |
| GET | `/api/jobs/search?query=` | Search jobs | None |
| POST | `/api/jobs` | Create job | Employer/Admin |
| DELETE | `/api/jobs/{id}` | Delete job | Employer/Admin |
| POST | `/api/jobs/{id}/toggle` | Toggle active status | Employer/Admin |
| GET | `/api/jobs/employer` | Employer's jobs | Employer |
| GET | `/api/jobs/{id}/applications` | View applicants | Employer/Admin |

### Applications
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/applications/{jobId}` | Apply to job | Applicant |
| GET | `/api/applications/my` | My applications | User |
| PUT | `/api/applications/{id}/status` | Update status | Employer/Admin |

## Setup Instructions

### Prerequisites
- .NET 10 SDK
- PostgreSQL (local or remote)

### Configuration
Update `src/JobFinderNet.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=JobFinderDb;Username=postgres;Password=your_password;Port=5432"
  },
  "Jwt": {
    "Key": "YourSecretKeyHereAtLeast32CharactersLong!",
    "Issuer": "JobFinderNet",
    "Audience": "JobFinderNet"
  }
}
```

### Run Locally
```bash
export PATH="$HOME/.dotnet10:$PATH"
dotnet restore
dotnet build
dotnet run --project src/JobFinderNet.Api
```

The API starts on `http://localhost:5179` with Swagger at `/openapi/v1.json`.

### Run Tests
```bash
dotnet test
```

## Default Accounts (seeded)
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
