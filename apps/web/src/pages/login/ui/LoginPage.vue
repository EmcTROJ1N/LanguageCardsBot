<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import TodoBanner from '@/shared/ui/TodoBanner.vue'

const mode = ref<'login' | 'register'>('login')
const email = ref('')
const password = ref('')
const firstName = ref('')
const lastName = ref('')
const router = useRouter()

function submit() {
  // Заглушка. Реальный вызов: POST /api/passport/v1/auth/{login|register}
  router.push('/')
}
</script>

<template>
  <div class="auth">
    <div class="auth__left">
      <div class="brand">
        <span class="serif">Language</span>
        <span>Cards Cabinet</span>
      </div>
      <blockquote class="serif epigraph">
        <span class="mono">Ep.</span>
        Every word is a small door. If you walk through it enough times, it stays open.
      </blockquote>
      <div class="epigraph__attr mono">— Cabinet manifesto, 2026</div>
      <div class="marks">
        <span class="mono muted">v0.1.0 · mockup edition</span>
      </div>
    </div>

    <div class="auth__right">
      <div class="tabs">
        <button
          class="tab"
          :class="{ active: mode === 'login' }"
          @click="mode = 'login'"
        >
          Вход
        </button>
        <button
          class="tab"
          :class="{ active: mode === 'register' }"
          @click="mode = 'register'"
        >
          Регистрация
        </button>
      </div>

      <form class="form" @submit.prevent="submit">
        <div v-if="mode === 'register'" class="grid-2">
          <div class="field">
            <label>Имя</label>
            <input v-model="firstName" type="text" placeholder="Имя" />
          </div>
          <div class="field">
            <label>Фамилия</label>
            <input v-model="lastName" type="text" placeholder="Фамилия" />
          </div>
        </div>

        <div class="field">
          <label>Email</label>
          <input v-model="email" type="email" placeholder="you@example.com" />
        </div>

        <div class="field">
          <label>Пароль</label>
          <input v-model="password" type="password" placeholder="••••••••" />
          <span v-if="mode === 'register'" class="hint mono">не короче 8 символов</span>
          <a v-else class="hint" href="#">Забыли пароль?</a>
        </div>

        <button class="btn ochre lg full" type="submit">
          {{ mode === 'login' ? 'Войти' : 'Создать аккаунт' }}
          <span class="mono">↵</span>
        </button>

        <TodoBanner
          text="Реальный флоу: POST /api/passport/v1/auth/login или /register → сохранить access + refresh токены → редирект в кабинет."
        />
      </form>
    </div>
  </div>
</template>

<style scoped>
.auth {
  display: grid;
  grid-template-columns: 1fr 1fr;
  min-height: 100vh;
  width: 100%;
  max-width: 1200px;
  background: var(--paper);
  border: 1px solid var(--rule);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-lifted);
  overflow: hidden;
}
@media (max-width: 900px) {
  .auth {
    grid-template-columns: 1fr;
  }
  .auth__left {
    display: none;
  }
}

.auth__left {
  padding: 56px 48px;
  background: var(--ink);
  color: var(--paper);
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  gap: 32px;
  position: relative;
}
.auth__left::before {
  content: '';
  position: absolute;
  inset: 40px 40px auto auto;
  width: 100px;
  height: 100px;
  background:
    radial-gradient(circle at 25% 25%, var(--ochre) 0%, transparent 65%);
  opacity: 0.35;
}
.brand {
  font-family: var(--serif);
  font-size: 22px;
  line-height: 1.1;
  color: var(--paper);
}
.brand span:first-child {
  color: var(--ochre-deep);
  display: block;
}

.epigraph {
  font-family: var(--serif);
  font-size: 28px;
  line-height: 1.3;
  color: var(--paper);
  margin: 0;
  border-left: 3px solid var(--ochre);
  padding-left: 20px;
  font-weight: 300;
  font-variation-settings: 'opsz' 100, 'SOFT' 60;
}
.epigraph .mono {
  color: var(--ochre-deep);
  font-size: 12px;
  display: block;
  margin-bottom: 8px;
}
.epigraph__attr {
  color: var(--ochre-deep);
  font-size: 12px;
  margin-top: -20px;
}
.marks .muted {
  color: rgba(20, 23, 30, 0.55);
  font-size: 11px;
}

.auth__right {
  padding: 56px 48px;
  display: flex;
  flex-direction: column;
  gap: 24px;
  justify-content: center;
}
.tabs {
  display: inline-flex;
  border-bottom: 2px solid var(--ink);
  gap: 24px;
  padding-bottom: 2px;
}
.tab {
  border: 0;
  background: transparent;
  font-family: var(--serif);
  font-size: 22px;
  color: var(--ink-mute);
  cursor: pointer;
  padding: 6px 0;
  position: relative;
  font-weight: 500;
}
.tab.active {
  color: var(--ink);
}
.tab.active::after {
  content: '';
  position: absolute;
  left: 0;
  right: 0;
  bottom: -4px;
  height: 3px;
  background: var(--ochre);
}

.form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}
.grid-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}
.field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.field label {
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 0.14em;
  color: var(--ink-mute);
  font-weight: 600;
}
input {
  font-family: var(--sans);
  font-size: 15px;
  color: var(--ink);
  background: var(--paper);
  border: 1px solid var(--rule);
  border-radius: var(--radius-sm);
  padding: 12px 14px;
  outline: none;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}
input:focus {
  border-color: var(--ink);
  box-shadow: 0 0 0 3px rgba(55, 118, 126, 0.14);
}
.hint {
  color: var(--ink-mute);
  font-size: 11px;
  border: 0;
}
.hint:hover {
  color: var(--ochre);
}
.btn.full {
  width: 100%;
}
</style>
