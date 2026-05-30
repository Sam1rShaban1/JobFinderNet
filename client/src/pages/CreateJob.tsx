import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../api/axios'

const SENIORITY_OPTIONS = ['', 'Junior', 'Mid-Level', 'Senior', 'Lead', 'Manager', 'Director']
const JOB_TYPE_OPTIONS = ['Full-time', 'Part-time', 'Contract', 'Internship', 'Temporary']
const EXPERIENCE_OPTIONS = ['Entry Level', '1-3 years', '3-5 years', '5+ years', '10+ years']
const WORK_ARRANGEMENTS = ['', 'On-Site', 'Hybrid', 'Remote']
const CURRENCY_OPTIONS = ['', 'USD', 'EUR', 'GBP', 'CAD', 'AUD']
const SALARY_PERIODS = ['', 'Yearly', 'Monthly', 'Weekly', 'Hourly']

export default function CreateJob() {
  const navigate = useNavigate()
  const [form, setForm] = useState({
    title: '', description: '', companyName: '',
    employerLogo: '', employerWebsite: '',
    location: '', city: '', state: '', country: '',
    jobType: 'Full-time', salary: '',
    salaryMin: '', salaryMax: '', salaryCurrency: '', salaryPeriod: '',
    experienceRequired: 'Entry Level', requiredExperienceYears: '',
    seniorityLevel: '', industry: '', jobFunction: '',
    workArrangement: '', applyLink: '',
    isRemote: false,
    educationRequired: '', contractDuration: '',
    requiredTechnologies: '', preferredTechnologies: '',
    softSkills: '', benefits: '', methodologies: '',
    highlightsQualifications: '', highlightsResponsibilities: '', highlightsBenefits: '',
  })
  const [error, setError] = useState('')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    try {
      const body = {
        ...form,
        salaryMin: form.salaryMin ? Number(form.salaryMin) : null,
        salaryMax: form.salaryMax ? Number(form.salaryMax) : null,
        requiredExperienceYears: form.requiredExperienceYears ? Number(form.requiredExperienceYears) : null,
        requiredTechnologies: form.requiredTechnologies ? form.requiredTechnologies.split(',').map((s: string) => s.trim()).filter(Boolean) : [],
        preferredTechnologies: form.preferredTechnologies ? form.preferredTechnologies.split(',').map((s: string) => s.trim()).filter(Boolean) : [],
        softSkills: form.softSkills ? form.softSkills.split(',').map((s: string) => s.trim()).filter(Boolean) : [],
        benefits: form.benefits ? form.benefits.split(',').map((s: string) => s.trim()).filter(Boolean) : [],
        methodologies: form.methodologies ? form.methodologies.split(',').map((s: string) => s.trim()).filter(Boolean) : [],
      }
      await api.post('/jobs', body)
      navigate('/jobs')
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create job')
    }
  }

  const update = (field: string) => (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) =>
    setForm({ ...form, [field]: e.target.value })

  const toggleRemote = () => setForm({ ...form, isRemote: !form.isRemote })

  const tagHint = 'Comma-separated list'

  return (
    <div className="form-page" style={{ maxWidth: 800 }}>
      <p className="micro" style={{ marginBottom: 12 }}>Post a new position</p>
      <h2>Create Job Listing</h2>
      {error && <div className="alert alert-error">{error}</div>}
      <form onSubmit={handleSubmit}>
        <h4 style={{ margin: '24px 0 12px' }}>Basic Info</h4>

        <div className="form-group">
          <label>Job Title</label>
          <input type="text" value={form.title} onChange={update('title')} required />
        </div>

        <div className="form-group">
          <label>Company Name</label>
          <input type="text" value={form.companyName} onChange={update('companyName')} required />
        </div>

        <div className="form-row">
          <div className="form-group">
            <label>Employer Logo URL</label>
            <input type="url" value={form.employerLogo} onChange={update('employerLogo')} placeholder="https://..." />
          </div>
          <div className="form-group">
            <label>Employer Website</label>
            <input type="url" value={form.employerWebsite} onChange={update('employerWebsite')} placeholder="https://..." />
          </div>
        </div>

        <div className="form-group">
          <label>Description</label>
          <textarea value={form.description} onChange={update('description')} required rows={6} />
        </div>

        <div className="form-group">
          <label>Apply Link (URL)</label>
          <input type="url" value={form.applyLink} onChange={update('applyLink')} placeholder="https://..." />
        </div>

        <h4 style={{ margin: '24px 0 12px' }}>Location</h4>

        <div className="form-group">
          <label>Full Location</label>
          <input type="text" value={form.location} onChange={update('location')} required placeholder="e.g. San Francisco, CA" />
        </div>

        <div className="form-row">
          <div className="form-group">
            <label>City</label>
            <input type="text" value={form.city} onChange={update('city')} />
          </div>
          <div className="form-group">
            <label>State</label>
            <input type="text" value={form.state} onChange={update('state')} />
          </div>
          <div className="form-group">
            <label>Country</label>
            <input type="text" value={form.country} onChange={update('country')} />
          </div>
        </div>

        <div className="form-group" style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          <label style={{ margin: 0 }}>Remote Position</label>
          <input type="checkbox" checked={form.isRemote} onChange={toggleRemote} style={{ width: 20, height: 20 }} />
        </div>

        <h4 style={{ margin: '24px 0 12px' }}>Job Details</h4>

        <div className="form-row">
          <div className="form-group">
            <label>Job Type</label>
            <select value={form.jobType} onChange={update('jobType')}>
              {JOB_TYPE_OPTIONS.map(o => <option key={o}>{o}</option>)}
            </select>
          </div>
          <div className="form-group">
            <label>Seniority Level</label>
            <select value={form.seniorityLevel} onChange={update('seniorityLevel')}>
              {SENIORITY_OPTIONS.map(o => <option key={o} value={o}>{o || 'Any'}</option>)}
            </select>
          </div>
          <div className="form-group">
            <label>Work Arrangement</label>
            <select value={form.workArrangement} onChange={update('workArrangement')}>
              {WORK_ARRANGEMENTS.map(o => <option key={o} value={o}>{o || 'Any'}</option>)}
            </select>
          </div>
        </div>

        <div className="form-row">
          <div className="form-group">
            <label>Industry</label>
            <input type="text" value={form.industry} onChange={update('industry')} />
          </div>
          <div className="form-group">
            <label>Job Function</label>
            <input type="text" value={form.jobFunction} onChange={update('jobFunction')} />
          </div>
        </div>

        <h4 style={{ margin: '24px 0 12px' }}>Compensation</h4>

        <div className="form-row">
          <div className="form-group">
            <label>Salary (display text)</label>
            <input type="text" value={form.salary} onChange={update('salary')} required placeholder="e.g. $80,000 - $120,000" />
          </div>
        </div>

        <div className="form-row">
          <div className="form-group">
            <label>Salary Min</label>
            <input type="number" value={form.salaryMin} onChange={update('salaryMin')} placeholder="80000" />
          </div>
          <div className="form-group">
            <label>Salary Max</label>
            <input type="number" value={form.salaryMax} onChange={update('salaryMax')} placeholder="120000" />
          </div>
          <div className="form-group">
            <label>Currency</label>
            <select value={form.salaryCurrency} onChange={update('salaryCurrency')}>
              {CURRENCY_OPTIONS.map(o => <option key={o} value={o}>{o || 'Select'}</option>)}
            </select>
          </div>
          <div className="form-group">
            <label>Period</label>
            <select value={form.salaryPeriod} onChange={update('salaryPeriod')}>
              {SALARY_PERIODS.map(o => <option key={o} value={o}>{o || 'Select'}</option>)}
            </select>
          </div>
        </div>

        <h4 style={{ margin: '24px 0 12px' }}>Experience & Requirements</h4>

        <div className="form-row">
          <div className="form-group">
            <label>Experience Required</label>
            <select value={form.experienceRequired} onChange={update('experienceRequired')}>
              {EXPERIENCE_OPTIONS.map(o => <option key={o}>{o}</option>)}
            </select>
          </div>
          <div className="form-group">
            <label>Required Experience (years)</label>
            <input type="number" value={form.requiredExperienceYears} onChange={update('requiredExperienceYears')} placeholder="3" />
          </div>
        </div>

        <div className="form-row">
          <div className="form-group">
            <label>Education Required</label>
            <input type="text" value={form.educationRequired} onChange={update('educationRequired')} placeholder="e.g. Bachelor's Degree" />
          </div>
          <div className="form-group">
            <label>Contract Duration</label>
            <input type="text" value={form.contractDuration} onChange={update('contractDuration')} placeholder="e.g. 6 months" />
          </div>
        </div>

        <h4 style={{ margin: '24px 0 12px' }}>Technologies & Skills</h4>

        <div className="form-row">
          <div className="form-group">
            <label>Required Technologies</label>
            <input type="text" value={form.requiredTechnologies} onChange={update('requiredTechnologies')} placeholder={tagHint} />
          </div>
          <div className="form-group">
            <label>Preferred Technologies</label>
            <input type="text" value={form.preferredTechnologies} onChange={update('preferredTechnologies')} placeholder={tagHint} />
          </div>
        </div>

        <div className="form-row">
          <div className="form-group">
            <label>Soft Skills</label>
            <input type="text" value={form.softSkills} onChange={update('softSkills')} placeholder={tagHint} />
          </div>
          <div className="form-group">
            <label>Benefits</label>
            <input type="text" value={form.benefits} onChange={update('benefits')} placeholder={tagHint} />
          </div>
        </div>

        <div className="form-group">
          <label>Methodologies</label>
          <input type="text" value={form.methodologies} onChange={update('methodologies')} placeholder={tagHint} />
        </div>

        <h4 style={{ margin: '24px 0 12px' }}>Highlights</h4>

        <div className="form-group">
          <label>Qualifications</label>
          <textarea value={form.highlightsQualifications} onChange={update('highlightsQualifications')} rows={3} />
        </div>

        <div className="form-group">
          <label>Responsibilities</label>
          <textarea value={form.highlightsResponsibilities} onChange={update('highlightsResponsibilities')} rows={3} />
        </div>

        <div className="form-group">
          <label>Benefits Highlights</label>
          <textarea value={form.highlightsBenefits} onChange={update('highlightsBenefits')} rows={3} />
        </div>

        <div style={{ marginTop: 32 }}>
          <button type="submit" className="btn btn-primary btn-full">Post Job</button>
        </div>
      </form>
    </div>
  )
}
