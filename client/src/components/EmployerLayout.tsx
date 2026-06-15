import { NavLink } from 'react-router-dom'

const NAV_ITEMS = [
  { to: '/employer-dashboard', label: 'Dashboard', icon: '▣' },
  { to: '/my-jobs', label: 'My Jobs', icon: '☰' },
  { to: '/create-job', label: 'Post New Job', icon: '+' },
]

export default function EmployerLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="employer-layout">
      <aside className="employer-sidebar">
        <p className="employer-sidebar-label">Employer</p>
        {NAV_ITEMS.map(item => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.to !== '/create-job'}
            className={({ isActive }) =>
              `employer-sidebar-link${isActive ? ' active' : ''}`
            }
          >
            <span style={{ fontSize: 14, width: 20, textAlign: 'center', flexShrink: 0 }}>{item.icon}</span>
            {item.label}
          </NavLink>
        ))}
      </aside>
      <div className="employer-content">
        {children}
      </div>
    </div>
  )
}
