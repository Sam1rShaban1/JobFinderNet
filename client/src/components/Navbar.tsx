import { Link } from 'react-router-dom'
import { useUser, SignInButton, UserButton } from '@clerk/react'
import { useAppUser } from '../context/AppContext'

export default function Navbar() {
  const { isSignedIn, user: clerkUser } = useUser()
  const { user: appUser } = useAppUser()

  return (
    <nav className="navbar">
      <div className="nav-inner">
        <Link to="/" className="nav-brand">JobFinderNet</Link>
        <div className="nav-links">
          <Link to="/jobs" className="nav-link">Jobs</Link>
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
              <UserButton afterSignOutUrl="/" />
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
