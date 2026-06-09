import { useState, useEffect, useCallback } from 'react'
import toast from 'react-hot-toast'
import { savedSearchesApi } from '../api/axios'

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

export default function SavedSearchesForm() {
  const [searches, setSearches] = useState<SavedSearch[]>([])
  const [loading, setLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [creating, setCreating] = useState(false)
  const [newName, setNewName] = useState('')
  const [newSearch, setNewSearch] = useState('')
  const [newLocation, setNewLocation] = useState('')
  const [newJobType, setNewJobType] = useState('')
  const [newIsRemote, setNewIsRemote] = useState(false)
  const [newFrequency, setNewFrequency] = useState('daily')

  const fetchSearches = useCallback(async () => {
    setLoading(true)
    try {
      const res = await savedSearchesApi.list()
      setSearches(res.data)
    } catch {
      // silent
    } finally {
      setLoading(false)
    }
  }, [])

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
      toast.success('Deleted.')
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

  if (loading) {
    return <div style={{ padding: '16px 0', color: '#888', fontSize: 13 }}>Loading searches...</div>
  }

  return (
    <div style={{ padding: '0 4px' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <span style={{ fontSize: 13, color: '#888' }}>{searches.length} saved search{searches.length !== 1 ? 'es' : ''}</span>
        <button className="btn btn-outline btn-xs" onClick={() => setShowForm(!showForm)}>
          {showForm ? 'Cancel' : '+ New'}
        </button>
      </div>

      {showForm && (
        <div style={{ padding: 12, border: '1px solid var(--color-border)', borderRadius: 8, marginBottom: 16, background: '#f8f9fa' }}>
          <input
            type="text"
            value={newName}
            onChange={e => setNewName(e.target.value)}
            placeholder="Search name (e.g. Remote React Jobs)"
            style={{ width: '100%', padding: '6px 10px', borderRadius: 6, border: '1px solid var(--color-border)', fontSize: 12, marginBottom: 8 }}
          />
          <div style={{ display: 'flex', gap: 6, marginBottom: 8 }}>
            <input
              type="text"
              value={newSearch}
              onChange={e => setNewSearch(e.target.value)}
              placeholder="Keywords"
              style={{ flex: 1, padding: '6px 10px', borderRadius: 6, border: '1px solid var(--color-border)', fontSize: 12 }}
            />
            <input
              type="text"
              value={newLocation}
              onChange={e => setNewLocation(e.target.value)}
              placeholder="Location"
              style={{ flex: 1, padding: '6px 10px', borderRadius: 6, border: '1px solid var(--color-border)', fontSize: 12 }}
            />
          </div>
          <div style={{ display: 'flex', gap: 6, marginBottom: 8 }}>
            <select
              value={newJobType}
              onChange={e => setNewJobType(e.target.value)}
              style={{ flex: 1, padding: '6px 10px', borderRadius: 6, border: '1px solid var(--color-border)', fontSize: 12 }}
            >
              <option value="">Any type</option>
              <option value="Full-time">Full-time</option>
              <option value="Part-time">Part-time</option>
              <option value="Contract">Contract</option>
              <option value="Internship">Internship</option>
            </select>
            <select
              value={newFrequency}
              onChange={e => setNewFrequency(e.target.value)}
              style={{ flex: 1, padding: '6px 10px', borderRadius: 6, border: '1px solid var(--color-border)', fontSize: 12 }}
            >
              <option value="immediate">Immediate</option>
              <option value="daily">Daily Digest</option>
              <option value="weekly">Weekly Digest</option>
            </select>
          </div>
          <label style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: 12, marginBottom: 8, cursor: 'pointer' }}>
            <input type="checkbox" checked={newIsRemote} onChange={e => setNewIsRemote(e.target.checked)} style={{ width: 14, height: 14 }} />
            Remote only
          </label>
          <button className="btn btn-primary btn-sm btn-full" onClick={handleCreate} disabled={creating}>
            {creating ? 'Saving...' : 'Save Search'}
          </button>
        </div>
      )}

      {searches.length === 0 ? (
        <p style={{ color: '#888', fontSize: 13 }}>No saved searches yet.</p>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
          {searches.map(s => (
            <div key={s.id} style={{ padding: '10px 12px', border: '1px solid var(--color-border)', borderRadius: 8 }}>
              <div style={{ fontWeight: 600, fontSize: 13, marginBottom: 2 }}>{s.name}</div>
              <div style={{ fontSize: 11, color: '#888', marginBottom: 6 }}>
                {parseFilters(s.filtersJson)} · {s.emailFrequency}
                {s.lastRunAt && <> · Last run {new Date(s.lastRunAt).toLocaleDateString()}</>}
              </div>
              <div style={{ display: 'flex', gap: 6 }}>
                <button className="btn btn-outline btn-xs" onClick={() => handleRun(s.id)}>Run</button>
                <button className="btn btn-xs" onClick={() => handleDelete(s.id)} style={{ color: '#c62828' }}>Delete</button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}