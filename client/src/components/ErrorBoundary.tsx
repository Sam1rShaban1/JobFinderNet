import { Component } from 'react'
import { Link } from 'react-router-dom'

interface Props {
  children: React.ReactNode
}

interface State {
  hasError: boolean
}

export default class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props)
    this.state = { hasError: false }
  }

  static getDerivedStateFromError(): State {
    return { hasError: true }
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="container" style={{ paddingTop: 80, textAlign: 'center' }}>
          <h1 style={{ fontSize: 48, marginBottom: 16 }}>Something went wrong</h1>
          <p style={{ color: '#616161', marginBottom: 24 }}>
            An unexpected error occurred. Please try refreshing the page.
          </p>
          <div style={{ display: 'flex', gap: 12, justifyContent: 'center' }}>
            <button className="btn btn-primary" onClick={() => window.location.reload()}>
              Refresh Page
            </button>
            <Link to="/" className="btn btn-outline">Go Home</Link>
          </div>
        </div>
      )
    }

    return this.props.children
  }
}
