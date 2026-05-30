import { Link } from 'react-router-dom'
import { useUser, SignInButton, UserButton } from '@clerk/react'
import { useAppUser } from '../context/AppContext'
import JobPreferencesForm from './JobPreferencesForm'

export default function Navbar() {
  const { isSignedIn, user: clerkUser } = useUser()
  const { user: appUser } = useAppUser()

  return (
    <nav className="navbar">
      <div className="nav-inner">
        <Link to="/" className="nav-brand">JobFinderNet</Link>
        <div className="nav-links">
          <Link to="/jobs" className="nav-link">Jobs</Link>
          <Link to="/suggestions" className="nav-link">Suggestions</Link>
          {appUser?.role === 'Applicant' && (
            <Link to="/my-applications" className="nav-link">My Applications</Link>
          )}
          {appUser?.role === 'Employer' && (
            <Link to="/create-job" className="nav-link">Post Job</Link>
          )}
        </div>
        <div className="nav-auth">
          {isSignedIn ? (
            <div className="nav-user">
              <span className="nav-email">
                {[clerkUser?.firstName, clerkUser?.lastName].filter(Boolean).join(' ') ||
                 clerkUser?.username ||
                 clerkUser?.primaryEmailAddress?.emailAddress}
              </span>
              {appUser && <span className={`badge role-${appUser.role.toLowerCase()}`}>{appUser.role}</span>}
              <UserButton afterSignOutUrl="/">
                <UserButton.UserProfilePage
                  label="Job Preferences"
                  url="job-preferences"
                  labelIcon={<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M12 2L2 7l10 5 10-5-10-5z"/><path d="M2 17l10 5 10-5"/><path d="M2 12l10 5 10-5"/></svg>}
                >
                  <JobPreferencesForm />
                </UserButton.UserProfilePage>
              </UserButton>
            </div>
          ) : (
            <div className="nav-auth-links">
              <SignInButton mode="redirect">
                <button className="btn btn-outline btn-sm">Sign In</button>
              </SignInButton>
              <Link to="/sign-up" className="btn btn-primary btn-sm">Register</Link>
            </div>
          )}
        </div>
      </div>
    </nav>
  )
}
