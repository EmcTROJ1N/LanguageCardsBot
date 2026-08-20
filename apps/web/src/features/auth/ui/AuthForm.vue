<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../model/useAuthStore'

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
  <div class="card">
    <div class="brand">
      <span class="brand-name">Language Cards</span>
      <span class="brand-sub mono">Cabinet · v0.1</span>
    </div>

    <div class="toggle" role="tablist">
      <button
        class="toggle-btn"
        :class="{ active: mode === 'login' }"
        type="button"
        role="tab"
        @click="router.push('/login')"
      >Вход</button>
      <button
        class="toggle-btn"
        :class="{ active: mode === 'register' }"
        type="button"
        role="tab"
        @click="router.push('/register')"
      >Регистрация</button>
    </div>

    <form class="form" @submit.prevent="submit">
      <div v-if="mode === 'register'" class="row-2">
        <div class="field">
          <input
            id="firstName"
            v-model="firstName"
            type="text"
            placeholder=" "
            required
            autocomplete="given-name"
          />
          <label for="firstName">Имя</label>
        </div>
        <div class="field">
          <input
            id="lastName"
            v-model="lastName"
            type="text"
            placeholder=" "
            required
            autocomplete="family-name"
          />
          <label for="lastName">Фамилия</label>
        </div>
      </div>

      <div class="field">
        <input
          id="email"
          v-model="email"
          type="email"
          placeholder=" "
          required
          autocomplete="email"
        />
        <label for="email">Email</label>
      </div>

      <div class="field">
        <input
          id="password"
          v-model="password"
          type="password"
          placeholder=" "
          required
          :autocomplete="mode === 'register' ? 'new-password' : 'current-password'"
        />
        <label for="password">Пароль</label>
        <a v-if="mode === 'login'" class="forgot" href="#">Забыли?</a>
      </div>

      <p v-if="error" class="error" role="alert">{{ error }}</p>

      <button class="submit-btn" type="submit" :disabled="loading">
        <span v-if="loading" class="mono">…</span>
        <template v-else>{{ mode === 'login' ? 'Войти' : 'Создать аккаунт' }}</template>
      </button>
    </form>

    <p class="footer">
      <span>{{ mode === 'login' ? 'Нет аккаунта?' : 'Уже есть аккаунт?' }}</span>
      <button
        type="button"
        class="switch-link"
        @click="router.push(mode === 'login' ? '/register' : '/login')"
      >{{ mode === 'login' ? 'Зарегистрироваться' : 'Войти' }}</button>
    </p>
  </div>
</template>

<style scoped>
.card {
  position: relative;
  z-index: 10;
  width: 100%;
  max-width: 400px;
  margin: 24px;
  background: rgba(28, 33, 42, 0.78);
  backdrop-filter: blur(28px) saturate(1.4);
  -webkit-backdrop-filter: blur(28px) saturate(1.4);
  border: 1px solid rgba(255, 255, 255, 0.07);
  border-radius: 20px;
  padding: 36px 32px 28px;
  box-shadow: 0 24px 64px rgba(0, 0, 0, 0.55), inset 0 1px 0 rgba(255, 255, 255, 0.06);
  display: flex;
  flex-direction: column;
  gap: 28px;
  animation: card-in 0.35s ease both;
}

@keyframes card-in {
  from { opacity: 0; transform: translateY(10px); }
  to   { opacity: 1; transform: translateY(0); }
}

/* Brand */
.brand {
  display: flex;
  flex-direction: column;
  gap: 3px;
}
.brand-name {
  font-family: var(--serif);
  font-size: 20px;
  font-weight: 700;
  color: var(--ink);
  line-height: 1.1;
}
.brand-sub {
  font-size: 11px;
  color: var(--ink-mute);
  letter-spacing: 0.1em;
}

/* Toggle */
.toggle {
  display: flex;
  background: rgba(255, 255, 255, 0.06);
  border-radius: 100px;
  padding: 3px;
  gap: 2px;
}
.toggle-btn {
  flex: 1;
  border: none;
  background: transparent;
  color: var(--ink-mute);
  font-family: var(--sans);
  font-size: 13px;
  font-weight: 500;
  padding: 7px 12px;
  border-radius: 100px;
  cursor: pointer;
  transition: background 0.18s, color 0.18s;
}
.toggle-btn.active {
  background: rgba(255, 255, 255, 0.11);
  color: var(--ink);
}

/* Form */
.form {
  display: flex;
  flex-direction: column;
  gap: 22px;
}
.row-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

/* Floating-label fields */
.field {
  position: relative;
}
.field input {
  width: 100%;
  background: transparent;
  border: none;
  border-bottom: 1px solid rgba(255, 255, 255, 0.14);
  border-radius: 0;
  padding: 20px 0 8px;
  font-family: var(--sans);
  font-size: 15px;
  color: var(--ink);
  outline: none;
  transition: border-color 0.2s;
  -webkit-appearance: none;
}
.field input:focus {
  border-bottom-color: var(--ochre);
}
/* Suppress browser autofill yellow background */
.field input:-webkit-autofill,
.field input:-webkit-autofill:focus {
  -webkit-box-shadow: 0 0 0 100px #1c212a inset;
  -webkit-text-fill-color: var(--ink);
  transition: background-color 9999s ease;
}
.field label {
  position: absolute;
  top: 18px;
  left: 0;
  font-family: var(--sans);
  font-size: 14px;
  color: var(--ink-mute);
  pointer-events: none;
  transition: top 0.18s, font-size 0.18s, color 0.18s, letter-spacing 0.18s;
}
.field input:focus ~ label,
.field input:not(:placeholder-shown) ~ label {
  top: 2px;
  font-size: 10px;
  color: var(--ochre);
  letter-spacing: 0.06em;
  text-transform: uppercase;
}

.forgot {
  position: absolute;
  right: 0;
  bottom: 10px;
  font-size: 11px;
  color: var(--ink-mute);
  text-decoration: none;
  transition: color 0.15s;
}
.forgot:hover { color: var(--ochre); }

/* Error */
.error {
  font-size: 13px;
  color: #e07070;
  margin: -8px 0 0;
}

/* Submit */
.submit-btn {
  width: 100%;
  padding: 13px;
  border: none;
  border-radius: 10px;
  background: linear-gradient(135deg, var(--rust) 0%, var(--rust-soft) 100%);
  color: #fff;
  font-family: var(--sans);
  font-size: 14px;
  font-weight: 600;
  letter-spacing: 0.02em;
  cursor: pointer;
  transition: opacity 0.15s, transform 0.1s;
}
.submit-btn:hover:not(:disabled) {
  opacity: 0.88;
  transform: translateY(-1px);
}
.submit-btn:active:not(:disabled) {
  transform: translateY(0);
}
.submit-btn:disabled {
  opacity: 0.45;
  cursor: not-allowed;
}

/* Footer link */
.footer {
  display: flex;
  gap: 6px;
  justify-content: center;
  align-items: center;
  font-size: 13px;
  color: var(--ink-mute);
  margin: -8px 0 0;
}
.switch-link {
  border: none;
  background: none;
  padding: 0;
  font-family: var(--sans);
  font-size: 13px;
  font-weight: 500;
  color: var(--ochre-soft);
  cursor: pointer;
  transition: color 0.15s;
}
.switch-link:hover { color: var(--ochre); }
</style>
