import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '@clerk/react'
import toast from 'react-hot-toast'
import api, { companyProfilesApi } from '../api/axios'

const SIZE_OPTIONS = ['1-10', '11-50', '51-200', '201-500', '501-1000', '1000+']

export default function ClaimCompany() {
  const { isSignedIn } = useAuth()
  const navigate = useNavigate()
  const [claiming, setClaiming] = useState(false)
  const [name, setName] = useState('')
  const [logoUrl, setLogoUrl] = useState('')
  const [description, setDescription] = useState('')
  const [website, setWebsite] = useState('')
  const [size, setSize] = useState('')
  const [industry, setIndustry] = useState('')

  if (!isSignedIn) {
    return (
      <div className="container" style={{ paddingTop: 60 }}>
        <h1 style={{ fontSize: 32, marginBottom: 16 }}>Claim a Company</h1>
        <p style={{ color: '#616161' }}>Sign in as an employer to claim your company.</p>
      </div>
    )
  }

  const handleClaim = async () => {
    if (!name.trim()) { toast.error('Company name is required'); return }
    setClaiming(true)
    try {
      const res = await companyProfilesApi.claim({
        name,
        logoUrl: logoUrl || undefined,
        description: description || undefined,
        website: website || undefined,
        size: size || undefined,
        industry: industry || undefined,
      })
      toast.success(res.data.message)
      navigate(`/company/${res.data.id}`)
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Failed to claim company')
    } finally {
      setClaiming(false)
    }
  }

  return (
    <div className="container" style={{ paddingTop: 60, maxWidth: 600 }}>
      <h1 style={{ fontSize: 32, marginBottom: 8 }}>Claim a Company</h1>
      <p style={{ color: '#616161', marginBottom: 32 }}>
        Claim your company profile to manage your brand and review applicants.
      </p>

      <div className="form-group">
        <label>Company Name *</label>
        <input type="text" value={name} onChange={e => setName(e.target.value)} placeholder="e.g. Acme Corp" />
      </div>

      <div className="form-group">
        <label>Logo URL</label>
        <input type="url" value={logoUrl} onChange={e => setLogoUrl(e.target.value)} placeholder="https://example.com/logo.png" />
      </div>

      <div className="form-group">
        <label>Description</label>
        <textarea
          value={description}
          onChange={e => setDescription(e.target.value)}
          placeholder="Tell candidates about your company..."
          style={{ minHeight: 100, resize: 'vertical' }}
        />
      </div>

      <div className="form-group">
        <label>Website</label>
        <input type="url" value={website} onChange={e => setWebsite(e.target.value)} placeholder="https://example.com" />
      </div>

      <div className="form-row">
        <div className="form-group">
          <label>Company Size</label>
          <select value={size} onChange={e => setSize(e.target.value)}>
            <option value="">Select size</option>
            {SIZE_OPTIONS.map(s => <option key={s} value={s}>{s} employees</option>)}
          </select>
        </div>
        <div className="form-group">
          <label>Industry</label>
          <input type="text" value={industry} onChange={e => setIndustry(e.target.value)} placeholder="e.g. Technology" />
        </div>
      </div>

      <div style={{ marginTop: 24 }}>
        <button className="btn btn-primary" onClick={handleClaim} disabled={claiming}>
          {claiming ? 'Claiming...' : 'Claim Company'}
        </button>
      </div>
    </div>
  )
}
