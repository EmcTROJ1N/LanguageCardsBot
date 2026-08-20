export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const headers: Record<string, string> = { 'Content-Type': 'application/json' }
  const token = localStorage.getItem('auth_token')
  if (token) headers['Authorization'] = `Bearer ${token}`

  const res = await fetch(path, { ...init, headers: { ...headers, ...init?.headers } })

  if (res.status === 401) {
    localStorage.removeItem('auth_token')
    localStorage.removeItem('refresh_token')
    window.location.href = '/login'
    await new Promise(() => {})  // hang until navigation completes
    return undefined as T        // unreachable; satisfies TypeScript
  }

  if (!res.ok) throw new Error(`HTTP ${res.status} ${res.statusText}`)
  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}
