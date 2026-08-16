import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { apiFetch } from '@/shared/api'

type TokenResponse = {
  accessToken: string
  refreshToken: string
  expiresIn: number
  tokenType: string
}

export function useAuth() {
  const router = useRouter()
  const mode = ref<'login' | 'register'>('login')
  const email = ref('')
  const password = ref('')
  const firstName = ref('')
  const lastName = ref('')
  const error = ref<string | null>(null)
  const loading = ref(false)

  async function submit() {
    error.value = null
    loading.value = true
    try {
      if (mode.value === 'register') {
        await apiFetch('/api/passport/v1/auth/register', {
          method: 'POST',
          body: JSON.stringify({
            email: email.value,
            password: password.value,
            firstName: firstName.value,
            lastName: lastName.value,
          }),
        })
      }
      const tokens = await apiFetch<TokenResponse>('/api/passport/v1/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email: email.value, password: password.value }),
      })
      localStorage.setItem('auth_token', tokens.accessToken)
      localStorage.setItem('refresh_token', tokens.refreshToken)
      await router.push('/')
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Ошибка входа'
    } finally {
      loading.value = false
    }
  }

  return { mode, email, password, firstName, lastName, error, loading, submit }
}
