import { useState, useEffect, useRef } from 'react'
import api from '../api/axios'

interface CounterProps {
  end: number
  label: string
  suffix?: string
}

function Counter({ end, label, suffix = '' }: CounterProps) {
  const [count, setCount] = useState(0)
  const ref = useRef<HTMLDivElement>(null)
  const [started, setStarted] = useState(false)

  useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting && !started) {
          setStarted(true)
        }
      },
      { threshold: 0.5 }
    )
    if (ref.current) observer.observe(ref.current)
    return () => observer.disconnect()
  }, [started])

  useEffect(() => {
    if (!started) return
    const duration = 1200
    const steps = 40
    const increment = end / steps
    let current = 0
    const timer = setInterval(() => {
      current += increment
      if (current >= end) {
        setCount(end)
        clearInterval(timer)
      } else {
        setCount(Math.floor(current))
      }
    }, duration / steps)
    return () => clearInterval(timer)
  }, [started, end])

  return (
    <div ref={ref} style={{ textAlign: 'center' }}>
      <p style={{ fontSize: 40, fontWeight: 700, lineHeight: 1 }}>
        {count.toLocaleString()}{suffix}
      </p>
      <p className="micro" style={{ marginTop: 8, color: 'var(--body-muted)' }}>{label}</p>
    </div>
  )
}

export default function StatsCounter() {
  const [stats, setStats] = useState<{ totalJobs: number; totalTechnologies: number; jobsByType: Record<string, number> } | null>(null)

  useEffect(() => {
    api.get('/statistics')
      .then(res => setStats(res.data))
      .catch(() => {
        setStats({ totalJobs: 0, totalTechnologies: 0, jobsByType: {} })
      })
  }, [])

  const totalJobs = stats?.totalJobs ?? 0
  const totalTech = stats?.totalTechnologies ?? 0
  const categories = stats?.jobsByType ? Object.keys(stats.jobsByType).length : 0

  return (
    <div style={{
      display: 'grid',
      gridTemplateColumns: 'repeat(3, 1fr)',
      gap: 32,
      padding: '40px 0',
    }}>
      <Counter end={totalJobs} label="Active Jobs" />
      <Counter end={totalTech} label="Tech Skills" />
      <Counter end={categories} label="Job Categories" />
    </div>
  )
}
