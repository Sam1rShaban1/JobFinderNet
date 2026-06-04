import { useState, useEffect } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { useAuth } from '@clerk/react'
import { useAppUser } from '../context/AppContext'
import api from '../api/axios'
import { SkeletonList } from '../components/Skeleton'
import HeartButton from '../components/HeartButton'

interface Job {
  id: number
  title: string
  companyName: string
  location: string
  jobType: string
  salary: string
  experienceRequired: string
  postedDate: string
  isActive: boolean
  isRemote?: boolean
}

interface MatchedJob extends Job {
  score: number
}

function formatSalaryText(salary: string): string {
  return salary.replace(/\$(\d+)\s*-\s*\$(\d+)/g, (_, a, b) => {
    const fmt = (n: number) => n >= 1000 ? `$${Math.round(n / 1000)}k` : `$${n}`
    return `${fmt(+a)} - ${fmt(+b)}`
  })
}

function ScoreBadge({ score }: { score: number }) {
  const color = score >= 80 ? 'var(--deep-green)' : score >= 60 ? 'var(--action-blue)' : 'var(--body-muted)'
  const bg = score >= 80 ? 'var(--pale-green)' : score >= 60 ? 'var(--pale-blue)' : 'var(--soft-stone)'
  return (
    <span style={{
      display: 'inline-flex',
      padding: '2px 10px',
      borderRadius: 9999,
      fontSize: 12,
      fontWeight: 600,
      color,
      background: bg,
    }}>
      {score}% match
    </span>
  )
}

export default function Jobs() {
  const [searchParams, setSearchParams] = useSearchParams()
  const [jobs, setJobs] = useState<Job[]>([])
  const [matchedJobs, setMatchedJobs] = useState<MatchedJob[]>([])
  const [search, setSearch] = useState(searchParams.get('search') || '')
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [savedIds, setSavedIds] = useState<Set<number>>(new Set())
  const { user } = useAppUser()
  const { isSignedIn } = useAuth()

  useEffect(() => {
    const fetchJobs = async () => {
      setLoading(true)
      setError('')
      try {
        const url = search
          ? `/jobs/search?query=${search}`
          : `/jobs?page=${page}&pageSize=12`
        const res = await api.get(url)
        if (search) {
          setJobs(res.data)
        } else {
          setJobs(res.data.items)
          setTotalPages(res.data.totalPages)
        }
      } catch (err: any) {
        setError('Failed to load jobs. Please try again.')
      } finally {
        setLoading(false)
      }
    }
    fetchJobs()
  }, [page, search])

  useEffect(() => {
    if (!isSignedIn) {
      setMatchedJobs([])
      return
    }
    const fetchMatched = async () => {
      try {
        const res = await api.get('/profile/matched?limit=6')
        setMatchedJobs(res.data)
      } catch {
        // not critical
      }
    }
    fetchMatched()
  }, [isSignedIn])

  useEffect(() => {
    if (!isSignedIn) return
    api.get('/savedjobs/ids').then((res) => setSavedIds(new Set(res.data))).catch(() => {})
  }, [isSignedIn])

  return (
    <div className="container">
      <div className="jobs-header">
        <h1>Jobs</h1>
        {user?.role === 'Employer' && (
          <Link to="/create-job" className="btn btn-primary">Post a Job</Link>
        )}
      </div>

      <div className="search-bar">
        <input
          type="text"
          placeholder="Search jobs by title, company, or description..."
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1) }}
        />
      </div>

      {error && (
        <div className="alert alert-error" style={{ marginBottom: 24, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <span>{error}</span>
          <button className="btn btn-outline btn-sm" onClick={() => setPage(1)}>Retry</button>
        </div>
      )}

      {loading ? (
        <SkeletonList count={6} />
      ) : (
        <>
          {matchedJobs.length > 0 && (
            <div style={{ marginBottom: 40 }}>
              <h2 style={{ fontSize: 24, marginBottom: 16 }}>Matched for You</h2>
              <div className="job-grid">
                {matchedJobs.map((job) => (
              <div key={`matched-${job.id}`} className="job-card">
                <div className="job-card-header">
                  <h3>{job.title}</h3>
                  <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
                    <ScoreBadge score={job.score} />
                    {isSignedIn && (
                      <HeartButton
                        jobId={job.id}
                        initialSaved={savedIds.has(job.id)}
                        onChange={(s) => setSavedIds(prev => {
                          const next = new Set(prev)
                          if (s) next.add(job.id); else next.delete(job.id)
                          return next
                        })}
                      />
                    )}
                  </div>
                </div>
                    <div className="job-card-body">
                      <p className="company">{job.companyName}</p>
                      <p className="meta">
                        <span>{job.location}</span>
                        <span>{formatSalaryText(job.salary)}</span>
                        {job.experienceRequired !== 'Not specified' && <span>{job.experienceRequired}</span>}
                      </p>
                      <p className="date">{new Date(job.postedDate).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}</p>
                    </div>
                    <Link to={`/jobs/${job.id}`} className="btn btn-outline btn-sm" style={{ alignSelf: 'flex-start' }}>View Details</Link>
                  </div>
                ))}
              </div>
            </div>
          )}

          <div className="job-grid">
            {jobs.map((job) => (
              <div key={job.id} className="job-card">
                <div className="job-card-header">
                  <h3>{job.title}</h3>
                  <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
                    <span className={`badge ${job.jobType.toLowerCase()}`}>{job.jobType}</span>
                    {isSignedIn && (
                      <HeartButton
                        jobId={job.id}
                        initialSaved={savedIds.has(job.id)}
                        onChange={(s) => setSavedIds(prev => {
                          const next = new Set(prev)
                          if (s) next.add(job.id); else next.delete(job.id)
                          return next
                        })}
                      />
                    )}
                  </div>
                </div>
                <div className="job-card-body">
                  <p className="company">{job.companyName}</p>
                  <p className="meta">
                    <span>{job.location}</span>
                    <span>{formatSalaryText(job.salary)}</span>
                    {job.experienceRequired !== 'Not specified' && <span>{job.experienceRequired}</span>}
                  </p>
                  <p className="date">{new Date(job.postedDate).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}</p>
                </div>
                <Link to={`/jobs/${job.id}`} className="btn btn-outline btn-sm" style={{ alignSelf: 'flex-start' }}>View Details</Link>
              </div>
            ))}
            {jobs.length === 0 && <p className="no-results">No jobs found.</p>}
          </div>
        </>
      )}

      {!search && totalPages > 1 && (
        <div className="pagination">
          <button disabled={page <= 1} onClick={() => setPage(page - 1)}>Previous</button>
          <span>Page {page} of {totalPages}</span>
          <button disabled={page >= totalPages} onClick={() => setPage(page + 1)}>Next</button>
        </div>
      )}
    </div>
  )
}
