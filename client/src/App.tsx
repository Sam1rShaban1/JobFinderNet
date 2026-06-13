import { useState } from 'react'
import { Routes, Route, Navigate, useNavigate } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import { useUser, SignIn, SignUp } from '@clerk/react'
import Navbar from './components/Navbar'
import ErrorBoundary from './components/ErrorBoundary'
import Iridescence from './components/Iridescence'
import StatsCounter from './components/StatsCounter'
import { AppProvider } from './context/AppContext'
import Jobs from './pages/Jobs'
import JobDetails from './pages/JobDetails'
import CreateJob from './pages/CreateJob'
import MyApplications from './pages/MyApplications'
import Suggestions from './pages/Suggestions'
import NotFound from './pages/NotFound'
import SavedJobs from './pages/SavedJobs'
import SavedSearches from './pages/SavedSearches'
import CompanyProfile from './pages/CompanyProfile'
import ClaimCompany from './pages/ClaimCompany'
import MyJobs from './pages/MyJobs'
import Admin from './pages/Admin'
import EmployerDashboard from './pages/EmployerDashboard'

function Home() {
  const { isSignedIn } = useUser()
  const navigate = useNavigate()
  const [heroSearch, setHeroSearch] = useState('')

  const handleHeroSearch = (e: React.FormEvent) => {
    e.preventDefault()
    navigate(`/jobs?search=${encodeURIComponent(heroSearch)}`)
  }

  return (
    <>
      <section className="hero-section">
        <Iridescence
          mouseReact
          amplitude={0.1}
          speed={0.8}
        />
        <p className="micro hero-label">JobFinderNet</p>
        <h1>Find your<br />next opportunity</h1>
        <p className="subtitle">
          Connect with top employers and discover career-defining roles
          that match your skills and ambitions.
        </p>
        <form onSubmit={handleHeroSearch} className="hero-search">
          <input
            type="text"
            value={heroSearch}
            onChange={(e) => setHeroSearch(e.target.value)}
            placeholder="Search by title, skill, or company..."
          />
          <button type="submit" className="btn btn-primary">Search</button>
        </form>
        {!isSignedIn ? (
          <div className="hero-actions">
            <a href="/sign-up" className="btn btn-secondary">Get Started</a>
            <a href="/sign-in" className="btn btn-secondary">Sign In</a>
          </div>
        ) : (
          <div className="hero-actions">
            <a href="/jobs" className="btn btn-secondary">Browse All Jobs</a>
          </div>
        )}
      </section>

      <div className="container">
        <StatsCounter />

        {/* <div className="trust-strip">
          <span className="micro">Trusted by leading companies</span>
          <div className="trust-logos">
            <span className="trust-logo" style={{ fontWeight: 700 }}>Stripe</span>
            <span className="trust-logo" style={{ fontWeight: 600 }}>Vercel</span>
            <span className="trust-logo" style={{ fontWeight: 700, letterSpacing: '-0.5px' }}>Linear</span>
            <span className="trust-logo" style={{ fontWeight: 600 }}>Figma</span>
            <span className="trust-logo" style={{ fontWeight: 700 }}>Notion</span>
            <span className="trust-logo" style={{ fontWeight: 600, letterSpacing: '0.5px' }}>Ramp</span>
          </div>
        </div> */}

        <div className="feature-grid">
          <div className="feature-card">
            <div className="feature-icon">&#9670;</div>
            <h4 className="h4">Smart Matching</h4>
            <p className="body-large" style={{ color: '#616161', fontSize: 14 }}>
              Our algorithm connects your skills with the right opportunities,
              saving you time and effort.
            </p>
          </div>
          <div className="feature-card">
            <div className="feature-icon">&#9632;</div>
            <h4 className="h4">Real-time Updates</h4>
            <p className="body-large" style={{ color: '#616161', fontSize: 14 }}>
              Get instant notifications when new positions match your
              preferences and experience.
            </p>
          </div>
          <div className="feature-card">
            <div className="feature-icon">&#9671;</div>
            <h4 className="h4">Seamless Process</h4>
            <p className="body-large" style={{ color: '#616161', fontSize: 14 }}>
              From application to offer, track every step of your job
              search journey in one place.
            </p>
          </div>
        </div>

        <div className="dark-band">
          <p className="micro" style={{ color: 'rgba(255,255,255,0.5)', marginBottom: 16 }}>For Employers</p>
          <h2>Find your next<br />great hire</h2>
          <p className="subtitle">
            Post jobs, review applications, and build your team
            with our powerful recruitment platform.
          </p>
          {!isSignedIn && <a href="/sign-up" className="btn btn-primary">Start Hiring</a>}
        </div>
      </div>
    </>
  )
}

function ProtectedRoute({ children }: { children: React.ReactElement }) {
  const { isSignedIn, isLoaded } = useUser()
  if (!isLoaded) return null
  if (!isSignedIn) return <Navigate to="/sign-in" />
  return children
}

export default function App() {
  return (
    <AppProvider>
      <Toaster position="top-right" toastOptions={{ duration: 3000 }} />
      <Navbar />
      <main style={{ flex: 1 }}>
        <ErrorBoundary>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/sign-in/*" element={<SignIn routing="path" path="/sign-in" />} />
            <Route path="/sign-up/*" element={<SignUp routing="path" path="/sign-up" />} />
            <Route path="/jobs" element={<Jobs />} />
            <Route path="/jobs/:id" element={<JobDetails />} />
            <Route path="/suggestions" element={<Suggestions />} />
            <Route path="/create-job" element={
              <ProtectedRoute><CreateJob /></ProtectedRoute>
            } />
            <Route path="/my-applications" element={
              <ProtectedRoute><MyApplications /></ProtectedRoute>
            } />
            <Route path="/saved" element={
              <ProtectedRoute><SavedJobs /></ProtectedRoute>
            } />
            <Route path="/saved-searches" element={
              <ProtectedRoute><SavedSearches /></ProtectedRoute>
            } />
            <Route path="/company/:id" element={<CompanyProfile />} />
            <Route path="/claim-company" element={
              <ProtectedRoute><ClaimCompany /></ProtectedRoute>
            } />
            <Route path="/my-jobs" element={
              <ProtectedRoute><MyJobs /></ProtectedRoute>
            } />
            <Route path="/employer-dashboard" element={
              <ProtectedRoute><EmployerDashboard /></ProtectedRoute>
            } />
            <Route path="/edit-job/:id" element={
              <ProtectedRoute><CreateJob /></ProtectedRoute>
            } />
            <Route path="/admin" element={
              <ProtectedRoute><Admin /></ProtectedRoute>
            } />
            <Route path="*" element={<NotFound />} />
          </Routes>
        </ErrorBoundary>
      </main>
      <footer className="footer">
        <div className="footer-inner">
          <span className="footer-brand">JobFinderNet</span>
          <span className="micro">&copy; {new Date().getFullYear()} JobFinderNet. All rights reserved.</span>
        </div>
      </footer>
    </AppProvider>
  )
}
