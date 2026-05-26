import React from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import { useAuth } from './context/AuthContext'
import Navbar from './components/Navbar'
import Login from './pages/Login'
import Register from './pages/Register'
import Jobs from './pages/Jobs'
import JobDetails from './pages/JobDetails'
import CreateJob from './pages/CreateJob'
import MyApplications from './pages/MyApplications'

function Home() {
  const { user } = useAuth()
  return (
    <div className="container" style={{ textAlign: 'center', marginTop: 60 }}>
      <h1>JobFinderNet</h1>
      <p className="subtitle">Find your next career opportunity</p>
      {!user && (
        <div className="home-cta">
          <a href="/register" className="btn btn-primary">Get Started</a>
          <a href="/login" className="btn btn-outline">Sign In</a>
        </div>
      )}
      {user && (
        <div className="home-cta">
          <a href="/jobs" className="btn btn-primary">Browse Jobs</a>
        </div>
      )}
    </div>
  )
}

function ProtectedRoute({ children, roles }: { children: React.ReactElement; roles?: string[] }) {
  const { user } = useAuth()
  if (!user) return <Navigate to="/login" />
  if (roles && !roles.includes(user.role)) return <Navigate to="/jobs" />
  return children
}

export default function App() {
  return (
    <>
      <Navbar />
      <main>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/jobs" element={<Jobs />} />
          <Route path="/jobs/:id" element={<JobDetails />} />
          <Route path="/create-job" element={
            <ProtectedRoute roles={['Employer']}><CreateJob /></ProtectedRoute>
          } />
          <Route path="/my-applications" element={
            <ProtectedRoute roles={['Applicant']}><MyApplications /></ProtectedRoute>
          } />
        </Routes>
      </main>
    </>
  )
}
