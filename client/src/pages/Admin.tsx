import { useState, useEffect } from 'react'
import toast from 'react-hot-toast'
import api from '../api/axios'
import { useAppUser } from '../context/AppContext'

interface Stats {
  totalJobs: number
  totalUsers: number
  totalApplications: number
  jobsWithTech: number
}

export default function Admin() {
  const { user } = useAppUser()
  const [syncing, setSyncing] = useState(false)
  const [populating, setPopulating] = useState(false)
  const [stats, setStats] = useState<Stats | null>(null)

  useEffect(() => {
    api.get('/statistics').then((res) => setStats(res.data)).catch(() => {})
  }, [])

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
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Failed to populate technologies')
    } finally {
      setPopulating(false)
    }
  }

  return (
    <div className="container" style={{ paddingTop: 60, maxWidth: 800 }}>
      <h1 style={{ fontSize: 32, marginBottom: 8 }}>Admin Dashboard</h1>
      <p style={{ color: '#616161', marginBottom: 32 }}>Manage jobs and data</p>

      {stats && (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 16, marginBottom: 40 }}>
          <div style={{ padding: 20, borderRadius: 12, border: '1px solid var(--hairline)' }}>
            <p className="micro" style={{ marginBottom: 4 }}>Total Jobs</p>
            <p style={{ fontSize: 28, fontWeight: 700 }}>{stats.totalJobs}</p>
          </div>
          <div style={{ padding: 20, borderRadius: 12, border: '1px solid var(--hairline)' }}>
            <p className="micro" style={{ marginBottom: 4 }}>Users</p>
            <p style={{ fontSize: 28, fontWeight: 700 }}>{stats.totalUsers}</p>
          </div>
          <div style={{ padding: 20, borderRadius: 12, border: '1px solid var(--hairline)' }}>
            <p className="micro" style={{ marginBottom: 4 }}>Applications</p>
            <p style={{ fontSize: 28, fontWeight: 700 }}>{stats.totalApplications}</p>
          </div>
          <div style={{ padding: 20, borderRadius: 12, border: '1px solid var(--hairline)' }}>
            <p className="micro" style={{ marginBottom: 4 }}>Jobs with Tech</p>
            <p style={{ fontSize: 28, fontWeight: 700 }}>{stats.jobsWithTech}</p>
          </div>
        </div>
      )}

      <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
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
