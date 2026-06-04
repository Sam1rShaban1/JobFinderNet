import { Routes, Route, Navigate } from 'react-router-dom'
import { useUser, SignIn, SignUp } from '@clerk/react'
import Navbar from './components/Navbar'
import ErrorBoundary from './components/ErrorBoundary'
import { AppProvider } from './context/AppContext'
import Jobs from './pages/Jobs'
import JobDetails from './pages/JobDetails'
import CreateJob from './pages/CreateJob'
import MyApplications from './pages/MyApplications'
import Suggestions from './pages/Suggestions'
import NotFound from './pages/NotFound'

function Home() {
  const { isSignedIn } = useUser()
  return (
    <>
      <section className="hero-section">
        <p className="micro" style={{ marginBottom: 24, color: '#616161' }}>JobFinderNet</p>
        <h1>Find your<br />next opportunity</h1>
        <p className="subtitle">
          Connect with top employers and discover career-defining roles
          that match your skills and ambitions.
        </p>
        {!isSignedIn ? (
          <div className="hero-actions">
            <a href="/sign-up" className="btn btn-primary">Get Started</a>
            <a href="/sign-in" className="btn btn-secondary">Sign In</a>
          </div>
        ) : (
          <div className="hero-actions">
            <a href="/jobs" className="btn btn-primary">Browse Jobs</a>
          </div>
        )}
      </section>

      <div className="container">
        <div className="trust-strip">
          <span className="micro">Trusted by leading companies</span>
          <div className="trust-logos">
            <span className="trust-logo">Stripe</span>
            <span className="trust-logo">Vercel</span>
            <span className="trust-logo">Linear</span>
            <span className="trust-logo">Figma</span>
            <span className="trust-logo">Notion</span>
            <span className="trust-logo">Ramp</span>
          </div>
        </div>

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
