import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import api from '../api/axios'
import { SkeletonList } from '../components/Skeleton'

interface SavedJob {
  id: number
  jobId: number
  savedDate: string
  job: {
    id: number
    title: string
    companyName: string
    location: string
    jobType: string
    salary: string
    experienceRequired: string
    postedDate: string
  }
}

function formatSalaryText(salary: string): string {
  return salary.replace(/\$(\d+)\s*-\s*\$(\d+)/g, (_, a, b) => {
    const fmt = (n: number) => n >= 1000 ? `$${Math.round(n / 1000)}k` : `$${n}`
    return `${fmt(+a)} - ${fmt(+b)}`
  })
}

export default function SavedJobs() {
  const [saved, setSaved] = useState<SavedJob[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    setLoading(true)
    api.get('/savedjobs')
      .then((res) => { setSaved(res.data); setLoading(false) })
      .catch(() => { setError('Failed to load saved jobs.'); setLoading(false) })
  }, [])

  const unsave = async (jobId: number) => {
    try {
      await api.delete(`/savedjobs/${jobId}`)
      setSaved(s => s.filter(x => x.jobId !== jobId))
    } catch {}
  }

  if (loading) return (
    <div className="container" style={{ paddingTop: 60 }}>
      <h1 style={{ fontSize: 32, marginBottom: 24 }}>Saved Jobs</h1>
      <SkeletonList count={4} />
    </div>
  )

  if (error) return (
    <div className="container" style={{ paddingTop: 60 }}>
      <h1 style={{ fontSize: 32, marginBottom: 16 }}>Saved Jobs</h1>
      <p style={{ color: '#616161', marginBottom: 16 }}>{error}</p>
      <button className="btn btn-outline" onClick={() => window.location.reload()}>Retry</button>
    </div>
  )

  return (
    <div className="container">
      <div className="jobs-header">
        <h1 style={{ fontSize: 32 }}>Saved Jobs</h1>
        <Link to="/jobs" className="btn btn-outline">Browse Jobs</Link>
      </div>

      {saved.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state-icon">&#9825;</div>
          <h3>No saved jobs yet</h3>
          <p>Browse jobs and click the heart icon to save positions you're interested in.</p>
          <Link to="/jobs" className="btn btn-outline">Browse Jobs</Link>
        </div>
      ) : (
        <div className="job-grid">
          {saved.map((s) => (
            <Link key={s.id} to={`/jobs/${s.job.id}`} className="job-card">
              <div className="job-card-header">
                <h3>{s.job.title}</h3>
                <span className={`badge ${s.job.jobType.toLowerCase()}`}>{s.job.jobType}</span>
              </div>
              <div className="job-card-body">
                <p className="company">{s.job.companyName}</p>
                <p className="meta">
                  <span>{s.job.location}</span>
                  <span>{formatSalaryText(s.job.salary)}</span>
                  {s.job.experienceRequired !== 'Not specified' && <span>{s.job.experienceRequired}</span>}
                </p>
                <p className="date">{new Date(s.job.postedDate).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}</p>
              </div>
              <div style={{ display: 'flex', gap: 8, alignSelf: 'flex-start' }}>
                <button
                  className="btn btn-text btn-sm"
                  onClick={(e) => { e.preventDefault(); e.stopPropagation(); unsave(s.jobId) }}
                  style={{ color: '#e44' }}
                >
                  Unsave
                </button>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  )
}
