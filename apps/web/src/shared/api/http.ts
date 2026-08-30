function authHeaders(): Record<string, string> {
  const token = localStorage.getItem('auth_token')
  const headers: Record<string, string> = { 'Content-Type': 'application/json' }
  if (token) headers['Authorization'] = `Bearer ${token}`
  return headers
}

function handle401(): never {
  localStorage.removeItem('auth_token')
  localStorage.removeItem('refresh_token')
  window.location.href = '/login'
  throw new Error('Unauthorized')
}

export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(path, { ...init, headers: { ...authHeaders(), ...init?.headers } })
  if (res.status === 401) handle401()
  if (!res.ok) throw new Error(`HTTP ${res.status} ${res.statusText}`)
  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}

export async function apiDownload(path: string, filename: string): Promise<void> {
  const res = await fetch(path, { headers: authHeaders() })
  if (res.status === 401) handle401()
  if (!res.ok) throw new Error(`HTTP ${res.status} ${res.statusText}`)
  const blob = await res.blob()
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  a.click()
  URL.revokeObjectURL(url)
}
