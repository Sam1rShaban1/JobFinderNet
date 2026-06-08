import { useState, useRef } from 'react'
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

export default function ResumeParserForm() {
  const [resumeText, setResumeText] = useState('')
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [parsed, setParsed] = useState<ParsedResume | null>(null)
  const [recommendations, setRecommendations] = useState<Job[]>([])
  const [loading, setLoading] = useState(false)
  const [loadingRecs, setLoadingRecs] = useState(false)
  const [error, setError] = useState('')
  const [activeTab, setActiveTab] = useState<'text' | 'upload'>('text')
  const fileInputRef = useRef<HTMLInputElement>(null)

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (file) {
      setSelectedFile(file)
      setActiveTab('upload')
    }
  }

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault()
    const file = e.dataTransfer.files[0]
    if (file) {
      setSelectedFile(file)
      setActiveTab('upload')
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

  const handleParse = async () => {
    if (!resumeText.trim() && !selectedFile) {
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
      } else if (activeTab === 'upload' && selectedFile) {
        const base64 = await fileToBase64(selectedFile)
        request.imageBase64 = base64
        request.imageMediaType = selectedFile.type
        request.isPdf = selectedFile.type === 'application/pdf' || selectedFile.name.toLowerCase().endsWith('.pdf')
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

  const clearAll = () => {
    setResumeText('')
    setSelectedFile(null)
    setParsed(null)
    setRecommendations([])
    setError('')
  }

  return (
    <div style={{ padding: '0 4px' }}>
      <div style={{ display: 'flex', gap: 8, marginBottom: 16 }}>
        <button
          className={`btn ${activeTab === 'text' ? 'btn-primary' : 'btn-outline'} btn-sm`}
          onClick={() => setActiveTab('text')}
        >
          Paste Text
        </button>
        <button
          className={`btn ${activeTab === 'upload' ? 'btn-primary' : 'btn-outline'} btn-sm`}
          onClick={() => setActiveTab('upload')}
        >
          Upload File
        </button>
        {(resumeText || selectedFile || parsed) && (
          <button className="btn btn-outline btn-sm" onClick={clearAll} style={{ marginLeft: 'auto' }}>
            Clear
          </button>
        )}
      </div>

      {activeTab === 'text' ? (
        <div style={{ marginBottom: 16 }}>
          <textarea
            value={resumeText}
            onChange={(e) => setResumeText(e.target.value)}
            placeholder="Paste your resume content here..."
            rows={8}
            style={{ width: '100%', fontFamily: 'monospace', fontSize: 12, padding: 12, borderRadius: 8, border: '1px solid var(--color-border)', resize: 'vertical' }}
          />
        </div>
      ) : (
        <div style={{ marginBottom: 16 }}>
          <input
            ref={fileInputRef}
            type="file"
            accept=".pdf,.png,.jpg,.jpeg"
            onChange={handleFileChange}
            style={{ display: 'none' }}
          />
          <div
            onClick={() => fileInputRef.current?.click()}
            onDragOver={(e) => { e.preventDefault(); e.currentTarget.style.borderColor = 'var(--color-primary)' }}
            onDragLeave={(e) => { e.currentTarget.style.borderColor = '#ccc' }}
            onDrop={handleDrop}
            style={{
              border: '2px dashed #ccc',
              borderRadius: 12,
              padding: '32px 16px',
              textAlign: 'center',
              cursor: 'pointer',
              transition: 'border-color 0.2s',
            }}
          >
            {selectedFile ? (
              <div>
                <div style={{ fontWeight: 600, marginBottom: 4 }}>{selectedFile.name}</div>
                <div className="micro" style={{ color: '#888' }}>{(selectedFile.size / 1024).toFixed(1)} KB</div>
              </div>
            ) : (
              <div>
                <div style={{ fontSize: 32, marginBottom: 8, opacity: 0.3 }}>&#128196;</div>
                <p style={{ color: '#888', margin: 0, fontSize: 13 }}>Click or drag PDF/image here</p>
              </div>
            )}
          </div>
        </div>
      )}

      {error && <div className="alert alert-error" style={{ marginBottom: 12, fontSize: 12 }}>{error}</div>}

      <button
        className="btn btn-primary btn-sm btn-full"
        onClick={handleParse}
        disabled={loading || (!resumeText.trim() && !selectedFile)}
        style={{ marginBottom: 16 }}
      >
        {loading ? 'Parsing...' : 'Parse Resume'}
      </button>

      {parsed && (
        <div style={{ borderTop: '1px solid var(--color-border)', paddingTop: 16 }}>
          <h4 style={{ marginBottom: 12 }}>Extracted Skills ({parsed.skills.length})</h4>
          {parsed.skills.length > 0 && (
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6, marginBottom: 16 }}>
              {parsed.skills.map((skill, i) => (
                <span
                  key={i}
                  style={{
                    padding: '4px 10px',
                    borderRadius: 12,
                    background: 'var(--color-bg-secondary)',
                    border: '1px solid var(--color-border)',
                    fontSize: 12,
                    fontWeight: 500,
                  }}
                >
                  {skill}
                </span>
              ))}
            </div>
          )}

          {parsed.seniorityLevel && (
            <p style={{ fontSize: 13, marginBottom: 4 }}>
              <strong>Level:</strong> {parsed.seniorityLevel}
              {parsed.experienceYears != null && <> &middot; {parsed.experienceYears} years</>}
            </p>
          )}

          {parsed.education.length > 0 && (
            <div style={{ marginBottom: 12 }}>
              {parsed.education.map((edu, i) => (
                <p key={i} style={{ fontSize: 12, color: '#888', margin: '2px 0' }}>
                  {edu.degree} — {edu.institution} {edu.year && `(${edu.year})`}
                </p>
              ))}
            </div>
          )}

          <button
            className="btn btn-primary btn-sm btn-full"
            onClick={handleGetRecommendations}
            disabled={loadingRecs || parsed.skills.length === 0}
          >
            {loadingRecs ? 'Finding Matches...' : `Get Job Recommendations`}
          </button>

          {recommendations.length > 0 && (
            <div style={{ marginTop: 16 }}>
              <h4 style={{ marginBottom: 8 }}>Matching Jobs ({recommendations.length})</h4>
              {recommendations.map((job) => (
                <a
                  key={job.id}
                  href={`/jobs/${job.id}`}
                  target="_blank"
                  rel="noreferrer"
                  style={{
                    display: 'block',
                    padding: '10px 12px',
                    border: '1px solid var(--color-border)',
                    borderRadius: 8,
                    marginBottom: 8,
                    textDecoration: 'none',
                    color: 'inherit',
                    fontSize: 13,
                  }}
                >
                  <div style={{ fontWeight: 600 }}>{job.title}</div>
                  <div style={{ color: '#888', fontSize: 12 }}>{job.companyName} — {job.location}</div>
                  <div style={{ marginTop: 4 }}>
                    <span className="badge" style={{ fontSize: 11 }}>{job.jobType}</span>
                    <span style={{ float: 'right', fontWeight: 700, color: job.score >= 70 ? '#2e7d32' : '#f57c00' }}>
                      {job.score}%
                    </span>
                  </div>
                </a>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  )
}