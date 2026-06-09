import { useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { useUser, SignInButton, UserButton } from '@clerk/react'
import { useAppUser } from '../context/AppContext'
import JobPreferencesForm from './JobPreferencesForm'
import ResumeParserForm from './ResumeParserForm'
import CoverLetterForm from './CoverLetterForm'
import SavedSearchesForm from './SavedSearchesForm'

export default function Navbar() {
  const { isSignedIn, user: clerkUser } = useUser()
  const { user: appUser } = useAppUser()
  const location = useLocation()
  const [menuOpen, setMenuOpen] = useState(false)

  const isActive = (path: string) => location.pathname === path

  const closeMenu = () => setMenuOpen(false)

  return (
    <nav className="navbar">
      <div className="nav-inner">
        <Link to="/" className="nav-brand" onClick={closeMenu}>JobFinderNet</Link>

        <button className="hamburger" onClick={() => setMenuOpen(!menuOpen)} aria-label="Toggle menu">
          <span className={`hamburger-line ${menuOpen ? 'open' : ''}`} />
          <span className={`hamburger-line ${menuOpen ? 'open' : ''}`} />
          <span className={`hamburger-line ${menuOpen ? 'open' : ''}`} />
        </button>

        <div className={`nav-links ${menuOpen ? 'nav-links--open' : ''}`}>
          <Link
            to="/jobs"
            className={`nav-link${isActive('/jobs') ? ' active' : ''}`}
            onClick={closeMenu}
          >
            Jobs
          </Link>
          <Link
            to="/suggestions"
            className={`nav-link${isActive('/suggestions') ? ' active' : ''}`}
            onClick={closeMenu}
          >
            Suggestions
          </Link>
          {appUser?.role === 'Applicant' && (
            <Link
              to="/my-applications"
              className={`nav-link${isActive('/my-applications') ? ' active' : ''}`}
              onClick={closeMenu}
            >
              My Applications
            </Link>
          )}
          {appUser?.role === 'Applicant' && (
            <Link
              to="/saved"
              className={`nav-link${isActive('/saved') ? ' active' : ''}`}
              onClick={closeMenu}
            >
              Saved Jobs
            </Link>
          )}
          {appUser?.role === 'Employer' && (
            <Link
              to="/create-job"
              className={`nav-link${isActive('/create-job') ? ' active' : ''}`}
              onClick={closeMenu}
            >
              Post Job
            </Link>
          )}
          {appUser?.role === 'Employer' && (
            <Link
              to="/my-jobs"
              className={`nav-link${isActive('/my-jobs') || isActive('/edit-job') ? ' active' : ''}`}
              onClick={closeMenu}
            >
              My Jobs
            </Link>
          )}
          {appUser?.role === 'Employer' && (
            <Link
              to="/claim-company"
              className={`nav-link${isActive('/claim-company') ? ' active' : ''}`}
              onClick={closeMenu}
            >
              Claim Company
            </Link>
          )}
          {appUser?.role === 'Admin' && (
            <Link
              to="/admin"
              className={`nav-link${isActive('/admin') ? ' active' : ''}`}
              onClick={closeMenu}
            >
              Admin
            </Link>
          )}
          {!isSignedIn && (
            <div className="nav-mobile-auth">
              <SignInButton mode="redirect">
                <button className="btn btn-outline btn-sm" onClick={closeMenu}>Sign In</button>
              </SignInButton>
              <Link to="/sign-up" className="btn btn-primary btn-sm" onClick={closeMenu}>Register</Link>
            </div>
          )}
        </div>

        {menuOpen && <div className="nav-overlay" onClick={closeMenu} />}

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
                <UserButton.UserProfilePage
                  label="Resume Parser"
                  url="resume-parser"
                  labelIcon={<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/><polyline points="10 9 9 9 8 9"/></svg>}
                >
                  <ResumeParserForm />
                </UserButton.UserProfilePage>
                <UserButton.UserProfilePage
                  label="Cover Letter"
                  url="cover-letter"
                  labelIcon={<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>}
                >
                  <CoverLetterForm />
                </UserButton.UserProfilePage>
                {appUser?.role === 'Applicant' && (
                  <UserButton.UserProfilePage
                    label="Saved Searches"
                    url="saved-searches"
                    labelIcon={<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>}
                  >
                    <SavedSearchesForm />
                  </UserButton.UserProfilePage>
                )}
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
