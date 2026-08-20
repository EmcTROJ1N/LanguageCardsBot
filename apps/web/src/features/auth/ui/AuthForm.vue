<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../model/useAuthStore'
import { AppField, AppBtn } from '@/shared/ui'

const props = defineProps<{ mode: 'login' | 'register' }>()

const router = useRouter()
const authStore = useAuthStore()

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
    if (props.mode === 'register') {
      await authStore.register(email.value, password.value, firstName.value, lastName.value)
    } else {
      await authStore.login(email.value, password.value)
    }
    await router.push('/')
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Произошла ошибка'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="auth__right">
    <div class="tabs">
      <button
        class="tab"
        :class="{ active: mode === 'login' }"
        type="button"
        @click="router.push('/login')"
      >Вход</button>
      <button
        class="tab"
        :class="{ active: mode === 'register' }"
        type="button"
        @click="router.push('/register')"
      >Регистрация</button>
    </div>

    <form class="form" @submit.prevent="submit">
      <div v-if="mode === 'register'" class="grid-2">
        <AppField label="Имя">
          <input v-model="firstName" type="text" placeholder="Имя" required />
        </AppField>
        <AppField label="Фамилия">
          <input v-model="lastName" type="text" placeholder="Фамилия" required />
        </AppField>
      </div>

      <AppField label="Email">
        <input v-model="email" type="email" placeholder="you@example.com" required />
      </AppField>

      <AppField label="Пароль" :hint="mode === 'register' ? 'не короче 8 символов' : undefined">
        <input v-model="password" type="password" placeholder="••••••••" required />
        <template v-if="mode === 'login'">
          <a class="hint" href="#">Забыли пароль?</a>
        </template>
      </AppField>

      <p v-if="error" class="error">{{ error }}</p>

      <AppBtn variant="ochre" size="lg" type="submit" class="full" :disabled="loading">
        <span v-if="loading">…</span>
        <template v-else>
          {{ mode === 'login' ? 'Войти' : 'Создать аккаунт' }}
          <span class="mono">↵</span>
        </template>
      </AppBtn>
    </form>
  </div>
</template>

<style scoped>
.auth__right { padding: 56px 48px; display: flex; flex-direction: column; gap: 24px; justify-content: center; }
.tabs { display: inline-flex; border-bottom: 2px solid var(--ink); gap: 24px; padding-bottom: 2px; }
.tab { border: 0; background: transparent; font-family: var(--serif); font-size: 22px; color: var(--ink-mute); cursor: pointer; padding: 6px 0; position: relative; font-weight: 500; }
.tab.active { color: var(--ink); }
.tab.active::after { content: ''; position: absolute; left: 0; right: 0; bottom: -4px; height: 3px; background: var(--ochre); }
.form { display: flex; flex-direction: column; gap: 20px; }
.grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
.hint { color: var(--ink-mute); font-size: 11px; border: 0; }
.hint:hover { color: var(--ochre); }
.error { color: #c0392b; font-size: 13px; margin: 0; }
:deep(.btn.full) { width: 100%; }
</style>
