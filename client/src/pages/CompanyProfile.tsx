import { useState, useEffect } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useAuth } from '@clerk/react'
import api, { companyProfilesApi } from '../api/axios'
import { SkeletonList } from '../components/Skeleton'

interface Company {
  id: number
  name: string
  logoUrl: string | null
  description: string | null
  website: string | null
  size: string | null
  industry: string | null
  foundedYear: number | null
  culture: string | null
  isVerified: boolean
  openRoles: number
}

export default function CompanyProfile() {
  const { id } = useParams<{ id: string }>()
  const { isSignedIn } = useAuth()
  const [company, setCompany] = useState<Company | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!id) return
    setLoading(true)
    companyProfilesApi.get(Number(id))
      .then(res => setCompany(res.data))
      .catch(() => setError('Company not found'))
      .finally(() => setLoading(false))
  }, [id])

  if (loading) {
    return (
      <div className="container" style={{ paddingTop: 60 }}>
        <SkeletonList count={2} />
      </div>
    )
  }

  if (error || !company) {
    return (
      <div className="container" style={{ paddingTop: 60 }}>
        <h1 style={{ fontSize: 32, marginBottom: 16 }}>Company Not Found</h1>
        <p style={{ color: '#616161', marginBottom: 16 }}>{error || 'This company profile does not exist.'}</p>
        <Link to="/jobs" className="btn btn-outline">Browse Jobs</Link>
      </div>
    )
  }

  return (
    <div className="container" style={{ paddingTop: 60, maxWidth: 800 }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 24, marginBottom: 32 }}>
        {company.logoUrl ? (
          <img src={company.logoUrl} alt={company.name} style={{ width: 80, height: 80, borderRadius: 16, objectFit: 'cover' }} />
        ) : (
          <div style={{
            width: 80, height: 80, borderRadius: 16,
            background: 'var(--soft-stone)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            fontSize: 32, fontWeight: 700, color: 'var(--body-muted)',
          }}>
            {company.name.charAt(0)}
          </div>
        )}
        <div>
          <h1 style={{ fontSize: 28, marginBottom: 4 }}>
            {company.name}
            {company.isVerified && <span style={{ marginLeft: 8, fontSize: 14, color: 'var(--deep-green)' }}>✓ Verified</span>}
          </h1>
          <p style={{ color: '#616161' }}>
            {company.industry && <>{company.industry}</>}
            {company.size && <> · {company.size} employees</>}
            {company.foundedYear && <> · Founded {company.foundedYear}</>}
            {company.openRoles > 0 && <> · {company.openRoles} open roles</>}
          </p>
        </div>
      </div>

      {company.description && (
        <div style={{ marginBottom: 32 }}>
          <h3 style={{ marginBottom: 12 }}>About</h3>
          <p style={{ color: '#616161', lineHeight: 1.7 }}>{company.description}</p>
        </div>
      )}

      {company.culture && (
        <div style={{ marginBottom: 32 }}>
          <h3 style={{ marginBottom: 12 }}>Culture</h3>
          <p style={{ color: '#616161', lineHeight: 1.7 }}>{company.culture}</p>
        </div>
      )}

      {company.website && (
        <div style={{ marginBottom: 32 }}>
          <h3 style={{ marginBottom: 12 }}>Website</h3>
          <a href={company.website} target="_blank" rel="noopener noreferrer" style={{ color: 'var(--action-blue)' }}>
            {company.website}
          </a>
        </div>
      )}

      {company.openRoles > 0 && (
        <div>
          <h3 style={{ marginBottom: 12 }}>Open Positions</h3>
          <Link to={`/jobs?company=${encodeURIComponent(company.name)}`} className="btn btn-primary">
            View {company.openRoles} Open Roles
          </Link>
        </div>
      )}

      {isSignedIn && (
        <div style={{ marginTop: 32, paddingTop: 32, borderTop: '1px solid var(--hairline)' }}>
          <Link to={`/claim-company`} className="btn btn-outline">
            Claim This Company
          </Link>
        </div>
      )}
    </div>
  )
}
