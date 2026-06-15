import { useState, useEffect, useCallback, useRef } from 'react'
import { Link } from 'react-router-dom'
import api from '../api/axios'
import { SkeletonList } from '../components/Skeleton'
import ApplicationNotesPanel from '../components/ApplicationNotesPanel'
import { useAppUser } from '../context/AppContext'

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
  const { user } = useAppUser()
  const [apps, setApps] = useState<Application[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const draggedId = useRef<number | null>(null)
  const [expandedNotes, setExpandedNotes] = useState<number | null>(null)

  const canSeeNotes = user?.role === 'Employer' || user?.role === 'Admin'
  const canDrag = true
  const [dragOverColumn, setDragOverColumn] = useState<string | null>(null)

  const toggleNotes = (appId: number) => {
    setExpandedNotes(prev => prev === appId ? null : appId)
  }

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
    draggedId.current = id
    e.dataTransfer.effectAllowed = 'move'
  }

  const handleDragOver = (e: React.DragEvent, colKey: string) => {
    e.preventDefault()
    e.dataTransfer.dropEffect = 'move'
    setDragOverColumn(colKey)
  }

  const handleDragLeave = () => {
    setDragOverColumn(null)
  }

  const handleDrop = async (e: React.DragEvent, newStatus: string) => {
    e.preventDefault()
    setDragOverColumn(null)
    const id = draggedId.current
    if (id === null) return
    draggedId.current = null

    const app = apps.find(a => a.id === id)
    if (!app || app.status === newStatus) return

    setApps(prev => prev.map(a => a.id === id ? { ...a, status: newStatus } : a))

    try {
      await api.put(`/applications/${id}/status`, { status: newStatus })
    } catch {
      setApps(prev => prev.map(a => a.id === id ? { ...a, status: app.status } : a))
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
        <div className="empty-state">
          <div className="empty-state-icon">&#9632;</div>
          <h3>No applications yet</h3>
          <p>Apply to jobs to track your progress here on the kanban board.</p>
          <Link to="/jobs" className="btn btn-outline">Browse Jobs</Link>
        </div>
      ) : (
        <div className="kanban-board">
          {grouped.map(col => (
            <div
              key={col.key}
              className={`kanban-column${dragOverColumn === col.key ? ' drag-over' : ''}`}
              onDragOver={(e) => handleDragOver(e, col.key)}
              onDragLeave={handleDragLeave}
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
                  draggable={canDrag}
                  onDragStart={canDrag ? (e) => handleDragStart(e, app.id) : undefined}
                  style={{ cursor: canDrag ? 'grab' : 'default' }}
                >
                  <Link to={`/jobs/${app.jobId}`} draggable={false} style={{ textDecoration: 'none', color: 'inherit' }}>
                    <div className="kanban-card-title">{app.job.title}</div>
                    <div className="kanban-card-company">{app.job.companyName}</div>
                    {app.job.location && (
                      <div className="kanban-card-company">{app.job.location}</div>
                    )}
                    <div className="kanban-card-date">
                      Applied {new Date(app.appliedDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}
                    </div>
                  </Link>
                  {canSeeNotes && (
                    <>
                      <button
                        onClick={() => toggleNotes(app.id)}
                        style={{
                          marginTop: 8,
                          background: 'none',
                          border: 'none',
                          cursor: 'pointer',
                          fontSize: 12,
                          color: 'var(--primary)',
                          padding: 0,
                          display: 'flex',
                          alignItems: 'center',
                          gap: 4,
                        }}
                      >
                        {expandedNotes === app.id ? '▲ Hide Notes' : '▼ Notes'}
                      </button>
                      {expandedNotes === app.id && (
                        <ApplicationNotesPanel applicationId={app.id} />
                      )}
                    </>
                  )}
                </div>
              ))}
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
