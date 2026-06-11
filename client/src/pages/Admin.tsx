import { useState, useEffect, useCallback } from 'react'
import toast from 'react-hot-toast'
import api from '../api/axios'
import { useAppUser } from '../context/AppContext'
import { SkeletonList } from '../components/Skeleton'

interface Stats {
  totalJobs: number
  totalUsers: number
  totalApplications: number
  jobsWithTech: number
  totalTechnologies: number
  jobsByType: Record<string, number>
}

export default function Admin() {
  const { user } = useAppUser()
  const [syncing, setSyncing] = useState(false)
  const [populating, setPopulating] = useState(false)
  const [stats, setStats] = useState<Stats | null>(null)
  const [loading, setLoading] = useState(true)

  const fetchStats = useCallback(async () => {
    setLoading(true)
    try {
      const res = await api.get('/statistics')
      setStats(res.data)
    } catch {
      // silently fail
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { fetchStats() }, [fetchStats])

  if (user?.role !== 'Admin') {
    return (
      <div className="container" style={{ paddingTop: 60, textAlign: 'center' }}>
        <h1>Access Denied</h1>
        <p style={{ color: '#616161' }}>You need admin privileges to access this page.</p>
      </div>
    )
  }

  const handleSync = async () => {
    setSyncing(true)
    try {
      const res = await api.post('/jobs/sync')
      toast.success(`Synced ${res.data.added} new jobs from JSearch`)
      fetchStats()
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Sync failed')
    } finally {
      setSyncing(false)
    }
  }

  const handlePopulate = async () => {
    setPopulating(true)
    try {
      const res = await api.post('/jobs/populate-techs')
      toast.success(`Populated technologies for ${res.data.updated} jobs`)
      fetchStats()
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Failed to populate technologies')
    } finally {
      setPopulating(false)
    }
  }

  return (
    <div className="container" style={{ paddingTop: 60, paddingBottom: 60, maxWidth: 900 }}>
      <div className="jobs-header">
        <div>
          <h1 style={{ fontSize: 32 }}>Admin Dashboard</h1>
          <p style={{ color: '#616161', marginTop: 8 }}>Platform management and statistics</p>
        </div>
      </div>

      {loading ? (
        <SkeletonList count={3} />
      ) : stats && (
        <>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: 12, marginBottom: 40 }}>
            <StatCard label="Total Jobs" value={stats.totalJobs} />
            <StatCard label="Users" value={stats.totalUsers} />
            <StatCard label="Applications" value={stats.totalApplications} />
            <StatCard label="Jobs with Tech" value={stats.jobsWithTech} />
            <StatCard label="Tech Skills" value={stats.totalTechnologies} />
          </div>

          {Object.keys(stats.jobsByType).length > 0 && (
            <div style={{ marginBottom: 40 }}>
              <h3 style={{ fontSize: 18, marginBottom: 16 }}>Jobs by Type</h3>
              <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                {Object.entries(stats.jobsByType).map(([type, count]) => (
                  <div key={type} style={{
                    padding: '8px 16px',
                    borderRadius: 8,
                    border: '1px solid var(--hairline)',
                    fontSize: 14,
                  }}>
                    <span style={{ fontWeight: 500 }}>{type}</span>
                    <span style={{ color: 'var(--body-muted)', marginLeft: 8 }}>{count}</span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
        <div style={{ padding: 24, borderRadius: 12, border: '1px solid var(--hairline)' }}>
          <h3 style={{ marginBottom: 8 }}>Sync Jobs from JSearch</h3>
          <p style={{ color: '#616161', marginBottom: 16, fontSize: 14 }}>
            Fetch new job listings from the JSearch API and add them to the database.
          </p>
          <button className="btn btn-primary" onClick={handleSync} disabled={syncing}>
            {syncing ? 'Syncing...' : 'Sync Jobs'}
          </button>
        </div>

        <div style={{ padding: 24, borderRadius: 12, border: '1px solid var(--hairline)' }}>
          <h3 style={{ marginBottom: 8 }}>Populate Technologies</h3>
          <p style={{ color: '#616161', marginBottom: 16, fontSize: 14 }}>
            Extract technology keywords from job titles and descriptions for jobs that don't have tech data yet.
          </p>
          <button className="btn btn-primary" onClick={handlePopulate} disabled={populating}>
            {populating ? 'Populating...' : 'Populate Technologies'}
          </button>
        </div>
      </div>
    </div>
  )
}

function StatCard({ label, value }: { label: string; value: number }) {
  return (
    <div style={{ padding: 16, borderRadius: 12, border: '1px solid var(--hairline)', textAlign: 'center' }}>
      <div style={{ fontSize: 12, color: 'var(--body-muted)', marginBottom: 4 }}>{label}</div>
      <div style={{ fontSize: 24, fontWeight: 700, color: 'var(--primary)' }}>{value}</div>
    </div>
  )
}
