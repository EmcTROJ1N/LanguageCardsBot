<script setup lang="ts">
import { useRouter } from 'vue-router'
import { LinkTelegramSteps } from '@/features/link-telegram'

const router = useRouter()
</script>

<template>
  <div class="link">
    <div class="link__card">
      <header class="link__head">
        <span />
        <button class="btn ghost sm" @click="router.back()">← назад</button>
      </header>
      <h1 class="display">
        Связать <em class="serif">Telegram-бот</em>
      </h1>
      <p class="lede serif">
        Веб-кабинет знает вашу учётную запись Passport (email + пароль), а бот —
        Telegram chat_id. Свяжем их одноразовым кодом.
      </p>
      <LinkTelegramSteps />
    </div>
  </div>
</template>

<style scoped>
.link { width: 100%; max-width: 720px; margin: 0 auto; }
.link__card { background: var(--paper); border: 1px solid var(--ink); border-radius: var(--radius-lg); padding: 40px; display: flex; flex-direction: column; gap: 20px; box-shadow: var(--shadow-lifted); }
.link__head { display: flex; justify-content: space-between; align-items: center; font-size: 12px; color: var(--ink-mute); }
.display em { color: var(--ochre); }
.lede { color: var(--ink-soft); font-size: 17px; }
</style>
