import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import api from '../api/axios'
import { useAuth } from '../context/AuthContext'

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
}

export default function Jobs() {
  const [jobs, setJobs] = useState<Job[]>([])
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const { user } = useAuth()

  useEffect(() => {
    const fetchJobs = async () => {
      try {
        const url = search
          ? `/jobs/search?query=${search}`
          : `/jobs?page=${page}&pageSize=10`
        const res = await api.get(url)
        if (search) {
          setJobs(res.data)
        } else {
          setJobs(res.data.items)
          setTotalPages(res.data.totalPages)
        }
      } catch (err: any) {
        console.error('Failed to fetch jobs:', err.response?.data || err.message)
      }
    }
    fetchJobs()
  }, [page, search])

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

      <div className="job-grid">
        {jobs.map((job) => (
          <div key={job.id} className="job-card">
            <div className="job-card-header">
              <h3>{job.title}</h3>
              <span className={`badge ${job.jobType.toLowerCase()}`}>{job.jobType}</span>
            </div>
            <div className="job-card-body">
              <p className="company">{job.companyName}</p>
              <p className="meta">
                <span>{job.location}</span>
                <span>{job.salary}</span>
                <span>{job.experienceRequired}</span>
              </p>
              <p className="date">{new Date(job.postedDate).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}</p>
            </div>
            <Link to={`/jobs/${job.id}`} className="btn btn-outline btn-sm" style={{ alignSelf: 'flex-start' }}>View Details</Link>
          </div>
        ))}
        {jobs.length === 0 && <p className="no-results">No jobs found.</p>}
      </div>

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
