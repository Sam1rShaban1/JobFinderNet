import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import api from '../api/axios'
import { useAuth } from '../context/AuthContext'

interface Job {
  id: number
  title: string
  description: string
  companyName: string
  location: string
  jobType: string
  salary: string
  experienceRequired: string
  postedDate: string
  isActive: boolean
}

export default function JobDetails() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { user } = useAuth()
  const [job, setJob] = useState<Job | null>(null)
  const [message, setMessage] = useState('')

  useEffect(() => {
    api.get(`/jobs/${id}`).then((res) => setJob(res.data)).catch((err) => {
      console.error('Failed to fetch job:', err.response?.data || err.message)
    })
  }, [id])

  const handleApply = async () => {
    try {
      const res = await api.post(`/applications/${id}`)
      setMessage(res.data.message || 'Application submitted!')
    } catch (err: any) {
      setMessage(err.response?.data?.message || 'Application failed')
    }
  }

  if (!job) return <div className="container" style={{ paddingTop: 40 }}><p>Loading...</p></div>

  return (
    <div className="job-detail-page">
      <div className="container">
        <button onClick={() => navigate(-1)} className="btn btn-text" style={{ marginBottom: 24 }}>&larr; Back</button>

        <div className="job-detail">
          <div className="detail-header">
            <div style={{ display: 'flex', gap: 12, alignItems: 'center', marginBottom: 16 }}>
              <span className={`badge ${job.jobType.toLowerCase()}`}>{job.jobType}</span>
              <span className="micro">{job.experienceRequired}</span>
            </div>
            <h1>{job.title}</h1>
            <p className="company" style={{ fontSize: 18, marginTop: 12 }}>{job.companyName}</p>
          </div>

          <div className="detail-meta">
            <div className="detail-meta-item">
              <p className="micro">Location</p>
              <p>{job.location}</p>
            </div>
            <div className="detail-meta-item">
              <p className="micro">Salary</p>
              <p>{job.salary}</p>
            </div>
            <div className="detail-meta-item">
              <p className="micro">Type</p>
              <p>{job.jobType}</p>
            </div>
            <div className="detail-meta-item">
              <p className="micro">Posted</p>
              <p>{new Date(job.postedDate).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })}</p>
            </div>
          </div>

          <div className="detail-description">
            <h3>About this role</h3>
            <p>{job.description}</p>
          </div>

          {message && (
            <div className={`alert ${message.includes('failed') || message.includes('already') ? 'alert-error' : 'alert-success'}`}>
              {message}
            </div>
          )}
        </div>

        {job.isActive && (
          <div className="apply-band">
            <p className="micro" style={{ color: 'rgba(255,255,255,0.5)', marginBottom: 12 }}>Interested?</p>
            <h3>Apply for this position</h3>
            <p>Submit your application and take the next step in your career.</p>
            {user?.role === 'Applicant' ? (
              <button onClick={handleApply} className="btn btn-primary">Submit Application</button>
            ) : (
              <p className="micro" style={{ color: 'rgba(255,255,255,0.5)' }}>
                Sign in as a job seeker to apply
              </p>
            )}
          </div>
        )}
      </div>
    </div>
  )
}
