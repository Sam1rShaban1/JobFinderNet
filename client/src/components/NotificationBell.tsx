import { useState, useEffect, useRef } from 'react'
import { Link } from 'react-router-dom'
import api from '../api/axios'

interface Notification {
  id: number
  title: string
  message: string
  isRead: boolean
  link?: string
  createdAt: string
}

export default function NotificationBell() {
  const [notifications, setNotifications] = useState<Notification[]>([])
  const [unreadCount, setUnreadCount] = useState(0)
  const [open, setOpen] = useState(false)
  const ref = useRef<HTMLDivElement>(null)

  const fetchUnread = () => {
    api.get('/notifications/unread-count')
      .then(res => setUnreadCount(res.data.count))
      .catch(() => {})
  }

  const fetchNotifications = () => {
    api.get('/notifications?limit=10')
      .then(res => setNotifications(res.data))
      .catch(() => {})
  }

  useEffect(() => {
    fetchUnread()
    const interval = setInterval(fetchUnread, 30000)
    return () => clearInterval(interval)
  }, [])

  useEffect(() => {
    const handleClick = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClick)
    return () => document.removeEventListener('mousedown', handleClick)
  }, [])

  const toggleOpen = () => {
    if (!open) {
      fetchNotifications()
    }
    setOpen(!open)
  }

  const markAsRead = async (id: number) => {
    await api.put(`/notifications/${id}/read`)
    setNotifications(prev => prev.map(n => n.id === id ? { ...n, isRead: true } : n))
    setUnreadCount(prev => Math.max(0, prev - 1))
  }

  const markAllAsRead = async () => {
    await api.put('/notifications/read-all')
    setNotifications(prev => prev.map(n => ({ ...n, isRead: true })))
    setUnreadCount(0)
  }

  return (
    <div ref={ref} style={{ position: 'relative' }}>
      <button
        onClick={toggleOpen}
        style={{
          background: 'none',
          border: 'none',
          cursor: 'pointer',
          position: 'relative',
          padding: 4,
          color: 'var(--ink)',
        }}
        aria-label="Notifications"
      >
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
          <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
          <path d="M13.73 21a2 2 0 0 1-3.46 0" />
        </svg>
        {unreadCount > 0 && (
          <span style={{
            position: 'absolute',
            top: 0,
            right: 0,
            background: 'var(--coral)',
            color: '#fff',
            fontSize: 10,
            fontWeight: 700,
            borderRadius: '50%',
            width: 16,
            height: 16,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
          }}>
            {unreadCount > 9 ? '9+' : unreadCount}
          </span>
        )}
      </button>

      {open && (
        <div style={{
          position: 'absolute',
          top: '100%',
          right: 0,
          width: 360,
          maxHeight: 400,
          overflowY: 'auto',
          background: '#fff',
          border: '1px solid var(--hairline)',
          borderRadius: 12,
          boxShadow: '0 4px 24px rgba(0,0,0,0.12)',
          zIndex: 200,
          marginTop: 8,
        }}>
          <div style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            padding: '12px 16px',
            borderBottom: '1px solid var(--hairline)',
          }}>
            <span style={{ fontWeight: 600, fontSize: 14 }}>Notifications</span>
            {unreadCount > 0 && (
              <button
                onClick={markAllAsRead}
                style={{
                  background: 'none',
                  border: 'none',
                  color: 'var(--action-blue)',
                  cursor: 'pointer',
                  fontSize: 13,
                  textDecoration: 'underline',
                }}
              >
                Mark all read
              </button>
            )}
          </div>

          {notifications.length === 0 ? (
            <div style={{ padding: 24, textAlign: 'center', color: 'var(--body-muted)', fontSize: 14 }}>
              No notifications yet.
            </div>
          ) : (
            notifications.map(n => (
              <div
                key={n.id}
                onClick={() => !n.isRead && markAsRead(n.id)}
                style={{
                  padding: '12px 16px',
                  borderBottom: '1px solid var(--hairline)',
                  cursor: n.link ? 'pointer' : 'default',
                  background: n.isRead ? 'transparent' : 'var(--pale-blue)',
                  transition: 'background 0.15s',
                }}
              >
                {n.link ? (
                  <Link
                    to={n.link}
                    onClick={() => setOpen(false)}
                    style={{ textDecoration: 'none', color: 'inherit' }}
                  >
                    <NotificationContent notification={n} />
                  </Link>
                ) : (
                  <NotificationContent notification={n} />
                )}
              </div>
            ))
          )}
        </div>
      )}
    </div>
  )
}

function NotificationContent({ notification }: { notification: Notification }) {
  return (
    <div>
      <div style={{ fontWeight: 600, fontSize: 13, marginBottom: 2, color: 'var(--primary)' }}>
        {notification.title}
      </div>
      <div style={{ fontSize: 13, color: 'var(--body-muted)' }}>
        {notification.message}
      </div>
      <div style={{ fontSize: 11, color: 'var(--muted)', marginTop: 4 }}>
        {new Date(notification.createdAt).toLocaleDateString('en-US', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })}
      </div>
    </div>
  )
}
