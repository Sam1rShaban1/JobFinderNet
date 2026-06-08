import { useState, useEffect, useCallback } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '@clerk/react'
import toast from 'react-hot-toast'
import api, { savedSearchesApi } from '../api/axios'
import { SkeletonList } from '../components/Skeleton'

interface SavedSearch {
  id: number
  name: string
  filtersJson: string
  emailFrequency: string
  lastRunAt: string | null
  createdAt: string
}

function parseFilters(json: string) {
  try {
    const filters = JSON.parse(json)
    const parts: string[] = []
    if (filters.search) parts.push(`"${filters.search}"`)
    if (filters.location) parts.push(filters.location)
    if (filters.jobType) parts.push(filters.jobType)
    if (filters.seniority) parts.push(filters.seniority)
    if (filters.isRemote) parts.push('Remote')
    if (filters.tech?.length) parts.push(filters.tech.join(', '))
    if (filters.salaryMin || filters.salaryMax) {
      const min = filters.salaryMin ? `$${Math.round(filters.salaryMin / 1000)}k` : '?'
      const max = filters.salaryMax ? `$${Math.round(filters.salaryMax / 1000)}k` : '?'
      parts.push(`${min} - ${max}`)
    }
    return parts.length > 0 ? parts.join(' · ') : 'No filters'
  } catch {
    return 'No filters'
  }
}

