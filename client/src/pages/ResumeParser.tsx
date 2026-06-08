import { useState, useRef } from 'react'
import { Link } from 'react-router-dom'
import toast from 'react-hot-toast'
import { resumeApi } from '../api/axios'

interface ParsedResume {
  skills: string[]
  seniorityLevel: string | null
  experienceYears: number | null
  education: { degree: string; institution: string; year: string }[]
  summary: string
  jobTitles: string[]
}

interface Job {
  id: number
  title: string
  companyName: string
  location: string
  jobType: string
  salary: string
  score: number
  isRemote: boolean
}

export default function ResumeParser() {
  const [resumeText, setResumeText] = useState('')
  const [parsed, setParsed] = useState<ParsedResume | null>(null)
  const [recommendations, setRecommendations] = useState<Job[]>([])
  const [loading, setLoading] = useState(false)
  const [loadingRecs, setLoadingRecs] = useState(false)
  const [error, setError] = useState('')
  const [activeTab, setActiveTab] = useState<'text' | 'upload'>('text')
  const fileInputRef = useRef<HTMLInputElement>(null)

  const handleParse = async () => {
    if (!resumeText.trim() && !fileInputRef.current?.files?.length) {
      toast.error('Please paste your resume text or upload a file')
      return
    }

    setLoading(true)
    setError('')
    setParsed(null)
    setRecommendations([])

    try {
      const request: any = {}
      if (activeTab === 'text' && resumeText.trim()) {
        request.resumeText = resumeText
      } else if (activeTab === 'upload' && fileInputRef.current?.files?.length) {
        const file = fileInputRef.current.files[0]
        const base64 = await fileToBase64(file)
        request.imageBase64 = base64
        request.imageMediaType = file.type
      }

      const res = await resumeApi.parse(request)
      setParsed(res.data)
      toast.success('Resume parsed successfully!')
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to parse resume'
      setError(msg)
      toast.error(msg)
    } finally {
      setLoading(false)
    }
  }

  const handleGetRecommendations = async () => {
    if (!parsed?.skills.length) {
      toast.error('No skills to search with')
      return
    }

    setLoadingRecs(true)
    try {
      const res = await resumeApi.recommendationsFromSkills(parsed.skills)
      setRecommendations(res.data)
      if (res.data.length === 0) {
        toast('No matching jobs found for your skills')
      } else {
        toast.success(`Found ${res.data.length} matching jobs!`)
      }
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Failed to get recommendations')
    } finally {
      setLoadingRecs(false)
    }
  }

  const fileToBase64 = (file: File): Promise<string> =>
    new Promise((resolve, reject) => {
      const reader = new FileReader()
      reader.onload = () => {
        const result = reader.result as string
        const base64 = result.split(',')[1]
        resolve(base64)
      }
      reader.onerror = reject
      reader.readAsDataURL(file)
    })

  const addSkillToProfile = (skill: string) => {
    navigator.clipboard.writeText(skill)
    toast.success(`"${skill}" copied — add it in your profile preferences`)
  }

  return (
    <div className="container" style={{ paddingTop: 40, maxWidth: 900 }}>
      <p className="micro" style={{ marginBottom: 12 }}>AI Resume Parser</p>
      <h2>Parse Your Resume</h2>
      <p style={{ color: '#888', marginBottom: 32 }}>
        Upload a resume (PDF/image) or paste the text. Our AI will extract your skills and find matching jobs.
      </p>

      <div className="parse-tabs" style={{ display: 'flex', gap: 8, marginBottom: 24 }}>
        <button
          className={`btn ${activeTab === 'text' ? 'btn-primary' : 'btn-outline'}`}
          onClick={() => setActiveTab('text')}
        >
          Paste Text
        </button>
        <button
          className={`btn ${activeTab === 'upload' ? 'btn-primary' : 'btn-outline'}`}
          onClick={() => setActiveTab('upload')}
        >
          Upload PDF / Image
        </button>
      </div>

      {activeTab === 'text' ? (
        <div className="form-group">
          <textarea
            value={resumeText}
            onChange={(e) => setResumeText(e.target.value)}
            placeholder="Paste your resume content here..."
            rows={12}
            style={{ fontFamily: 'monospace', fontSize: 13 }}
          />
        </div>
      ) : (
        <div className="form-group">
          <div
            className="upload-zone"
            style={{
              border: '2px dashed #ccc',
              borderRadius: 12,
              padding: '48px 24px',
              textAlign: 'center',
              cursor: 'pointer',
              transition: 'border-color 0.2s',
            }}
            onClick={() => fileInputRef.current?.click()}
            onDragOver={(e) => { e.preventDefault(); e.currentTarget.style.borderColor = 'var(--color-primary)' }}
            onDragLeave={(e) => { e.currentTarget.style.borderColor = '#ccc' }}
            onDrop={(e) => {
              e.preventDefault()
              e.currentTarget.style.borderColor = '#ccc'
              if (e.dataTransfer.files.length) {
                if (fileInputRef.current) {
                  fileInputRef.current.files = e.dataTransfer.files
                  setActiveTab('upload')
                }
              }
            }}
          >
            <div style={{ fontSize: 48, marginBottom: 12, opacity: 0.3 }}>&#128196;</div>
            <p style={{ color: '#888', marginBottom: 8 }}>Drag & drop your resume here</p>
            <p className="micro" style={{ color: '#aaa' }}>Supports PDF, PNG, JPG</p>
            <input
              ref={fileInputRef}
              type="file"
              accept=".pdf,.png,.jpg,.jpeg"
              style={{ display: 'none' }}
              onChange={() => setActiveTab('upload')}
            />
          </div>
          {fileInputRef.current?.files?.length ? (
            <p style={{ marginTop: 12, color: '#888' }}>
              Selected: <strong>{fileInputRef.current.files[0].name}</strong>
            </p>
          ) : null}
        </div>
      )}

      {error && <div className="alert alert-error">{error}</div>}

      <button
        className="btn btn-primary"
        onClick={handleParse}
        disabled={loading || (!resumeText.trim() && !fileInputRef.current?.files?.length)}
        style={{ marginBottom: 48 }}
      >
        {loading ? 'Parsing...' : 'Parse Resume'}
      </button>

      {parsed && (
        <div className="parsed-results">
          <h3 style={{ marginBottom: 24 }}>Extracted Information</h3>

          {parsed.summary && (
            <div style={{ marginBottom: 24, padding: 20, background: '#f8f9fa', borderRadius: 12 }}>
              <h4 style={{ marginBottom: 8 }}>Summary</h4>
              <p style={{ color: '#555', lineHeight: 1.6 }}>{parsed.summary}</p>
            </div>
          )}

          <div className="parsed-grid" style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 24, marginBottom: 32 }}>
            {parsed.seniorityLevel && (
              <div style={{ padding: 16, background: '#f8f9fa', borderRadius: 12 }}>
                <h4 style={{ marginBottom: 4 }}>Seniority Level</h4>
                <span className="badge role-applicant">{parsed.seniorityLevel}</span>
              </div>
            )}
            {parsed.experienceYears != null && (
              <div style={{ padding: 16, background: '#f8f9fa', borderRadius: 12 }}>
                <h4 style={{ marginBottom: 4 }}>Experience</h4>
                <span style={{ fontWeight: 600 }}>{parsed.experienceYears} years</span>
              </div>
            )}
          </div>

          {parsed.skills.length > 0 && (
            <div style={{ marginBottom: 32 }}>
              <h4 style={{ marginBottom: 12 }}>Skills ({parsed.skills.length})</h4>
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8 }}>
                {parsed.skills.map((skill, i) => (
                  <span
                    key={i}
                    className="chip"
                    onClick={() => addSkillToProfile(skill)}
                    title="Click to copy"
                    style={{
                      padding: '6px 14px',
                      borderRadius: 20,
                      background: 'var(--color-bg-secondary)',
                      border: '1px solid var(--color-border)',
                      cursor: 'pointer',
                      fontSize: 13,
                      fontWeight: 500,
                      transition: 'all 0.2s',
                    }}
                  >
                    {skill}
                  </span>
                ))}
              </div>
            </div>
          )}

          {parsed.education.length > 0 && (
            <div style={{ marginBottom: 32 }}>
              <h4 style={{ marginBottom: 12 }}>Education</h4>
              {parsed.education.map((edu, i) => (
                <div key={i} style={{ padding: '12px 0', borderBottom: '1px solid var(--color-border)' }}>
                  <div style={{ fontWeight: 600 }}>{edu.degree}</div>
                  <div className="micro" style={{ color: '#888' }}>{edu.institution} {edu.year && `— ${edu.year}`}</div>
                </div>
              ))}
            </div>
          )}

          {parsed.jobTitles.length > 0 && (
            <div style={{ marginBottom: 32 }}>
              <h4 style={{ marginBottom: 12 }}>Previous Roles</h4>
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8 }}>
                {parsed.jobTitles.map((title, i) => (
                  <span key={i} style={{ padding: '6px 14px', borderRadius: 8, background: '#f0f0f0', fontSize: 13 }}>
                    {title}
                  </span>
                ))}
              </div>
            </div>
          )}

          <div style={{ marginTop: 32, paddingTop: 24, borderTop: '2px solid var(--color-border)' }}>
            <button
              className="btn btn-primary"
              onClick={handleGetRecommendations}
              disabled={loadingRecs || parsed.skills.length === 0}
              style={{ marginBottom: 16 }}
            >
              {loadingRecs ? 'Finding Matches...' : `Get Job Recommendations (${parsed.skills.length} skills)`}
            </button>
          </div>
        </div>
      )}

      {recommendations.length > 0 && (
        <div className="recommendations" style={{ marginTop: 32 }}>
          <h3 style={{ marginBottom: 24 }}>Matching Jobs ({recommendations.length})</h3>
          <div className="job-list" style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
            {recommendations.map((job) => (
              <Link
                key={job.id}
                to={`/jobs/${job.id}`}
                className="job-card"
                style={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  padding: 20,
                  border: '1px solid var(--color-border)',
                  borderRadius: 12,
                  textDecoration: 'none',
                  color: 'inherit',
                  transition: 'all 0.2s',
                }}
              >
                <div>
                  <div style={{ fontWeight: 600, fontSize: 16, marginBottom: 4 }}>{job.title}</div>
                  <div className="micro" style={{ color: '#888' }}>{job.companyName} — {job.location}</div>
                  <div style={{ marginTop: 8, display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                    <span className="badge">{job.jobType}</span>
                    {job.isRemote && <span className="badge">Remote</span>}
                    {job.salary && <span className="badge">{job.salary}</span>}
                  </div>
                </div>
                <div style={{ textAlign: 'right', minWidth: 60 }}>
                  <div
                    style={{
                      fontSize: 24,
                      fontWeight: 700,
                      color: job.score >= 70 ? '#2e7d32' : job.score >= 40 ? '#f57c00' : '#c62828',
                    }}
                  >
                    {job.score}%
                  </div>
                  <div className="micro" style={{ color: '#888' }}>match</div>
                </div>
              </Link>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}