import { useState, useEffect, useCallback } from 'react'
import { Link } from 'react-router-dom'
import api from '../api/axios'
import { SkeletonList } from '../components/Skeleton'

interface Application {
  id: number
  jobId: number
  status: string
  appliedDate: string
  job: { title: string; companyName: string; location: string }
}

const COLUMNS = [
  { key: 'Pending', label: 'Applied' },
  { key: 'Screening', label: 'Screening' },
  { key: 'Interview', label: 'Interview' },
  { key: 'Accepted', label: 'Offer' },
  { key: 'Rejected', label: 'Rejected' },
]

export default function MyApplications() {
  const [apps, setApps] = useState<Application[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [draggedId, setDraggedId] = useState<number | null>(null)

  const fetchApps = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const res = await api.get('/applications/my')
      setApps(res.data)
    } catch {
      setError('Failed to load applications.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { fetchApps() }, [fetchApps])

  const handleDragStart = (e: React.DragEvent, id: number) => {
    setDraggedId(id)
    e.dataTransfer.effectAllowed = 'move'
  }

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault()
    e.dataTransfer.dropEffect = 'move'
  }

  const handleDrop = async (e: React.DragEvent, newStatus: string) => {
    e.preventDefault()
    if (draggedId === null) return

    const app = apps.find(a => a.id === draggedId)
    if (!app || app.status === newStatus) {
      setDraggedId(null)
      return
    }

    setDraggedId(null)
    setApps(prev => prev.map(a => a.id === draggedId ? { ...a, status: newStatus } : a))

    try {
      await api.put(`/applications/${draggedId}/status`, { status: newStatus })
    } catch {
      setApps(prev => prev.map(a => a.id === draggedId ? { ...a, status: app.status } : a))
    }
  }

  const grouped = COLUMNS.map(col => ({
    ...col,
    items: apps.filter(a => a.status === col.key),
  }))

  return (
    <div className="container" style={{ paddingTop: 60 }}>
      <div className="jobs-header">
        <div>
          <h1 style={{ fontSize: 32 }}>My Applications</h1>
          <p style={{ color: '#616161', marginTop: 8 }}>
            Track and manage your job applications
          </p>
        </div>
        <Link to="/jobs" className="btn btn-outline">Browse Jobs</Link>
      </div>

      {loading ? (
        <SkeletonList count={4} />
      ) : error ? (
        <div>
          <p style={{ color: '#616161', marginBottom: 16 }}>{error}</p>
          <button className="btn btn-outline" onClick={fetchApps}>Retry</button>
        </div>
      ) : apps.length === 0 ? (
        <p style={{ color: '#616161' }}>
          You haven't applied to any jobs yet.{' '}
          <Link to="/jobs">Browse jobs</Link> to get started.
        </p>
      ) : (
        <div className="kanban-board">
          {grouped.map(col => (
            <div
              key={col.key}
              className="kanban-column"
              onDragOver={handleDragOver}
              onDrop={(e) => handleDrop(e, col.key)}
            >
              <div className="kanban-column-header">
                <span>{col.label}</span>
                <span className="kanban-column-count">{col.items.length}</span>
              </div>
              {col.items.map(app => (
                <div
                  key={app.id}
                  className="kanban-card"
                  draggable
                  onDragStart={(e) => handleDragStart(e, app.id)}
                >
                  <Link to={`/jobs/${app.jobId}`} style={{ textDecoration: 'none', color: 'inherit' }}>
                    <div className="kanban-card-title">{app.job.title}</div>
                    <div className="kanban-card-company">{app.job.companyName}</div>
                    {app.job.location && (
                      <div className="kanban-card-company">{app.job.location}</div>
                    )}
                    <div className="kanban-card-date">
                      Applied {new Date(app.appliedDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}
                    </div>
                  </Link>
                </div>
              ))}
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