export default function SavedSearches() {
  const { isSignedIn, isLoaded } = useAuth()
  const [searches, setSearches] = useState<SavedSearch[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [creating, setCreating] = useState(false)
  const [showForm, setShowForm] = useState(false)
  const [newName, setNewName] = useState('')
  const [newSearch, setNewSearch] = useState('')
  const [newLocation, setNewLocation] = useState('')
  const [newJobType, setNewJobType] = useState('')
  const [newIsRemote, setNewIsRemote] = useState(false)
  const [newFrequency, setNewFrequency] = useState('daily')

  const fetchSearches = useCallback(async () => {
    if (!isLoaded || !isSignedIn) return
    setLoading(true)
    setError('')
    try {
      const res = await savedSearchesApi.list()
      setSearches(res.data)
    } catch {
      setError('Failed to load saved searches.')
    } finally {
      setLoading(false)
    }
  }, [isLoaded, isSignedIn])

  useEffect(() => { fetchSearches() }, [fetchSearches])

  const handleCreate = async () => {
    if (!newName.trim()) { toast.error('Name is required'); return }
    setCreating(true)
    try {
      const res = await savedSearchesApi.create({
        name: newName,
        search: newSearch || undefined,
        location: newLocation || undefined,
        jobType: newJobType || undefined,
        isRemote: newIsRemote || undefined,
        emailFrequency: newFrequency,
      })
      setSearches(prev => [res.data, ...prev])
      setShowForm(false)
      setNewName('')
      setNewSearch('')
      setNewLocation('')
      setNewJobType('')
      setNewIsRemote(false)
      setNewFrequency('daily')
      toast.success('Search saved!')
    } catch {
      toast.error('Failed to save search.')
    } finally {
      setCreating(false)
    }
  }

  const handleDelete = async (id: number) => {
    try {
      await savedSearchesApi.delete(id)
      setSearches(prev => prev.filter(s => s.id !== id))
      toast.success('Search deleted.')
    } catch {
      toast.error('Failed to delete.')
    }
  }

  const handleRun = async (id: number) => {
    try {
      const res = await savedSearchesApi.run(id)
      toast.success(`Found ${res.data.matchCount} matches!`)
      fetchSearches()
    } catch {
      toast.error('Failed to run search.')
    }
  }

  if (!isLoaded || loading) {
    return (
      <div className="container" style={{ paddingTop: 60 }}>
        <h1 style={{ fontSize: 32, marginBottom: 24 }}>Saved Searches</h1>
        <SkeletonList count={4} />
      </div>
    )
  }

  if (error) {
    return (
      <div className="container" style={{ paddingTop: 60 }}>
        <h1 style={{ fontSize: 32, marginBottom: 16 }}>Saved Searches</h1>
        <p style={{ color: '#616161', marginBottom: 16 }}>{error}</p>
        <button className="btn btn-outline" onClick={fetchSearches}>Retry</button>
      </div>
    )
  }

  return (
    <div className="container" style={{ paddingTop: 60 }}>
      <div className="jobs-header">
        <div>
          <h1 style={{ fontSize: 32 }}>Saved Searches</h1>
          <p style={{ color: '#616161', marginTop: 8 }}>
            Save search filters and get email alerts for new matches
          </p>
        </div>
        <button className="btn btn-primary" onClick={() => setShowForm(!showForm)}>
          {showForm ? 'Cancel' : '+ New Search'}
        </button>
      </div>

      {showForm && (
        <div style={{
          border: '1px solid var(--hairline)',
          borderRadius: 'var(--radius-md)',
          padding: 'var(--space-lg)',
          marginBottom: 'var(--space-lg)',
          background: 'var(--canvas)',
        }}>
          <h3 style={{ marginBottom: 16 }}>Create Saved Search</h3>
          <div className="form-group">
            <label>Search Name</label>
            <input type="text" value={newName} onChange={e => setNewName(e.target.value)} placeholder="e.g. Remote React Jobs" />
          </div>
          <div className="form-row">
            <div className="form-group">
              <label>Keywords</label>
              <input type="text" value={newSearch} onChange={e => setNewSearch(e.target.value)} placeholder="e.g. React, frontend" />
            </div>
            <div className="form-group">
              <label>Location</label>
              <input type="text" value={newLocation} onChange={e => setNewLocation(e.target.value)} placeholder="e.g. San Francisco" />
            </div>
          </div>
          <div className="form-row">
            <div className="form-group">
              <label>Job Type</label>
              <select value={newJobType} onChange={e => setNewJobType(e.target.value)}>
                <option value="">Any</option>
                <option value="Full-time">Full-time</option>
                <option value="Part-time">Part-time</option>
                <option value="Contract">Contract</option>
                <option value="Internship">Internship</option>
              </select>
            </div>
            <div className="form-group">
              <label>Email Frequency</label>
              <select value={newFrequency} onChange={e => setNewFrequency(e.target.value)}>
                <option value="immediate">Immediate</option>
                <option value="daily">Daily Digest</option>
                <option value="weekly">Weekly Digest</option>
              </select>
            </div>
          </div>
          <div className="form-group" style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
            <label style={{ margin: 0 }}>Remote Only</label>
            <input type="checkbox" checked={newIsRemote} onChange={e => setNewIsRemote(e.target.checked)} style={{ width: 20, height: 20 }} />
          </div>
          <div style={{ marginTop: 16 }}>
            <button className="btn btn-primary" onClick={handleCreate} disabled={creating}>
              {creating ? 'Saving...' : 'Save Search'}
            </button>
          </div>
        </div>
      )}

      {searches.length === 0 ? (
        <p style={{ color: '#616161' }}>
          No saved searches yet. Create one to get email alerts for new matching jobs.
        </p>
      ) : (
        <div>
          {searches.map(s => (
            <div key={s.id} className="saved-search-card">
              <div className="saved-search-info">
                <div className="saved-search-name">{s.name}</div>
                <div className="saved-search-meta">
                  {parseFilters(s.filtersJson)} · {s.emailFrequency}
                  {s.lastRunAt && <> · Last run {new Date(s.lastRunAt).toLocaleDateString()}</>}
                </div>
              </div>
              <div className="saved-search-actions">
                <button className="btn btn-outline btn-sm" onClick={() => handleRun(s.id)}>Run Now</button>
                <button className="btn btn-text btn-sm" onClick={() => handleDelete(s.id)} style={{ color: '#e44' }}>Delete</button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
