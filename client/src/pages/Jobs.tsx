import { useState, useEffect, useRef } from 'react'
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
      whiteSpace: 'nowrap',
    }}>
      {score}% match
    </span>
  )
}

const JOB_TYPES = ['Full-time', 'Part-time', 'Contract', 'Internship']

export default function Jobs() {
  const [searchParams] = useSearchParams()
  const [jobs, setJobs] = useState<Job[]>([])
  const [matchedJobs, setMatchedJobs] = useState<MatchedJob[]>([])
  const [inputValue, setInputValue] = useState(searchParams.get('search') || '')
  const [search, setSearch] = useState(searchParams.get('search') || '')
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [savedIds, setSavedIds] = useState<Set<number>>(new Set())
  const [remoteOnly, setRemoteOnly] = useState(false)
  const [jobTypeFilter, setJobTypeFilter] = useState('')
  const [sortBy, setSortBy] = useState('newest')
  const [hasProfile, setHasProfile] = useState(true)
  const { user } = useAppUser()
  const { isSignedIn } = useAuth()
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const handleSearchChange = (value: string) => {
    setInputValue(value)
    if (debounceRef.current) clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(() => {
      setSearch(value)
      setPage(1)
    }, 300)
  }

  useEffect(() => {
    const fetchJobs = async () => {
      setLoading(true)
      setError('')
      try {
        const url = search
          ? `/jobs/search?query=${encodeURIComponent(search)}`
          : `/jobs?page=${page}&pageSize=12`
        const res = await api.get(url)
        if (search) {
          setJobs(res.data)
        } else {
          setJobs(res.data.items)
          setTotalPages(res.data.totalPages)
        }
      } catch {
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
    api.get('/profile/matched?limit=6')
      .then((res) => { setMatchedJobs(res.data); setHasProfile(true) })
      .catch((err) => { if (err.response?.status === 404) setHasProfile(false) })
  }, [isSignedIn])

  useEffect(() => {
    if (!isSignedIn) return
    api.get('/savedjobs/ids').then((res) => setSavedIds(new Set(res.data))).catch(() => {})
  }, [isSignedIn])

  const filteredJobs = jobs
    .filter(j => !remoteOnly || j.isRemote)
    .filter(j => !jobTypeFilter || j.jobType.toLowerCase() === jobTypeFilter.toLowerCase())
    .sort((a, b) => {
      if (sortBy === 'newest') return new Date(b.postedDate).getTime() - new Date(a.postedDate).getTime()
      if (sortBy === 'oldest') return new Date(a.postedDate).getTime() - new Date(b.postedDate).getTime()
      return 0
    })

  const showResumeBanner = isSignedIn && !hasProfile && user?.role !== 'Employer' && user?.role !== 'Admin'

  const savedIdsHandler = (jobId: number) => (saved: boolean) =>
    setSavedIds(prev => { const n = new Set(prev); saved ? n.add(jobId) : n.delete(jobId); return n })

  return (
    <div className="container">
      <div className="jobs-header">
        <h1>Jobs</h1>
        {user?.role === 'Employer' && (
          <Link to="/create-job" className="btn btn-primary">Post a Job</Link>
        )}
      </div>

      {showResumeBanner && (
        <div className="resume-banner">
          <div>
            <strong>Unlock AI job matching</strong>
            <span> — upload your resume to see personalized match scores on every job.</span>
          </div>
          <Link to="/suggestions" className="btn btn-primary btn-sm">Set Up Profile</Link>
        </div>
      )}

      <div className="search-bar">
        <input
          type="text"
          placeholder="Search jobs by title, company, or description..."
          value={inputValue}
          onChange={(e) => handleSearchChange(e.target.value)}
        />
      </div>

      <div className="filter-chips">
        <button
          className={`filter-chip${remoteOnly ? ' active' : ''}`}
          onClick={() => setRemoteOnly(v => !v)}
        >
          Remote
        </button>
        {JOB_TYPES.map(type => (
          <button
            key={type}
            className={`filter-chip${jobTypeFilter === type ? ' active' : ''}`}
            onClick={() => setJobTypeFilter(prev => prev === type ? '' : type)}
          >
            {type}
          </button>
        ))}
      </div>

      <div className="sort-toolbar">
        <span className="micro">Sort by</span>
        <select
          value={sortBy}
          onChange={(e) => setSortBy(e.target.value)}
          className="sort-select"
        >
          <option value="newest">Newest first</option>
          <option value="oldest">Oldest first</option>
        </select>
        {(remoteOnly || jobTypeFilter) && (
          <span className="micro" style={{ marginLeft: 'auto' }}>
            {filteredJobs.length} of {jobs.length} jobs
          </span>
        )}
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
            <div className="matched-section">
              <h2 className="matched-section-title">Matched for You</h2>
              <div className="job-grid">
                {matchedJobs.map((job) => (
                  <Link key={`matched-${job.id}`} to={`/jobs/${job.id}`} className="job-card">
                    <div className="job-card-header">
                      <h3>{job.title}</h3>
                      <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
                        <ScoreBadge score={job.score} />
                        {isSignedIn && (
                          <HeartButton
                            jobId={job.id}
                            initialSaved={savedIds.has(job.id)}
                            onChange={savedIdsHandler(job.id)}
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
                  </Link>
                ))}
              </div>
            </div>
          )}

          <div className="job-grid">
            {filteredJobs.map((job) => (
              <Link key={job.id} to={`/jobs/${job.id}`} className="job-card">
                <div className="job-card-header">
                  <h3>{job.title}</h3>
                  <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
                    <span className={`badge ${job.jobType.toLowerCase()}`}>{job.jobType}</span>
                    {isSignedIn && (
                      <HeartButton
                        jobId={job.id}
                        initialSaved={savedIds.has(job.id)}
                        onChange={savedIdsHandler(job.id)}
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
              </Link>
            ))}
            {filteredJobs.length === 0 && (
              <div className="empty-state">
                <div className="empty-state-icon">&#9670;</div>
                <h3>No jobs found</h3>
                <p>
                  {search
                    ? `No results for "${search}".`
                    : 'Try adjusting your filters.'}
                </p>
                {(search || remoteOnly || jobTypeFilter) && (
                  <button
                    className="btn btn-outline"
                    onClick={() => {
                      handleSearchChange('')
                      setRemoteOnly(false)
                      setJobTypeFilter('')
                    }}
                  >
                    Clear filters
                  </button>
                )}
              </div>
            )}
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
