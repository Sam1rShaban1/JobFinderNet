import { useState, useEffect, useRef } from 'react'

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
  return (
    <div style={{
      display: 'grid',
      gridTemplateColumns: 'repeat(3, 1fr)',
      gap: 32,
      padding: '40px 0',
    }}>
      <Counter end={962} label="Active Jobs" />
      <Counter end={117} label="Tech Skills" />
      <Counter end={12} label="Job Categories" />
    </div>
  )
}
