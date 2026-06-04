import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import api from '../api/axios'
import { SkeletonList } from '../components/Skeleton'

interface Application {
  id: number
  jobId: number
  status: string
  appliedDate: string
  job: { title: string; companyName: string }
}

export default function MyApplications() {
  const [apps, setApps] = useState<Application[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    setLoading(true)
    setError('')
    api.get('/applications/my').then((res) => {
      setApps(res.data)
      setLoading(false)
    }).catch((err) => {
      setError('Failed to load applications.')
      setLoading(false)
    })
  }, [])

  return (
    <div className="container">
      <div className="applications-page">
        <h1>My Applications</h1>
        {loading ? (
          <SkeletonList count={4} />
        ) : error ? (
          <div>
            <p className="body-large" style={{ color: '#616161', marginBottom: 16 }}>{error}</p>
            <button className="btn btn-outline" onClick={() => window.location.reload()}>Retry</button>
          </div>
        ) : apps.length === 0 ? (
          <p className="body-large" style={{ color: '#616161' }}>
            You haven't applied to any jobs yet.
          </p>
        ) : (
          <div className="applications-list">
            {apps.map((app) => (
              <Link key={app.id} to={`/jobs/${app.jobId}`} className="application-card" style={{ textDecoration: 'none', color: 'inherit', display: 'flex' }}>
                <div className="app-info">
                  <h3>{app.job.title}</h3>
                  <p className="company">{app.job.companyName}</p>
                  <p className="date">
                    Applied {new Date(app.appliedDate).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}
                  </p>
                </div>
                <span className={`badge status-${app.status.toLowerCase()}`}>{app.status}</span>
              </Link>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
