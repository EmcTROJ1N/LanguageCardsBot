<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import TodoBanner from '@/shared/ui/TodoBanner.vue'

const router = useRouter()
const code = ref('AURORA · 4128 · MURMUR')
const step = ref<1 | 2 | 3>(1)

function goToBot() {
  step.value = 2
}

function confirm() {
  step.value = 3
  setTimeout(() => router.push('/'), 800)
}

const stepLabel = computed(() => `Шаг ${step.value} из 3`)
</script>

<template>
  <div class="link">
    <div class="link__card">
      <header class="link__head">
        <span class="mono">{{ stepLabel }}</span>
        <button class="btn ghost sm" @click="router.back()">← назад</button>
      </header>

      <h1 class="display">
        Связать <em class="serif">Telegram-бот</em>
      </h1>
      <p class="lede serif">
        Веб-кабинет знает вашу учётную запись Passport (email + пароль), а бот —
        Telegram chat_id. Свяжем их одноразовым кодом.
      </p>

      <div v-if="step === 1" class="step">
        <span class="eyebrow">One-time code</span>
        <div class="code mono">{{ code }}</div>
        <ol class="how">
          <li>Откройте бот <span class="mono">@languagecards_bot</span>.</li>
          <li>Отправьте команду <span class="mono">/link AURORA-4128-MURMUR</span>.</li>
          <li>Бот подтвердит связку, а страница переключится сама.</li>
        </ol>
        <div class="row-actions">
          <a class="btn ochre" href="#" @click.prevent="goToBot">Я скопировал код</a>
          <button class="btn ghost">Обновить код</button>
        </div>
      </div>

      <div v-else-if="step === 2" class="step">
        <span class="eyebrow">Ожидаем подтверждения</span>
        <div class="waiting">
          <span class="serif waiting__text">
            Бот получит команду и отправит подтверждение через WebSocket…
          </span>
          <div class="waiting__pulse" />
        </div>
        <div class="row-actions">
          <button class="btn ochre" @click="confirm">Симулировать успех</button>
          <button class="btn ghost" @click="step = 1">Показать код снова</button>
        </div>
      </div>

      <div v-else class="step done">
        <span class="chip sage">✓ связано</span>
        <h2 class="display done__title">
          Готово. Возвращаемся в кабинет…
        </h2>
      </div>

      <TodoBanner
        label="Целиком заглушка"
        text="Нет ни таблицы связок, ни команды /link в боте, ни WebSocket-канала. Реализация: (1) таблица в Passport или отдельном сервисе passport_user_id ↔ cards_user_id ↔ telegram_chat_id; (2) команда /link {code} в LanguageCardsBot; (3) SignalR/WebSocket или short-polling GET /link/status."
      />
    </div>
  </div>
</template>

<style scoped>
.link {
  width: 100%;
  max-width: 720px;
  margin: 0 auto;
}
.link__card {
  background: var(--paper);
  border: 1px solid var(--ink);
  border-radius: var(--radius-lg);
  padding: 40px;
  display: flex;
  flex-direction: column;
  gap: 20px;
  box-shadow: var(--shadow-lifted);
}
.link__head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 12px;
  color: var(--ink-mute);
}
.display em {
  color: var(--ochre);
}
.lede {
  color: var(--ink-soft);
  font-size: 17px;
}

.step {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding-top: 12px;
  border-top: 1px solid var(--rule);
}
.code {
  font-size: 32px;
  padding: 20px 24px;
  background: var(--paper-sunk);
  color: var(--ochre);
  border: 1px solid var(--rule);
  border-radius: var(--radius);
  letter-spacing: 0.04em;
  text-align: center;
}
.how {
  margin: 0;
  padding-left: 20px;
  font-family: var(--serif);
  color: var(--ink-soft);
  font-size: 16px;
  line-height: 1.55;
}
.how .mono {
  font-style: normal;
  color: var(--ink);
  background: var(--paper-elevated);
  padding: 1px 6px;
  border-radius: 3px;
}
.row-actions {
  display: flex;
  gap: 10px;
  padding-top: 6px;
}

.waiting {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px;
  background: var(--paper-elevated);
  border: 1px dashed var(--rule);
  border-radius: var(--radius);
}
.waiting__text {
  color: var(--ink);
  font-size: 16px;
}
.waiting__pulse {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: var(--ochre);
  box-shadow: 0 0 0 0 rgba(55, 118, 126, 0.5);
  animation: pulse 1.4s infinite;
  flex-shrink: 0;
  margin-left: auto;
}
@keyframes pulse {
  0% {
    box-shadow: 0 0 0 0 rgba(55, 118, 126, 0.5);
  }
  70% {
    box-shadow: 0 0 0 14px rgba(55, 118, 126, 0);
  }
  100% {
    box-shadow: 0 0 0 0 rgba(55, 118, 126, 0);
  }
}

.step.done {
  align-items: flex-start;
}
.done__title {
  font-size: clamp(36px, 4vw, 48px);
}
</style>
