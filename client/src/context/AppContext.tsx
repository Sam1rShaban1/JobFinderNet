import { createContext, useContext, useEffect, useState, useRef } from 'react'
import { useAuth, useUser } from '@clerk/react'
import api, { setGetToken } from '../api/axios'

interface AppUser {
  userId: string
  email: string
  role: string
}

const AppContext = createContext<{ user: AppUser | null }>({ user: null })

export function AppProvider({ children }: { children: React.ReactNode }) {
  const { isSignedIn: authSignedIn, isLoaded: authLoaded, getToken } = useAuth()
  const { isSignedIn: userSignedIn, isLoaded: userLoaded } = useUser()
  const [appUser, setAppUser] = useState<AppUser | null>(null)

  const getTokenRef = useRef(getToken)
  getTokenRef.current = getToken

  useEffect(() => {
    setGetToken(() => getTokenRef.current())
  }, [])

  useEffect(() => {
    if (!authLoaded || !userLoaded) return
    if (!authSignedIn || !userSignedIn) {
      setAppUser(null)
      return
    }
    let cancelled = false
    api.get('/auth/me').then((res) => {
      if (cancelled) return
      if (res.data.success) {
        setAppUser({
          userId: res.data.userId,
          email: res.data.email,
          role: res.data.role,
        })
      }
    }).catch(() => {
      if (!cancelled) setAppUser(null)
    })
    return () => { cancelled = true }
  }, [authLoaded, userLoaded, authSignedIn, userSignedIn])

  return (
    <AppContext.Provider value={{ user: appUser }}>
      {children}
    </AppContext.Provider>
  )
}

export function useAppUser() {
  return useContext(AppContext)
}
