import axios from 'axios'

const API_BASE = '/api'

const api = axios.create({
  baseURL: API_BASE,
  headers: { 'Content-Type': 'application/json' },
})

let _getToken: (() => Promise<string | null>) | null = null
let _unauthorizedHandler: (() => void) | null = null

export function setGetToken(fn: () => Promise<string | null>) {
  _getToken = fn
}

export function onUnauthorized(handler: () => void) {
  _unauthorizedHandler = handler
}

api.interceptors.request.use(async (config) => {
  if (_getToken) {
    try {
      const token = await _getToken()
      if (token) {
        config.headers.Authorization = `Bearer ${token}`
      }
    } catch {
      // token fetch failed, proceed without auth
    }
  }
  return config
})

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 && !error.config?.url?.includes('/auth/me')) {
      _unauthorizedHandler?.()
    }
    return Promise.reject(error)
  }
)

export default api
