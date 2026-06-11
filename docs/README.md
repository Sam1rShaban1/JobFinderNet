# JobFinderNet — Mermaid Diagrams

This directory contains architecture, flow, and system diagrams for the JobFinderNet project, rendered in [Mermaid](https://mermaid.js.org/) syntax.

## Diagrams

| File | Description |
|---|---|
| `architecture.mmd` | Full layered architecture — Client → Nginx → API → Services → Repos → Data → External |
| `er-diagram.mmd` | Entity-Relationship diagram for all database entities with attributes and relationships |
| `auth-flow.mmd` | Sequence diagram: Clerk sign-in, JWT validation, auto-provisioning, role-based authorization |
| `job-matching.mmd` | Flowchart: 5-factor smart matching algorithm (Tech 40%, Seniority 20%, Salary 15%, Location 15%, JobType 10%) |
| `application-flow.mmd` | Sequence diagram: Apply to job → email notification → employer reviews → status update |
| `resume-parsing.mmd` | Sequence diagram: Resume upload → NVIDIA LLM parsing → job recommendations → cover letter generation |
| `system-overview.mmd` | High-level system map showing user roles, frontend pages, API endpoints, and background services |
| `docker-architecture.mmd` | Docker Compose services (dev vs prod) and GitHub Actions CI/CD pipeline |
| `notification-flow.mmd` | Email notification pipeline: event triggers → NotificationService → Channel queue → background workers → SMTP/Mailjet |
| `data-access-layer.mmd` | Repository pattern with DI registration, CachedJobRepository decorator, and Redis caching |

## How to View

### VS Code
Install the **Markdown Preview Mermaid Support** extension, then open any `.mmd` file and preview.

### Online
Copy the contents of any `.mmd` file into [mermaid.live](https://mermaid.live) for instant rendering.

### GitHub
GitHub renders Mermaid diagrams natively in `.md` files. Embed them in any Markdown file:
```markdown
```mermaid
<contents of .mmd file>
```
```
