import { useState, useEffect } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import { useUser } from '@clerk/react'
import { useAppUser } from '../context/AppContext'
import toast from 'react-hot-toast'
import api from '../api/axios'
import { SkeletonDetails } from '../components/Skeleton'
import HeartButton from '../components/HeartButton'

interface SimilarJob {
  id: number
  title: string
  companyName: string
  location: string
  jobType: string
  salary: string
  postedDate: string
}

interface Job {
  id: number
  title: string
  description: string
  companyName: string
  employerLogo: string | null
  employerWebsite: string | null
  jobPublisher: string | null
  location: string
  city: string | null
  state: string | null
  country: string | null
  latitude: number | null
  longitude: number | null
  jobType: string
  salary: string
  salaryMin: number | null
  salaryMax: number | null
  salaryCurrency: string | null
  salaryPeriod: string | null
  experienceRequired: string
  requiredExperienceYears: number | null
  seniorityLevel: string | null
  industry: string | null
  jobFunction: string | null
  workArrangement: string | null
  externalJobId: string | null
  applyLink: string | null
  isRemote: boolean
  isActive: boolean
  postedAtTimestamp: number | null
  postedDate: string
  hasManagementResponsibilities: boolean | null
  isAiMlInvolved: boolean | null
  educationRequired: string | null
  contractDuration: string | null
  requiredTechnologies: string[]
  preferredTechnologies: string[]
  softSkills: string[]
  benefits: string[]
  methodologies: string[]
  highlightsQualifications: string | null
  highlightsResponsibilities: string | null
  highlightsBenefits: string | null
  source: string | null
  sourceUrl: string | null
}

function titleCase(str: string): string {
  return str.split('_').map(w => w.charAt(0).toUpperCase() + w.slice(1).toLowerCase()).join(' ')
}

function siteNameFromUrl(url: string): string {
  try {
    const host = new URL(url).hostname.replace('www.', '')
    const parts = host.split('.')
    const name = parts[0]
    return name.charAt(0).toUpperCase() + name.slice(1)
  } catch {
    return 'Company Site'
  }
}

function formatSalary(job: Job): string {
  if (job.salaryMin != null && job.salaryMax != null) {
    const fmt = (n: number) => n >= 1000 ? `$${Math.round(n / 1000)}k` : `$${n}`
    const period = job.salaryPeriod ? `/${job.salaryPeriod}` : ''
    return `${fmt(job.salaryMin)} - ${fmt(job.salaryMax)}${period}`
  }
  return job.salary || 'Not specified'
}

