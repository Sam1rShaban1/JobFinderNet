# JobFinderNet Client

React 19 single-page application for the JobFinderNet job board.

## Tech Stack

- **React 19** + TypeScript
- **Vite** (dev server + build)
- **Clerk** — Authentication (sign in, sign up, user profile)
- **React Router v7** — Client-side routing
- **Axios** — API client with token injection
- **react-hot-toast** — Toast notifications

## Setup

```bash
npm install
npm run dev
```

The dev server runs at http://localhost:5173 and proxies API requests to http://localhost:8080.

## Environment Variables

Create a `.env` file in the `client/` directory:

```env
VITE_CLERK_PUBLISHABLE_KEY=pk_test_your_clerk_key
```

## Scripts

| Command | Description |
|---|---|
| `npm run dev` | Start Vite dev server with HMR |
| `npm run build` | Production build |
| `npm run preview` | Preview production build locally |
| `npm run lint` | Run ESLint |

## Project Structure

```
client/src/
├── pages/                  # Page components (14 routes)
│   ├── Jobs.tsx            # Job listings with search + pagination
│   ├── JobDetails.tsx      # Job detail + similar jobs + apply
│   ├── CreateJob.tsx       # Create/edit job form (employer)
│   ├── MyJobs.tsx          # Employer job management dashboard
│   ├── MyApplications.tsx  # Kanban application tracker
│   ├── Suggestions.tsx     # AI-matched jobs with score breakdown
│   ├── SavedJobs.tsx       # Bookmarked jobs list
│   ├── SavedSearches.tsx   # Saved search filters
│   ├── CompanyProfile.tsx  # Public company page
│   ├── ClaimCompany.tsx    # Claim company profile (employer)
│   ├── Admin.tsx           # Admin dashboard
│   └── NotFound.tsx        # 404 page
├── components/             # Reusable UI components
│   ├── Navbar.tsx          # Responsive nav with role-based links
│   ├── HeartButton.tsx     # Save/unsave toggle
│   ├── StatsCounter.tsx    # Animated homepage stats
│   ├── Skeleton.tsx        # Loading skeleton variants
│   ├── ErrorBoundary.tsx   # React error boundary
│   ├── JobPreferencesForm.tsx   # Profile editor (Clerk tab)
│   ├── ResumeParserForm.tsx     # AI resume parser (Clerk tab)
│   ├── CoverLetterForm.tsx      # AI cover letter generator (Clerk tab)
│   └── SavedSearchesForm.tsx    # Saved searches (Clerk tab)
├── api/
│   └── axios.ts            # Axios instance + typed API helpers
├── context/
│   └── AppContext.tsx       # App user state (fetches /api/auth/me)
├── App.tsx                 # Route definitions + layout
├── main.tsx                # Entry point
└── index.css               # Global styles
```

## Key Features

- **Role-based navigation** — Navbar adapts for Applicant, Employer, Admin roles
- **Clerk UserButton tabs** — Job Preferences, Resume Parser, Cover Letter, Saved Searches inside the profile modal
- **Kanban board** — Drag-and-drop application tracking with 5 status columns
- **AI resume parsing** — Upload PDF/image or paste text to extract skills and get job recommendations
- **Cover letter generation** — Tailored letters with tone selection, persisted in localStorage
- **Skeleton loading** — Loading states for all major views
- **Error handling** — ErrorBoundary + 404 page + toast notifications
- **Responsive** — Mobile hamburger menu, adaptive layouts
