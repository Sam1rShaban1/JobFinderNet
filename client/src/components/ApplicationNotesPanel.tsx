import { useState, useEffect, useCallback } from 'react'
import api from '../api/axios'
import { useAppUser } from '../context/AppContext'

interface Note {
  id: number
  applicationId: number
  userId: string
  content: string
  createdAt: string
}

interface Props {
  applicationId: number
}

export default function ApplicationNotesPanel({ applicationId }: Props) {
  const { user } = useAppUser()
  const [notes, setNotes] = useState<Note[]>([])
  const [loading, setLoading] = useState(true)
  const [newNote, setNewNote] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

  const canAddNotes = user?.role === 'Employer' || user?.role === 'Admin'

  const fetchNotes = useCallback(async () => {
    if (!canAddNotes) {
      setLoading(false)
      return
    }
    setLoading(true)
    try {
      const res = await api.get(`/applications/${applicationId}/notes`)
      setNotes(res.data)
    } catch {
      setError('Failed to load notes.')
    } finally {
      setLoading(false)
    }
  }, [applicationId, canAddNotes])

  useEffect(() => { fetchNotes() }, [fetchNotes])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!newNote.trim()) return
    setSubmitting(true)
    try {
      const res = await api.post(`/applications/${applicationId}/notes`, { content: newNote.trim() })
      setNotes(prev => [res.data, ...prev])
      setNewNote('')
    } catch {
      setError('Failed to add note.')
    } finally {
      setSubmitting(false)
    }
  }

  if (!canAddNotes) return null

  return (
    <div style={{
      marginTop: 12,
      paddingTop: 12,
      borderTop: '1px solid var(--hairline)',
    }}>
      <p style={{ fontSize: 12, fontWeight: 600, color: 'var(--body-muted)', marginBottom: 10, textTransform: 'uppercase', letterSpacing: '0.05em' }}>
        Notes
      </p>

      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 8, marginBottom: 12 }}>
        <textarea
          value={newNote}
          onChange={e => setNewNote(e.target.value)}
          placeholder="Add a private note about this application..."
          rows={2}
          style={{
            width: '100%',
            padding: '8px 10px',
            fontSize: 13,
            borderRadius: 6,
            border: '1px solid var(--hairline)',
            resize: 'vertical',
            fontFamily: 'inherit',
            background: 'var(--surface)',
            color: 'var(--body)',
            boxSizing: 'border-box',
          }}
        />
        <button
          type="submit"
          disabled={submitting || !newNote.trim()}
          className="btn btn-primary"
          style={{ alignSelf: 'flex-end', fontSize: 13, padding: '6px 14px' }}
        >
          {submitting ? 'Saving...' : 'Add Note'}
        </button>
      </form>

      {error && <p style={{ color: '#e53e3e', fontSize: 13, marginBottom: 8 }}>{error}</p>}

      {loading ? (
        <p style={{ fontSize: 13, color: 'var(--body-muted)' }}>Loading notes...</p>
      ) : notes.length === 0 ? (
        <p style={{ fontSize: 13, color: 'var(--body-muted)', fontStyle: 'italic' }}>No notes yet.</p>
      ) : (
        <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'flex', flexDirection: 'column', gap: 8 }}>
          {notes.map(note => (
            <li key={note.id} style={{
              padding: '8px 10px',
              borderRadius: 6,
              background: 'var(--surface-raised, #f9f9f9)',
              border: '1px solid var(--hairline)',
              fontSize: 13,
            }}>
              <p style={{ margin: 0, marginBottom: 4, lineHeight: 1.5 }}>{note.content}</p>
              <p style={{ margin: 0, fontSize: 11, color: 'var(--body-muted)' }}>
                {new Date(note.createdAt).toLocaleDateString('en-US', {
                  month: 'short', day: 'numeric', year: 'numeric',
                  hour: '2-digit', minute: '2-digit',
                })}
              </p>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
