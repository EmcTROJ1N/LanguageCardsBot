import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { userApi } from '@/shared/api'
import type { Profile } from '@/entities/user'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<Profile | null>(null)

  const isLoggedIn = computed(() => user.value !== null)

  async function init(): Promise<void> {
    const token = localStorage.getItem('auth_token')
    if (!token) return
    try {
      user.value = await userApi.getProfile()
    } catch {
      user.value = null
    }
  }

  async function login(email: string, password: string): Promise<void> {
    const tokens = await userApi.login(email, password)
    localStorage.setItem('auth_token', tokens.accessToken)
    localStorage.setItem('refresh_token', tokens.refreshToken)
    user.value = await userApi.getProfile()
  }

  async function register(
    email: string,
    password: string,
    firstName: string,
    lastName: string,
  ): Promise<void> {
    await userApi.register(email, password, firstName, lastName)
    await login(email, password)
  }

  function logout(): void {
    localStorage.removeItem('auth_token')
    localStorage.removeItem('refresh_token')
    user.value = null
  }

  return { user, isLoggedIn, init, login, register, logout }
})
