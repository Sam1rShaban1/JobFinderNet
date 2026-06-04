import { Link } from 'react-router-dom'

export default function NotFound() {
  return (
    <div className="container" style={{ paddingTop: 80, textAlign: 'center' }}>
      <h1 style={{ fontSize: 72, marginBottom: 8, color: 'var(--body-muted)' }}>404</h1>
      <p style={{ fontSize: 20, color: '#616161', marginBottom: 8 }}>Page not found</p>
      <p style={{ color: '#888', marginBottom: 32 }}>
        The page you're looking for doesn't exist or has been moved.
      </p>
      <Link to="/" className="btn btn-primary">Go Home</Link>
    </div>
  )
}
