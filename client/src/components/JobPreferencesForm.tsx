import { useState, useEffect, useCallback, useRef } from 'react'
import { useUser } from '@clerk/react'
import api from '../api/axios'

interface UserProfile {
  skills: string[]
  seniorityLevel: string | null
  desiredSalaryMin: number | null
  desiredSalaryMax: number | null
  isOpenToRemote: boolean
  preferredLocation: string | null
  preferredJobType: string | null
  emailOnMatch: boolean
  minimumMatchScore: number
  emailFrequency: string
}

const SENIORITY_OPTIONS = ['Junior', 'Mid-Level', 'Senior', 'Lead', 'Manager', 'Director']
const JOB_TYPE_OPTIONS = ['Full-Time', 'Part-Time', 'Contract', 'Internship', 'Temporary']
const FREQUENCY_OPTIONS = [
  { value: 'immediate', label: 'Immediate' },
  { value: 'daily', label: 'Daily Digest' },
  { value: 'weekly', label: 'Weekly Digest' },
]
const SCORE_OPTIONS = [10, 20, 30, 50, 70, 90]

export default function JobPreferencesForm() {
  const { user } = useUser()
  const [profile, setProfile] = useState<UserProfile | null>(null)
  const [availableSkills, setAvailableSkills] = useState<string[]>([])
  const [dropdownOpen, setDropdownOpen] = useState(false)
  const [filter, setFilter] = useState('')
  const dropdownRef = useRef<HTMLDivElement>(null)
  const [saving, setSaving] = useState(false)
  const [saved, setSaved] = useState(false)

  useEffect(() => {
    const fetch = async () => {
      try {
        const [profileRes, skillsRes] = await Promise.all([
          api.get('/profile'),
          api.get('/profile/skills'),
        ])
        setProfile(profileRes.data)
        setAvailableSkills(skillsRes.data)
      } catch (err) {
        console.error('Failed to load profile', err)
      }
    }
    fetch()
  }, [])

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target as Node)) {
        setDropdownOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  const update = useCallback((partial: Partial<UserProfile>) => {
    setProfile(p => p ? { ...p, ...partial } : null)
    setSaved(false)
  }, [])

  const toggleSkill = useCallback((skill: string) => {
    if (!profile) return
    const skills = profile.skills
    if (skills.includes(skill)) {
      update({ skills: skills.filter(s => s !== skill) })
    } else {
      update({ skills: [...skills, skill] })
    }
    setFilter('')
  }, [profile, update])

  const filteredSkills = availableSkills.filter(s =>
    s.toLowerCase().includes(filter.toLowerCase())
  )

  const selectedSkills = profile?.skills ?? []

  const handleSave = async () => {
    if (!profile) return
    setSaving(true)
    try {
      await api.put('/profile', profile)
      setSaved(true)
      window.dispatchEvent(new CustomEvent('preferences-saved'))
      setTimeout(() => setSaved(false), 3000)
    } catch (err) {
      console.error('Failed to save profile', err)
    } finally {
      setSaving(false)
    }
  }

  if (!profile) return <div style={{ padding: 24, color: '#888' }}>Loading...</div>

  return (
    <div style={{ padding: '0 0 24px' }}>
      <h3 style={{ marginBottom: 24, fontSize: 20 }}>Job Preferences</h3>

      <div className="form-group" ref={dropdownRef}>
        <label>Skills (click to select)</label>
        <div style={{ position: 'relative' }}>
          <div
            onClick={() => setDropdownOpen(!dropdownOpen)}
            style={{
              padding: '10px 14px',
              border: '1px solid var(--hairline)',
              borderRadius: 8,
              cursor: 'pointer',
              fontSize: 14,
              color: selectedSkills.length === 0 ? '#999' : 'var(--ink)',
              background: 'var(--canvas)',
              minHeight: 44,
              display: 'flex',
              alignItems: 'center',
              flexWrap: 'wrap',
              gap: 4,
            }}
          >
            {selectedSkills.length === 0
              ? 'Select skills...'
              : selectedSkills.slice(0, 5).map(s => (
                  <span key={s} className="tag" style={{ cursor: 'pointer', fontSize: 12 }}
                    onClick={e => { e.stopPropagation(); toggleSkill(s) }}>
                    {s} ✕
                  </span>
                ))
            }
            {selectedSkills.length > 5 && (
              <span style={{ fontSize: 12, color: '#888' }}>+{selectedSkills.length - 5} more</span>
            )}
          </div>

          {dropdownOpen && (
            <div style={{
              position: 'absolute',
              top: '100%',
              left: 0,
              right: 0,
              zIndex: 50,
              background: 'var(--canvas)',
              border: '1px solid var(--hairline)',
              borderRadius: 8,
              marginTop: 4,
              maxHeight: 260,
              overflow: 'hidden',
              display: 'flex',
              flexDirection: 'column',
              boxShadow: '0 4px 12px rgba(0,0,0,0.1)',
            }}>
              <input
                type="text"
                value={filter}
                onChange={e => setFilter(e.target.value)}
                placeholder="Filter skills..."
                style={{
                  padding: '10px 12px',
                  border: 'none',
                  borderBottom: '1px solid var(--hairline)',
                  outline: 'none',
                  fontSize: 14,
                }}
              />
              <div style={{ overflowY: 'auto', flex: 1 }}>
                {filteredSkills.map(s => (
                  <label
                    key={s}
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: 8,
                      padding: '8px 12px',
                      cursor: 'pointer',
                      fontSize: 14,
                      background: selectedSkills.includes(s) ? 'var(--pale-blue)' : 'transparent',
                    }}
                  >
                    <input
                      type="checkbox"
                      checked={selectedSkills.includes(s)}
                      onChange={() => toggleSkill(s)}
                      style={{ width: 16, height: 16 }}
                    />
                    {s}
                  </label>
                ))}
                {filteredSkills.length === 0 && (
                  <div style={{ padding: 12, color: '#888', fontSize: 14, textAlign: 'center' }}>
                    No skills found
                  </div>
                )}
              </div>
            </div>
          )}
        </div>
      </div>

      <div className="form-row">
        <div className="form-group">
          <label>Seniority Level</label>
          <select value={profile.seniorityLevel ?? ''} onChange={e => update({ seniorityLevel: e.target.value || null })}>
            <option value="">Any</option>
            {SENIORITY_OPTIONS.map(o => <option key={o} value={o}>{o}</option>)}
          </select>
        </div>
        <div className="form-group">
          <label>Preferred Job Type</label>
          <select value={profile.preferredJobType ?? ''} onChange={e => update({ preferredJobType: e.target.value || null })}>
            <option value="">Any</option>
            {JOB_TYPE_OPTIONS.map(o => <option key={o} value={o}>{o}</option>)}
          </select>
        </div>
      </div>

      <div className="form-row">
        <div className="form-group">
          <label>Desired Salary Min</label>
          <input
            type="number"
            value={profile.desiredSalaryMin ?? ''}
            onChange={e => update({ desiredSalaryMin: e.target.value ? Number(e.target.value) : null })}
            placeholder="e.g. 80000"
          />
        </div>
        <div className="form-group">
          <label>Desired Salary Max</label>
          <input
            type="number"
            value={profile.desiredSalaryMax ?? ''}
            onChange={e => update({ desiredSalaryMax: e.target.value ? Number(e.target.value) : null })}
            placeholder="e.g. 150000"
          />
        </div>
      </div>

      <div className="form-group">
        <label>Preferred Location</label>
        <input
          type="text"
          value={profile.preferredLocation ?? ''}
          onChange={e => update({ preferredLocation: e.target.value || null })}
          placeholder="e.g. San Francisco, CA, or United States"
        />
      </div>

      <div className="form-group" style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
        <label style={{ margin: 0 }}>Open to Remote</label>
        <input
          type="checkbox"
          checked={profile.isOpenToRemote}
          onChange={e => update({ isOpenToRemote: e.target.checked })}
          style={{ width: 20, height: 20 }}
        />
      </div>

      <hr style={{ border: 'none', borderTop: '1px solid var(--hairline)', margin: '24px 0' }} />

      <h4 style={{ marginBottom: 16, fontSize: 16 }}>Email Notifications</h4>

      <div className="form-group" style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
        <label style={{ margin: 0 }}>Email me when jobs match my profile</label>
        <input
          type="checkbox"
          checked={profile.emailOnMatch}
          onChange={e => update({ emailOnMatch: e.target.checked })}
          style={{ width: 20, height: 20 }}
        />
      </div>

      {profile.emailOnMatch && (
        <>
          <div className="form-group">
            <label>Minimum Match Score</label>
            <div style={{ display: 'flex', gap: 8 }}>
              {SCORE_OPTIONS.map(s => (
                <button
                  key={s}
                  onClick={() => update({ minimumMatchScore: s })}
                  style={{
                    padding: '8px 16px',
                    borderRadius: 32,
                    border: `2px solid ${profile.minimumMatchScore === s ? 'var(--primary)' : 'var(--hairline)'}`,
                    background: profile.minimumMatchScore === s ? 'var(--primary)' : 'transparent',
                    color: profile.minimumMatchScore === s ? 'white' : 'var(--ink)',
                    cursor: 'pointer',
                    fontWeight: 500,
                    fontSize: 14,
                  }}
                >
                  {s}%
                </button>
              ))}
            </div>
          </div>

          <div className="form-group">
            <label>Email Frequency</label>
            <div style={{ display: 'flex', gap: 16, flexDirection: 'row' }}>
              {FREQUENCY_OPTIONS.map(opt => (
                <label key={opt.value} style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 8,
                  cursor: 'pointer',
                  fontSize: 14,
                  padding: '8px 16px',
                  borderRadius: 8,
                  border: `2px solid ${profile.emailFrequency === opt.value ? 'var(--primary)' : 'var(--hairline)'}`,
                  background: profile.emailFrequency === opt.value ? 'var(--soft-stone)' : 'transparent',
                }}>
                  <input
                    type="radio"
                    name="frequency"
                    value={opt.value}
                    checked={profile.emailFrequency === opt.value}
                    onChange={e => update({ emailFrequency: e.target.value })}
                    style={{ margin: 0 }}
                  />
                  {opt.label}
                </label>
              ))}
            </div>
          </div>
        </>
      )}

      <div style={{ marginTop: 24, display: 'flex', gap: 12, alignItems: 'center' }}>
        <button className="btn btn-primary" onClick={handleSave} disabled={saving}>
          {saving ? 'Saving...' : 'Save Preferences'}
        </button>
        {saved && <span style={{ color: 'var(--deep-green)', fontSize: 14 }}>Saved!</span>}
      </div>
    </div>
  )
}
