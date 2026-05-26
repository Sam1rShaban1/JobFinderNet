import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../api/axios'

export default function CreateJob() {
  const navigate = useNavigate()
  const [form, setForm] = useState({
    title: '', description: '', companyName: '', location: '',
    jobType: 'Full-time', salary: '', experienceRequired: 'Entry Level'
  })
  const [error, setError] = useState('')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    try {
      await api.post('/jobs', form)
      navigate('/jobs')
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create job')
    }
  }

  const update = (field: string) => (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) =>
    setForm({ ...form, [field]: e.target.value })

  return (
    <div className="form-page">
      <p className="micro" style={{ marginBottom: 12 }}>Post a new position</p>
      <h2>Create Job Listing</h2>
      {error && <div className="alert alert-error">{error}</div>}
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label>Job Title</label>
          <input type="text" value={form.title} onChange={update('title')} required />
        </div>
        <div className="form-group">
          <label>Company Name</label>
          <input type="text" value={form.companyName} onChange={update('companyName')} required />
        </div>
        <div className="form-group">
          <label>Description</label>
          <textarea value={form.description} onChange={update('description')} required />
        </div>
        <div className="form-row">
          <div className="form-group">
            <label>Location</label>
            <input type="text" value={form.location} onChange={update('location')} required />
          </div>
          <div className="form-group">
            <label>Job Type</label>
            <select value={form.jobType} onChange={update('jobType')}>
              <option>Full-time</option><option>Part-time</option><option>Contract</option><option>Internship</option>
            </select>
          </div>
        </div>
        <div className="form-row">
          <div className="form-group">
            <label>Salary</label>
            <input type="text" value={form.salary} onChange={update('salary')} required />
          </div>
          <div className="form-group">
            <label>Experience Required</label>
            <select value={form.experienceRequired} onChange={update('experienceRequired')}>
              <option>Entry Level</option><option>1-3 years</option><option>3-5 years</option><option>5+ years</option>
            </select>
          </div>
        </div>
        <button type="submit" className="btn btn-primary btn-full">Post Job</button>
      </form>
    </div>
  )
}
