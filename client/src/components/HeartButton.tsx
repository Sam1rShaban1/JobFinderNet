import { useState, useEffect } from 'react'
import toast from 'react-hot-toast'
import api from '../api/axios'

interface HeartButtonProps {
  jobId: number
  initialSaved?: boolean
  onChange?: (saved: boolean) => void
}

export default function HeartButton({ jobId, initialSaved = false, onChange }: HeartButtonProps) {
  const [saved, setSaved] = useState(initialSaved)

  useEffect(() => {
    setSaved(initialSaved)
  }, [initialSaved])

  const toggle = async (e: React.MouseEvent) => {
    e.preventDefault()
    e.stopPropagation()
    const prev = saved
    setSaved(!saved)
    onChange?.(!saved)
    try {
      if (!prev) {
        await api.post(`/savedjobs/${jobId}`)
        toast.success('Job saved')
      } else {
        await api.delete(`/savedjobs/${jobId}`)
        toast.success('Job unsaved')
      }
    } catch {
      setSaved(prev)
      onChange?.(prev)
      toast.error('Failed to update')
    }
  }

  return (
    <button
      className="heart-btn"
      onClick={toggle}
      aria-label={saved ? 'Unsave job' : 'Save job'}
      title={saved ? 'Unsave job' : 'Save job'}
    >
      <svg width="20" height="20" viewBox="0 0 24 24" fill={saved ? 'var(--coral)' : 'none'} stroke={saved ? 'var(--coral)' : 'currentColor'} strokeWidth="2">
        <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z" />
      </svg>
    </button>
  )
}
