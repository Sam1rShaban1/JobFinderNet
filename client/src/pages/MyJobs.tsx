import { useState, useEffect } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import toast from 'react-hot-toast'
import { myJobsApi } from '../api/axios'

interface Job {
  id: number
  title: string
  companyName: string
  location: string
  jobType: string
  salary: string
  createdAt: string
  isActive: boolean
  applicationCount?: number
}

export default function MyJobs() {
  const navigate = useNavigate()
  const [jobs, setJobs] = useState<Job[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const fetchJobs = () => {
    setLoading(true)
    setError('')
    myJobsApi.list()
      .then(res => setJobs(res.data))
      .catch(err => setError(err.response?.data?.message || 'Failed to load jobs'))
      .finally(() => setLoading(false))
  }

  useEffect(() => { fetchJobs() }, [])

  const toggleActive = async (id: number) => {
    try {
      await myJobsApi.toggle(id)
      setJobs(jobs.map(j => j.id === id ? { ...j, isActive: !j.isActive } : j))
      toast.success('Job status toggled')
    } catch {
      toast.error('Failed to toggle status')
    }
  }

  const deleteJob = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this job listing?')) return
    try {
      await myJobsApi.delete(id)
      setJobs(jobs.filter(j => j.id !== id))
      toast.success('Job deleted')
    } catch {
      toast.error('Failed to delete job')
    }
  }

  if (loading) return <div className="container" style={{ padding: 60, color: '#888' }}>Loading...</div>
  if (error) return <div className="container" style={{ padding: 60 }}><div className="alert alert-error">{error}<br /><button className="btn btn-outline btn-sm" style={{ marginTop: 12 }} onClick={fetchJobs}>Retry</button></div></div>

  return (
    <div className="container" style={{ paddingTop: 40 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 32 }}>
        <div>
          <p className="micro" style={{ marginBottom: 8 }}>Employer Dashboard</p>
          <h2>My Jobs</h2>
        </div>
        <Link to="/create-job" className="btn btn-primary">Post New Job</Link>
      </div>

      {jobs.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '80px 0', color: '#888' }}>
          <p>No job listings yet.</p>
          <Link to="/create-job" className="btn btn-primary" style={{ marginTop: 16, display: 'inline-block' }}>
            Post Your First Job
          </Link>
        </div>
      ) : (
        <table className="my-jobs-table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Location</th>
              <th>Type</th>
              <th>Salary</th>
              <th>Applications</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {jobs.map(job => (
              <tr key={job.id}>
                <td>
                  <Link to={`/jobs/${job.id}`} style={{ fontWeight: 600, color: 'var(--color-primary)' }}>
                    {job.title}
                  </Link>
                  <div className="micro" style={{ color: '#888', marginTop: 2 }}>{job.companyName}</div>
                </td>
                <td>{job.location}</td>
                <td>{job.jobType}</td>
                <td>{job.salary}</td>
                <td>{job.applicationCount ?? 0}</td>
                <td>
                  <span className={`badge ${job.isActive ? 'role-applicant' : 'role-admin'}`}
                    style={{ background: job.isActive ? '#e8f5e9' : '#fce4ec', color: job.isActive ? '#2e7d32' : '#c62828' }}>
                    {job.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td>
                  <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                    <button className="btn btn-outline btn-xs" onClick={() => navigate(`/edit-job/${job.id}`)}>Edit</button>
                    <button className="btn btn-outline btn-xs" onClick={() => toggleActive(job.id)}>
                      {job.isActive ? 'Deactivate' : 'Activate'}
                    </button>
                    <button className="btn btn-outline btn-xs" style={{ color: '#c62828', borderColor: '#ef9a9a' }} onClick={() => deleteJob(job.id)}>Delete</button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}