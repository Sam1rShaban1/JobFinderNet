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

export const savedSearchesApi = {
  list: () => api.get('/savedsearches'),
  create: (dto: any) => api.post('/savedsearches', dto),
  update: (id: number, dto: any) => api.put(`/savedsearches/${id}`, dto),
  delete: (id: number) => api.delete(`/savedsearches/${id}`),
  run: (id: number) => api.post(`/savedsearches/${id}/run`),
}

export const companyProfilesApi = {
  get: (id: number) => api.get(`/companyprofiles/${id}`),
  search: (q: string) => api.get(`/companyprofiles?q=${encodeURIComponent(q)}`),
  claim: (dto: any) => api.post('/companyprofiles/claim', dto),
  update: (id: number, dto: any) => api.put(`/companyprofiles/${id}`, dto),
}

export const applicationNotesApi = {
  list: (applicationId: number) => api.get(`/applications/${applicationId}/notes`),
  add: (applicationId: number, content: string) =>
    api.post(`/applications/${applicationId}/notes`, { content }),
}

export const myJobsApi = {
  list: () => api.get('/jobs/employer'),
  update: (id: number, dto: any) => api.put(`/jobs/${id}`, dto),
  toggle: (id: number) => api.post(`/jobs/${id}/toggle`),
  delete: (id: number) => api.delete(`/jobs/${id}`),
}

export const getClaimedCompany = () => api.get('/companyprofiles/my')

export const resumeApi = {
  parse: (request: { resumeText?: string; imageBase64?: string; imageMediaType?: string; isPdf?: boolean }) =>
    api.post('/resume/parse', request),
  recommendations: (request: { resumeText?: string; imageBase64?: string; imageMediaType?: string; isPdf?: boolean }, limit = 10) =>
    api.post(`/resume/recommendations?limit=${limit}`, request),
  recommendationsFromSkills: (skills: string[], limit = 10) =>
    api.post(`/resume/recommendations/from-skills?limit=${limit}`, skills),
  coverLetter: (request: { jobTitle: string; companyName: string; jobDescription?: string; hiringManager?: string; tone?: string }) =>
    api.post('/resume/cover-letter', request),
}

export default api
