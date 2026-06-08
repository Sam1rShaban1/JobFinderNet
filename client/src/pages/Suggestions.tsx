import { useState, useEffect, useCallback } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '@clerk/react'
import api from '../api/axios'
import { SkeletonList } from '../components/Skeleton'

interface MatchScoreBreakdown {
  technologyScore: number
  seniorityScore: number
  salaryScore: number
  locationScore: number
  jobTypeScore: number
  totalScore: number
  matchedSkills: string[]
  missingSkills: string[]
  seniorityMatchReason?: string
  salaryMatchReason?: string
  locationMatchReason?: string
  jobTypeMatchReason?: string
}

interface MatchedJob {
  id: number
  title: string
  companyName: string
  location: string
  jobType: string
  salary: string
  experienceRequired: string
  postedDate: string
  isRemote: boolean
  score: number
  breakdown: MatchScoreBreakdown
}

function formatSalaryText(salary: string): string {
  return salary.replace(/\$(\d+)\s*-\s*\$(\d+)/g, (_, a, b) => {
    const fmt = (n: number) => n >= 1000 ? `$${Math.round(n / 1000)}k` : `$${n}`
    return `${fmt(+a)} - ${fmt(+b)}`
  })
}

export default function Suggestions() {
  const { isSignedIn, isLoaded } = useAuth()
  const [jobs, setJobs] = useState<MatchedJob[]>([])
  const [loading, setLoading] = useState(true)
  const [hasProfile, setHasProfile] = useState(true)
  const [error, setError] = useState('')

  const fetchJobs = useCallback(async () => {
    if (!isLoaded || !isSignedIn) return
    setLoading(true)
    setError('')
    try {
      const res = await api.get('/profile/matched/detailed?limit=12')
      setJobs(res.data)
      setHasProfile(true)
    } catch (err: any) {
      if (err.response?.status === 404) {
        setHasProfile(false)
      } else {
        setError('Failed to load suggestions.')
      }
    } finally {
      setLoading(false)
    }
  }, [isLoaded, isSignedIn])

  useEffect(() => {
    if (!isLoaded) return
    if (!isSignedIn) {
      setLoading(false)
      setHasProfile(false)
      return
    }
    fetchJobs()
  }, [fetchJobs, isLoaded, isSignedIn])

  useEffect(() => {
    const handler = () => fetchJobs()
    window.addEventListener('preferences-saved', handler)
    return () => window.removeEventListener('preferences-saved', handler)
  }, [fetchJobs])

  if (!isLoaded || loading) {
    return (
      <div className="container" style={{ paddingTop: 60 }}>
        <h1 style={{ fontSize: 32, marginBottom: 24 }}>Suggestions</h1>
        <SkeletonList count={6} />
      </div>
    )
  }

  if (error) {
    return (
      <div className="container" style={{ paddingTop: 60 }}>
        <h1 style={{ fontSize: 32, marginBottom: 16 }}>Suggestions</h1>
        <p style={{ color: '#616161', marginBottom: 24 }}>{error}</p>
        <button className="btn btn-outline" onClick={() => fetchJobs()}>Retry</button>
      </div>
    )
  }

  if (!isSignedIn) {
    return (
      <div className="container" style={{ paddingTop: 60 }}>
        <h1 style={{ fontSize: 32, marginBottom: 16 }}>Suggestions</h1>
        <p style={{ color: '#616161' }}>Sign in to get personalized job suggestions.</p>
      </div>
    )
  }

  if (!hasProfile) {
    return (
      <div className="container" style={{ paddingTop: 60 }}>
        <h1 style={{ fontSize: 32, marginBottom: 16 }}>Suggestions</h1>
        <p style={{ color: '#616161', marginBottom: 24 }}>
          Set up your job preferences first to get personalized suggestions.
        </p>
        <p style={{ color: '#888', fontSize: 14 }}>
          Click your avatar {'>'} <strong>Job Preferences</strong> in the menu to add skills and preferences.
        </p>
      </div>
    )
  }

  if (jobs.length === 0) {
    return (
      <div className="container" style={{ paddingTop: 60 }}>
        <h1 style={{ fontSize: 32, marginBottom: 16 }}>Suggestions</h1>
        <p style={{ color: '#616161' }}>
          No matching jobs found based on your current preferences. Try adding more skills or lowering the match threshold.
        </p>
      </div>
    )
  }

  const scoreColor = (score: number) => {
    if (score >= 80) return 'var(--deep-green)'
    if (score >= 60) return 'var(--action-blue)'
    return 'var(--body-muted)'
  }

  const scoreBg = (score: number) => {
    if (score >= 80) return 'var(--pale-green)'
    if (score >= 60) return 'var(--pale-blue)'
    return 'var(--soft-stone)'
  }

  return (
    <div className="container" style={{ paddingTop: 32 }}>
      <div className="jobs-header">
        <div>
          <h1>Suggestions</h1>
          <p style={{ color: '#616161', marginTop: 8 }}>
            Top {jobs.length} jobs matched to your profile
          </p>
        </div>
        <Link to="/jobs" className="btn btn-outline">Browse All Jobs</Link>
      </div>

      <div className="job-grid">
        {jobs.map((job) => (
          <div key={job.id} className="job-card">
            <div className="job-card-header">
              <h3>{job.title}</h3>
              <span
                style={{
                  display: 'inline-flex',
                  padding: '2px 10px',
                  borderRadius: 9999,
                  fontSize: 12,
                  fontWeight: 600,
                  color: scoreColor(job.score),
                  background: scoreBg(job.score),
                  whiteSpace: 'nowrap',
                }}
              >
                {job.score}% match
              </span>
            </div>
            <div className="job-card-body">
              <p className="company">{job.companyName}</p>
              <p className="meta">
                <span>{job.location}</span>
                <span>{formatSalaryText(job.salary)}</span>
                {job.experienceRequired !== 'Not specified' && <span>{job.experienceRequired}</span>}
              </p>
              <p className="date">
                {new Date(job.postedDate).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}
              </p>
            </div>

            <div className="match-breakdown">
              {job.breakdown.matchedSkills.length > 0 && (
                <>
                  <h4>Skills Match</h4>
                  <div className="match-skills">
                    {job.breakdown.matchedSkills.slice(0, 5).map(s => (
                      <span key={s} className="match-skill matched">✓ {s.charAt(0).toUpperCase() + s.slice(1).toLowerCase()}</span>
                    ))}
                    {job.breakdown.missingSkills.slice(0, 3).map(s => (
                      <span key={s} className="match-skill missing">✗ {s.charAt(0).toUpperCase() + s.slice(1).toLowerCase()}</span>
                    ))}
                  </div>
                </>
              )}

              <h4 style={{ marginTop: 4 }}>Score Breakdown</h4>
              <div className="match-score-row">
                <div className="match-score-item">
                  <span className="match-score-label">Tech</span>
                  <div className="match-score-track">
                    <div className="match-score-fill tech" style={{ width: `${(job.breakdown.technologyScore / 40) * 100}%` }} />
                  </div>
                  <span className="match-score-value">{job.breakdown.technologyScore}/40</span>
                </div>
                <div className="match-score-item">
                  <span className="match-score-label">Level</span>
                  <div className="match-score-track">
                    <div className="match-score-fill level" style={{ width: `${(job.breakdown.seniorityScore / 20) * 100}%` }} />
                  </div>
                  <span className="match-score-value">{job.breakdown.seniorityScore}/20</span>
                </div>
                <div className="match-score-item">
                  <span className="match-score-label">Salary</span>
                  <div className="match-score-track">
                    <div className="match-score-fill salary" style={{ width: `${(job.breakdown.salaryScore / 15) * 100}%` }} />
                  </div>
                  <span className="match-score-value">{job.breakdown.salaryScore}/15</span>
                </div>
                <div className="match-score-item">
                  <span className="match-score-label">Location</span>
                  <div className="match-score-track">
                    <div className="match-score-fill location" style={{ width: `${(job.breakdown.locationScore / 15) * 100}%` }} />
                  </div>
                  <span className="match-score-value">{job.breakdown.locationScore}/15</span>
                </div>
                <div className="match-score-item">
                  <span className="match-score-label">Type</span>
                  <div className="match-score-track">
                    <div className="match-score-fill type" style={{ width: `${(job.breakdown.jobTypeScore / 10) * 100}%` }} />
                  </div>
                  <span className="match-score-value">{job.breakdown.jobTypeScore}/10</span>
                </div>
              </div>
            </div>

            <Link to={`/jobs/${job.id}`} className="btn btn-outline btn-sm" style={{ alignSelf: 'flex-start' }}>View Details</Link>
          </div>
        ))}
      </div>
    </div>
  )
}
