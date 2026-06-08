import { useState, useEffect } from 'react'
import toast from 'react-hot-toast'
import { resumeApi } from '../api/axios'

const TONE_OPTIONS = ['Professional', 'Enthusiastic', 'Formal', 'Friendly', 'Confident']
const STORAGE_KEY = 'cover-letter-data'

function loadSaved() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? JSON.parse(raw) : null
  } catch { return null }
}

export default function CoverLetterForm() {
  const saved = loadSaved()
  const [jobTitle, setJobTitle] = useState(saved?.jobTitle || '')
  const [companyName, setCompanyName] = useState(saved?.companyName || '')
  const [jobDescription, setJobDescription] = useState(saved?.jobDescription || '')
  const [hiringManager, setHiringManager] = useState(saved?.hiringManager || '')
  const [tone, setTone] = useState(saved?.tone || 'Professional')
  const [coverLetter, setCoverLetter] = useState(saved?.coverLetter || '')
  const [tips, setTips] = useState(saved?.tips || '')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (coverLetter) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify({
        jobTitle, companyName, jobDescription, hiringManager, tone, coverLetter, tips
      }))
    }
  }, [coverLetter, tips])

  const handleGenerate = async () => {
    if (!jobTitle.trim() || !companyName.trim()) {
      toast.error('Job title and company name are required')
      return
    }

    setLoading(true)
    setError('')
    setCoverLetter('')
    setTips('')

    try {
      const res = await resumeApi.coverLetter({
        jobTitle: jobTitle.trim(),
        companyName: companyName.trim(),
        jobDescription: jobDescription.trim() || undefined,
        hiringManager: hiringManager.trim() || undefined,
        tone,
      })
      if (res.data.coverLetter) {
        setCoverLetter(res.data.coverLetter)
        setTips(res.data.tips || '')
        toast.success('Cover letter generated!')
      } else {
        setError('Empty response from AI. Try again.')
        toast.error('Empty response')
      }
    } catch (err: any) {
      const msg = err.response?.data?.message || err.message || 'Failed to generate cover letter'
      setError(msg)
      toast.error(msg)
    } finally {
      setLoading(false)
    }
  }

  const copyToClipboard = () => {
    navigator.clipboard.writeText(coverLetter)
    toast.success('Copied to clipboard!')
  }

  const clearAll = () => {
    setJobTitle('')
    setCompanyName('')
    setJobDescription('')
    setHiringManager('')
    setCoverLetter('')
    setTips('')
    setError('')
    localStorage.removeItem(STORAGE_KEY)
  }

  return (
    <div style={{ padding: '0 4px' }}>
      <div className="form-group" style={{ marginBottom: 12 }}>
        <label style={{ fontSize: 12, fontWeight: 600, marginBottom: 4, display: 'block' }}>Job Title *</label>
        <input
          type="text"
          value={jobTitle}
          onChange={(e) => setJobTitle(e.target.value)}
          placeholder="e.g. Senior Software Engineer"
          style={{ width: '100%', padding: '8px 12px', borderRadius: 8, border: '1px solid var(--color-border)', fontSize: 13 }}
        />
      </div>

      <div className="form-group" style={{ marginBottom: 12 }}>
        <label style={{ fontSize: 12, fontWeight: 600, marginBottom: 4, display: 'block' }}>Company Name *</label>
        <input
          type="text"
          value={companyName}
          onChange={(e) => setCompanyName(e.target.value)}
          placeholder="e.g. Google"
          style={{ width: '100%', padding: '8px 12px', borderRadius: 8, border: '1px solid var(--color-border)', fontSize: 13 }}
        />
      </div>

      <div className="form-group" style={{ marginBottom: 12 }}>
        <label style={{ fontSize: 12, fontWeight: 600, marginBottom: 4, display: 'block' }}>Hiring Manager (optional)</label>
        <input
          type="text"
          value={hiringManager}
          onChange={(e) => setHiringManager(e.target.value)}
          placeholder="e.g. Jane Smith"
          style={{ width: '100%', padding: '8px 12px', borderRadius: 8, border: '1px solid var(--color-border)', fontSize: 13 }}
        />
      </div>

      <div className="form-group" style={{ marginBottom: 12 }}>
        <label style={{ fontSize: 12, fontWeight: 600, marginBottom: 4, display: 'block' }}>Tone</label>
        <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap' }}>
          {TONE_OPTIONS.map((t) => (
            <button
              key={t}
              className={`btn btn-xs ${tone === t ? 'btn-primary' : 'btn-outline'}`}
              onClick={() => setTone(t)}
              style={{ fontSize: 11 }}
            >
              {t}
            </button>
          ))}
        </div>
      </div>

      <div className="form-group" style={{ marginBottom: 16 }}>
        <label style={{ fontSize: 12, fontWeight: 600, marginBottom: 4, display: 'block' }}>Job Description (optional)</label>
        <textarea
          value={jobDescription}
          onChange={(e) => setJobDescription(e.target.value)}
          placeholder="Paste the job description for a more tailored letter..."
          rows={4}
          style={{ width: '100%', padding: '8px 12px', borderRadius: 8, border: '1px solid var(--color-border)', fontSize: 12, resize: 'vertical' }}
        />
      </div>

      {error && <div className="alert alert-error" style={{ marginBottom: 12, fontSize: 12 }}>{error}</div>}

      <div style={{ display: 'flex', gap: 8, marginBottom: 16 }}>
        <button
          className="btn btn-primary btn-sm"
          onClick={handleGenerate}
          disabled={loading || !jobTitle.trim() || !companyName.trim()}
          style={{ flex: 1 }}
        >
          {loading ? 'Generating...' : 'Generate Cover Letter'}
        </button>
        {(coverLetter || jobTitle || companyName) && (
          <button className="btn btn-outline btn-sm" onClick={clearAll}>
            Clear
          </button>
        )}
      </div>

      {coverLetter && (
        <div style={{ borderTop: '1px solid var(--color-border)', paddingTop: 16 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
            <h4 style={{ margin: 0 }}>Cover Letter</h4>
            <button className="btn btn-outline btn-xs" onClick={copyToClipboard}>
              Copy
            </button>
          </div>
          <div
            style={{
              padding: 16,
              background: '#f8f9fa',
              borderRadius: 12,
              fontSize: 13,
              lineHeight: 1.7,
              whiteSpace: 'pre-wrap',
              fontFamily: 'Georgia, serif',
              border: '1px solid var(--color-border)',
            }}
          >
            {coverLetter}
          </div>

          {tips && (
            <div style={{ marginTop: 16 }}>
              <h4 style={{ marginBottom: 8 }}>Tips to Strengthen</h4>
              <div
                style={{
                  padding: 12,
                  background: '#fff8e1',
                  borderRadius: 8,
                  fontSize: 12,
                  lineHeight: 1.6,
                  whiteSpace: 'pre-wrap',
                  border: '1px solid #ffe082',
                }}
              >
                {tips}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  )
}