import { useState, useEffect, useCallback } from 'react'
import { Link } from 'react-router-dom'
import api from '../api/axios'
import { SkeletonList } from '../components/Skeleton'

interface DashboardData {
  totalJobs: number
  activeJobs: number
  totalApplications: number
  applicationsByStatus: Record<string, number>
  topJobs: { jobId: number; title: string; applicationCount: number }[]
  monthlyPostings: { year: number; month: number; count: number }[]
}

const STATUS_COLORS: Record<string, string> = {
  Pending: '#fef3c7',
  Screening: '#f1f5ff',
  Interview: '#ede9fe',
  Accepted: '#edfce9',
  Rejected: '#fef2f2',
}

const STATUS_TEXT: Record<string, string> = {
  Pending: '#92400e',
  Screening: '#1863dc',
  Interview: '#7c3aed',
  Accepted: '#003c33',
  Rejected: '#b30000',
}

export default function EmployerDashboard() {
  const [data, setData] = useState<DashboardData | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const fetchData = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const res = await api.get('/statistics/employer')
      setData(res.data)
    } catch {
      setError('Failed to load dashboard.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { fetchData() }, [fetchData])

  if (loading) return <div className="container" style={{ paddingTop: 60 }}><SkeletonList count={3} /></div>
  if (error) return <div className="container" style={{ paddingTop: 60 }}><p style={{ color: '#616161' }}>{error}</p><button className="btn btn-outline" onClick={fetchData}>Retry</button></div>
  if (!data) return null

  return (
    <div className="container" style={{ paddingTop: 60, paddingBottom: 60 }}>
      <div className="jobs-header">
        <div>
          <h1 style={{ fontSize: 32 }}>Employer Dashboard</h1>
          <p style={{ color: '#616161', marginTop: 8 }}>Overview of your hiring activity</p>
        </div>
        <Link to="/create-job" className="btn btn-primary">Post New Job</Link>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 16, marginBottom: 40 }}>
        <StatCard label="Total Jobs" value={data.totalJobs} />
        <StatCard label="Active Jobs" value={data.activeJobs} />
        <StatCard label="Total Applications" value={data.totalApplications} />
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 24, marginBottom: 40 }}>
        <div>
          <h3 style={{ fontSize: 18, marginBottom: 16 }}>Applications by Status</h3>
          {Object.keys(data.applicationsByStatus).length === 0 ? (
            <p style={{ color: '#616161', fontSize: 14 }}>No applications yet.</p>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
              {Object.entries(data.applicationsByStatus).map(([status, count]) => (
                <div key={status} style={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  padding: '10px 16px',
                  borderRadius: 8,
                  background: STATUS_COLORS[status] || '#f5f5f5',
                }}>
                  <span style={{ fontSize: 14, fontWeight: 500, color: STATUS_TEXT[status] || '#333' }}>{status}</span>
                  <span style={{ fontSize: 14, fontWeight: 600, color: STATUS_TEXT[status] || '#333' }}>{count}</span>
                </div>
              ))}
            </div>
          )}
        </div>

        <div>
          <h3 style={{ fontSize: 18, marginBottom: 16 }}>Top Jobs by Applications</h3>
          {data.topJobs.length === 0 ? (
            <p style={{ color: '#616161', fontSize: 14 }}>No jobs posted yet.</p>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
              {data.topJobs.map(job => (
                <Link
                  key={job.jobId}
                  to={`/jobs/${job.jobId}`}
                  style={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    padding: '10px 16px',
                    borderRadius: 8,
                    border: '1px solid #e5e7eb',
                    textDecoration: 'none',
                    color: 'inherit',
                    transition: 'background 0.15s',
                  }}
                >
                  <span style={{ fontSize: 14, fontWeight: 500, color: '#212121' }}>{job.title}</span>
                  <span style={{ fontSize: 13, color: '#616161' }}>{job.applicationCount} applications</span>
                </Link>
              ))}
            </div>
          )}
        </div>
      </div>

      <div>
        <h3 style={{ fontSize: 18, marginBottom: 16 }}>Recent Activity</h3>
        {data.monthlyPostings.length === 0 ? (
          <p style={{ color: '#616161', fontSize: 14 }}>No posting history yet.</p>
        ) : (
          <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
            {data.monthlyPostings.slice(-6).map(mp => (
              <div key={`${mp.year}-${mp.month}`} style={{
                padding: '10px 16px',
                borderRadius: 8,
                border: '1px solid #e5e7eb',
                textAlign: 'center',
                minWidth: 80,
              }}>
                <div style={{ fontSize: 20, fontWeight: 700, color: '#17171c' }}>{mp.count}</div>
                <div style={{ fontSize: 12, color: '#616161' }}>{new Date(mp.year, mp.month - 1).toLocaleString('default', { month: 'short' })} {mp.year}</div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}

function StatCard({ label, value }: { label: string; value: number }) {
  return (
    <div style={{
      padding: 20,
      borderRadius: 12,
      border: '1px solid #e5e7eb',
      background: '#fff',
    }}>
      <div style={{ fontSize: 13, color: '#616161', marginBottom: 4 }}>{label}</div>
      <div style={{ fontSize: 28, fontWeight: 700, color: '#17171c' }}>{value}</div>
    </div>
  )
}
