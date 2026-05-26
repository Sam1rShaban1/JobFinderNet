import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Navbar() {
  const { user, logout } = useAuth();

  return (
    <nav className="navbar">
      <div className="nav-inner">
        <Link to="/" className="nav-brand">JobFinderNet</Link>
        <div className="nav-links">
          <Link to="/jobs" className="nav-link">Jobs</Link>
          {user?.role === 'Applicant' && (
            <Link to="/my-applications" className="nav-link">My Applications</Link>
          )}
          {user?.role === 'Employer' && (
            <Link to="/create-job" className="nav-link">Post Job</Link>
          )}
        </div>
        <div className="nav-auth">
          {user ? (
            <div className="nav-user">
              <span className="nav-email">{user.email}</span>
              <span className={`badge role-${user.role.toLowerCase()}`}>{user.role}</span>
              <button onClick={logout} className="btn btn-outline btn-sm">Logout</button>
            </div>
          ) : (
            <div className="nav-auth-links">
              <Link to="/login" className="btn btn-outline btn-sm">Sign In</Link>
              <Link to="/register" className="btn btn-primary btn-sm">Register</Link>
            </div>
          )}
        </div>
      </div>
    </nav>
  );
}
