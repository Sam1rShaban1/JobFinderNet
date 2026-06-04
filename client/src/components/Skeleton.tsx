export function SkeletonCard() {
  return (
    <div className="skeleton-card">
      <div className="skeleton-line skeleton-title" />
      <div className="skeleton-line skeleton-text" />
      <div className="skeleton-line skeleton-text short" />
      <div className="skeleton-line skeleton-meta" />
    </div>
  )
}

export function SkeletonDetails() {
  return (
    <div className="container" style={{ paddingTop: 40 }}>
      <div className="skeleton-line skeleton-title" style={{ width: '70%', marginBottom: 16 }} />
      <div className="skeleton-line skeleton-text" style={{ width: '40%', marginBottom: 32 }} />
      <div className="skeleton-grid" style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 16, marginBottom: 32 }}>
        {Array.from({ length: 4 }).map((_, i) => (
          <div key={i} className="skeleton-card">
            <div className="skeleton-line skeleton-text" style={{ width: '60%' }} />
            <div className="skeleton-line skeleton-text short" style={{ width: '80%' }} />
          </div>
        ))}
      </div>
      <div className="skeleton-line skeleton-text" style={{ width: '100%', marginBottom: 8 }} />
      <div className="skeleton-line skeleton-text" style={{ width: '100%', marginBottom: 8 }} />
      <div className="skeleton-line skeleton-text" style={{ width: '65%', marginBottom: 8 }} />
    </div>
  )
}

export function SkeletonList({ count = 6 }: { count?: number }) {
  return (
    <div className="job-grid">
      {Array.from({ length: count }).map((_, i) => (
        <SkeletonCard key={i} />
      ))}
    </div>
  )
}