export default function JobDetails() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { isSignedIn } = useUser()
  const { user: appUser } = useAppUser()
  const [job, setJob] = useState<Job | null>(null)
  const [message, setMessage] = useState('')
  const [showPrompt, setShowPrompt] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [applied, setApplied] = useState(false)
  const [isSaved, setIsSaved] = useState(false)
  const [similarJobs, setSimilarJobs] = useState<SimilarJob[]>([])
  const [coverLetter, setCoverLetter] = useState(() =>
    localStorage.getItem('coverLetter') || ''
  )

  useEffect(() => {
    setLoading(true)
    setError('')
    api.get(`/jobs/${id}`).then((res) => {
      setJob(res.data)
      setLoading(false)
    }).catch((err) => {
      setError('Failed to load job details. Please try again.')
      setLoading(false)
    })

    if (isSignedIn) {
      api.get('/savedjobs/ids').then((res) => {
        setIsSaved(res.data.includes(Number(id)))
      }).catch(() => {})
    }

    api.get(`/jobs/${id}/similar`).then((res) => {
      setSimilarJobs(res.data)
    }).catch(() => {})
  }, [id, isSignedIn])

  const handleExternalApply = () => {
    if (job?.applyLink) window.open(job.applyLink, '_blank', 'noopener')
    setShowPrompt(true)
  }

  const handleConfirmApply = async () => {
    setSubmitting(true)
    try {
      const res = await api.post(`/applications/${id}`, { coverLetter: coverLetter || undefined })
      toast.success('Application submitted!')
      setMessage(res.data.message || 'Application submitted!')
      setApplied(true)
      if (coverLetter) localStorage.removeItem('coverLetter')
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Application failed')
      setMessage(err.response?.data?.message || 'Application failed')
    } finally {
      setSubmitting(false)
      setShowPrompt(false)
    }
  }

  const handleSkip = () => {
    setShowPrompt(false)
  }

  if (loading) return <SkeletonDetails />

  if (error) {
    return (
      <div className="container" style={{ paddingTop: 60, textAlign: 'center' }}>
        <p style={{ color: '#616161', marginBottom: 16 }}>{error}</p>
        <Link to="/jobs" className="btn btn-outline">Browse Jobs</Link>
      </div>
    )
  }

  if (!job) {
    return (
      <div className="container" style={{ paddingTop: 60, textAlign: 'center' }}>
        <p style={{ color: '#616161', marginBottom: 16 }}>Job not found.</p>
        <Link to="/jobs" className="btn btn-outline">Browse Jobs</Link>
      </div>
    )
  }

  const descLines = job.description.split('\n')

  return (
    <div className="job-detail-page">
      <div className="container">
        <button onClick={() => navigate(-1)} className="btn btn-text" style={{ marginBottom: 24 }}>&larr; Back</button>

        <div className="job-detail">
          <div className="detail-header">
            <div style={{ display: 'flex', gap: 12, alignItems: 'center', marginBottom: 16, flexWrap: 'wrap' }}>
              <span className={`badge ${job.jobType.toLowerCase()}`}>{job.jobType}</span>
              {job.workArrangement && (
                <span className="badge badge-remote" style={{ background: 'var(--pale-green)', color: 'var(--deep-green)' }}>
                  {job.workArrangement}
                </span>
              )}
              {job.seniorityLevel && <span className="micro">{job.seniorityLevel}</span>}
              {job.experienceRequired && job.experienceRequired !== 'Not specified' && <span className="micro">{job.experienceRequired}</span>}
              {isSignedIn && (
                <HeartButton
                  jobId={job.id}
                  initialSaved={isSaved}
                  onChange={setIsSaved}
                />
              )}
            </div>
            <h1>{job.title}</h1>
            <p className="company" style={{ fontSize: 28, marginTop: 12, display: 'flex', alignItems: 'center', gap: 10 }}>
              {job.employerLogo && (
                <img src={job.employerLogo} alt="" style={{ width: 32, height: 32, borderRadius: 6 }} />
              )}
              {job.companyName}
            </p>
          </div>

          <div className="detail-meta">
            <div className="detail-meta-item">
              <p className="micro">Location</p>
              <p>{job.isRemote ? 'Remote' : [job.city, job.state, job.country].filter(Boolean).join(', ') || job.location}</p>
            </div>
            <div className="detail-meta-item">
              <p className="micro">Salary</p>
              <p>{formatSalary(job)}</p>
            </div>
            <div className="detail-meta-item">
              <p className="micro">Type</p>
              <p>{job.jobType}</p>
            </div>
            <div className="detail-meta-item">
              <p className="micro">Posted</p>
              <p>{new Date(job.postedDate).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })}</p>
            </div>
            {job.industry && (
              <div className="detail-meta-item">
                <p className="micro">Industry</p>
                <p>{job.industry}</p>
              </div>
            )}
            {job.educationRequired && (
              <div className="detail-meta-item">
                <p className="micro">Education</p>
                <p>{job.educationRequired}</p>
              </div>
            )}
            {job.jobFunction && (
              <div className="detail-meta-item">
                <p className="micro">Function</p>
                <p>{job.jobFunction}</p>
              </div>
            )}
            {job.contractDuration && (
              <div className="detail-meta-item">
                <p className="micro">Duration</p>
                <p>{job.contractDuration}</p>
              </div>
            )}
          </div>

          {job.requiredTechnologies.length > 0 && (
            <div className="detail-section">
              <h3>Required Technologies</h3>
              <div className="tag-list">
                {job.requiredTechnologies.map((t, i) => <span key={i} className="tag">{t}</span>)}
              </div>
            </div>
          )}

          {job.preferredTechnologies.length > 0 && (
            <div className="detail-section">
              <h3>Preferred Technologies</h3>
              <div className="tag-list">
                {job.preferredTechnologies.map((t, i) => <span key={i} className="tag tag-outline">{t}</span>)}
              </div>
            </div>
          )}

          {job.softSkills.length > 0 && (
            <div className="detail-section">
              <h3>Soft Skills</h3>
              <div className="tag-list">
                {job.softSkills.map((s, i) => <span key={i} className="tag tag-soft">{s}</span>)}
              </div>
            </div>
          )}

          {job.benefits.length > 0 && (
            <div className="detail-section">
              <h3>Benefits</h3>
              <div className="tag-list">
                {job.benefits.map((b, i) => <span key={i} className="tag tag-benefit">{titleCase(b)}</span>)}
              </div>
            </div>
          )}

          <div className="detail-section">
            <h3>About this role</h3>
            <div className="job-description">
              {descLines.map((line, i) => {
                const trimmed = line.trim()
                if (!trimmed) return <br key={i} />
                if (/^[•\-\*]\s/.test(trimmed)) return <p key={i} className="desc-bullet">{trimmed.replace(/^[•\-\*]\s+/, '')}</p>
                if (/^\d+[\.\)]/.test(trimmed)) return <p key={i} className="desc-numbered">{trimmed}</p>
                if (/^[A-Z][A-Za-z\s]+\s*:/.test(trimmed)) return <p key={i} className="desc-label">{trimmed}</p>
                return <p key={i} className="desc-paragraph">{trimmed}</p>
              })}
            </div>
          </div>

          {job.highlightsResponsibilities && (
            <div className="detail-section">
              <h3>Key Responsibilities</h3>
              <div className="job-description">
                {job.highlightsResponsibilities.split('\n').filter(Boolean).map((line, i) => (
                  <p key={i} className="desc-bullet">{line.replace(/^[•\-\*]\s+/, '')}</p>
                ))}
              </div>
            </div>
          )}

          {job.highlightsQualifications && (
            <div className="detail-section">
              <h3>Qualifications</h3>
              <div className="job-description">
                {job.highlightsQualifications.split('\n').filter(Boolean).map((line, i) => (
                  <p key={i} className="desc-bullet">{line.replace(/^[•\-\*]\s+/, '')}</p>
                ))}
              </div>
            </div>
          )}

          {similarJobs.length > 0 && (
            <div className="detail-section">
              <h3>Similar Jobs</h3>
              <div className="job-grid" style={{ marginTop: 16 }}>
                {similarJobs.map((sj) => (
                  <div key={sj.id} className="job-card">
                    <div className="job-card-header">
                      <h3 style={{ fontSize: 16 }}>{sj.title}</h3>
                      <span className={`badge ${sj.jobType.toLowerCase()}`}>{sj.jobType}</span>
                    </div>
                    <div className="job-card-body">
                      <p className="company">{sj.companyName}</p>
                      <p className="meta">
                        <span>{sj.location}</span>
                      </p>
                      <p className="date">{new Date(sj.postedDate).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}</p>
                    </div>
                    <Link to={`/jobs/${sj.id}`} className="btn btn-outline btn-sm" style={{ alignSelf: 'flex-start' }}>View Details</Link>
                  </div>
                ))}
              </div>
            </div>
          )}

          {message && (
            <div className={`alert ${message.includes('failed') || message.includes('already') ? 'alert-error' : 'alert-success'}`}>
              {message}
            </div>
          )}
        </div>

        {job.isActive && !applied && (
          <div className="apply-band">
            <p className="micro" style={{ color: 'rgba(255,255,255,0.5)', marginBottom: 12 }}>Interested?</p>
            <h3>Apply for this position</h3>
            <p>Submit your application and take the next step in your career.</p>
            {job.applyLink ? (
              <button onClick={handleExternalApply} className="btn btn-primary" style={{ gap: 8 }}>
                <img src={`https://www.google.com/s2/favicons?domain=${new URL(job.applyLink).hostname}&sz=24`} alt="" style={{ width: 20, height: 20, borderRadius: 2 }} />
                Apply on {siteNameFromUrl(job.applyLink)}
              </button>
            ) : appUser?.role === 'Applicant' ? (
              <button onClick={handleConfirmApply} className="btn btn-primary">Submit Application</button>
            ) : (
              <p className="micro" style={{ color: 'rgba(255,255,255,0.5)' }}>
                Sign in as a job seeker to apply
              </p>
            )}
          </div>
        )}

        {showPrompt && (
          <div className="confirm-overlay">
            <div className="confirm-card" style={{ maxWidth: 480 }}>
              <h3>Did you apply?</h3>
              <p>Click confirm to track this application in your profile.</p>
              <div style={{ marginBottom: 16, textAlign: 'left' }}>
                <label style={{ display: 'block', fontSize: 14, fontWeight: 500, marginBottom: 6, color: 'var(--primary)' }}>
                  Cover Letter (optional)
                </label>
                <textarea
                  value={coverLetter}
                  onChange={(e) => {
                    setCoverLetter(e.target.value)
                    localStorage.setItem('coverLetter', e.target.value)
                  }}
                  placeholder="Paste your cover letter or leave blank..."
                  rows={4}
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    border: '1px solid var(--hairline)',
                    borderRadius: 8,
                    fontSize: 14,
                    fontFamily: 'var(--font-body)',
                    resize: 'vertical',
                  }}
                />
              </div>
              <div className="confirm-actions">
                <button onClick={handleSkip} className="btn btn-outline">Skip</button>
                <button onClick={handleConfirmApply} className="btn btn-primary" disabled={submitting}>
                  {submitting ? 'Saving...' : 'Yes, I applied'}
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
