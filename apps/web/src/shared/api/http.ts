function authHeaders(): Record<string, string> {
  const token = localStorage.getItem('auth_token')
  const headers: Record<string, string> = { 'Content-Type': 'application/json' }
  if (token) headers['Authorization'] = `Bearer ${token}`
  return headers
}

// Single in-flight refresh to avoid concurrent races
let refreshPromise: Promise<boolean> | null = null

async function tryRefresh(): Promise<boolean> {
  if (refreshPromise) return refreshPromise
  refreshPromise = (async () => {
    const refreshToken = localStorage.getItem('refresh_token')
    if (!refreshToken) return false
    try {
      const res = await fetch('/api/passport/v1/auth/refresh', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(refreshToken),
      })
      if (!res.ok) return false
      const tokens = await res.json() as { accessToken: string; refreshToken: string }
      localStorage.setItem('auth_token', tokens.accessToken)
      localStorage.setItem('refresh_token', tokens.refreshToken)
      return true
    } catch {
      return false
    } finally {
      refreshPromise = null
    }
  })()
  return refreshPromise
}

function redirectToLogin(): never {
  localStorage.removeItem('auth_token')
  localStorage.removeItem('refresh_token')
  window.location.href = '/login'
  throw new Error('Unauthorized')
}

const PUBLIC_ERROR_MESSAGES: Record<number, string> = {
  400: 'Проверьте введённые данные.',
  401: 'Неверный email или пароль.',
  403: 'Доступ запрещён.',
  404: 'Ресурс не найден.',
  503: 'Сервис временно недоступен. Попробуйте позже.',
  500: 'Произошла непредвиденная ошибка. Попробуйте позже.',
}

export async function publicFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(path, {
    ...init,
    headers: { 'Content-Type': 'application/json', ...init?.headers },
  })
  if (res.ok) {
    if (res.status === 204) return undefined as T
    return res.json() as Promise<T>
  }
  let message = PUBLIC_ERROR_MESSAGES[res.status] ?? `HTTP ${res.status} ${res.statusText}`
  try {
    const body = await res.json() as { error?: string }
    if (body.error) message = body.error
  } catch {}
  throw new Error(message)
}

export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(path, { ...init, headers: { ...authHeaders(), ...init?.headers } })
  if (res.status === 401) {
    if (!(await tryRefresh())) redirectToLogin()
    const retry = await fetch(path, { ...init, headers: { ...authHeaders(), ...init?.headers } })
    if (retry.status === 401) redirectToLogin()
    if (!retry.ok) throw new Error(`HTTP ${retry.status} ${retry.statusText}`)
    if (retry.status === 204) return undefined as T
    return retry.json() as Promise<T>
  }
  if (!res.ok) throw new Error(`HTTP ${res.status} ${res.statusText}`)
  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}

export async function apiDownload(path: string, filename: string): Promise<void> {
  const res = await fetch(path, { headers: authHeaders() })
  if (res.status === 401) {
    if (!(await tryRefresh())) redirectToLogin()
    const retry = await fetch(path, { headers: authHeaders() })
    if (retry.status === 401) redirectToLogin()
    if (!retry.ok) throw new Error(`HTTP ${retry.status} ${retry.statusText}`)
    const blob = await retry.blob()
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = filename
    a.click()
    URL.revokeObjectURL(url)
    return
  }
  if (!res.ok) throw new Error(`HTTP ${res.status} ${res.statusText}`)
  const blob = await res.blob()
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  a.click()
  URL.revokeObjectURL(url)
}
