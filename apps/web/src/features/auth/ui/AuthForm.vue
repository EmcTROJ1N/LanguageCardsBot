<script setup lang="ts">
import { useAuth } from '../model/useAuth'
import { AppField, AppBtn, TodoBanner } from '@/shared/ui'

const { mode, email, password, firstName, lastName, submit } = useAuth()
</script>

<template>
  <div class="auth__right">
    <div class="tabs">
      <button class="tab" :class="{ active: mode === 'login' }" @click="mode = 'login'">Вход</button>
      <button class="tab" :class="{ active: mode === 'register' }" @click="mode = 'register'">Регистрация</button>
    </div>

    <form class="form" @submit.prevent="submit">
      <div v-if="mode === 'register'" class="grid-2">
        <AppField label="Имя">
          <input v-model="firstName" type="text" placeholder="Имя" />
        </AppField>
        <AppField label="Фамилия">
          <input v-model="lastName" type="text" placeholder="Фамилия" />
        </AppField>
      </div>

      <AppField label="Email">
        <input v-model="email" type="email" placeholder="you@example.com" />
      </AppField>

      <AppField label="Пароль" :hint="mode === 'register' ? 'не короче 8 символов' : undefined">
        <input v-model="password" type="password" placeholder="••••••••" />
        <template v-if="mode === 'login'">
          <a class="hint" href="#">Забыли пароль?</a>
        </template>
      </AppField>

      <AppBtn variant="ochre" size="lg" type="submit" class="full">
        {{ mode === 'login' ? 'Войти' : 'Создать аккаунт' }}
        <span class="mono">↵</span>
      </AppBtn>

      <TodoBanner
        text="Реальный флоу: POST /api/passport/v1/auth/login или /register → сохранить access + refresh токены → редирект в кабинет."
      />
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
:deep(.btn.full) { width: 100%; }
</style>
