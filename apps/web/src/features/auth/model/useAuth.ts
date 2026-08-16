import { ref } from 'vue'
import { useRouter } from 'vue-router'

export function useAuth() {
  const router = useRouter()
  const mode = ref<'login' | 'register'>('login')
  const email = ref('')
  const password = ref('')
  const firstName = ref('')
  const lastName = ref('')

  async function submit() {
    // POST /api/passport/v1/auth/login or /register — implement when backend is ready
    await router.push('/')
  }

  return { mode, email, password, firstName, lastName, submit }
}
