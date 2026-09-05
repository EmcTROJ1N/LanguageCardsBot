<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { userApi, cardsApi } from '@/shared/api'
import type { Profile } from '@/entities/user'
import { PageHeader } from '@/shared/ui'
import { useAuthStore } from '@/features/auth'

const router = useRouter()
const authStore = useAuthStore()

const profile = ref<Profile | null>(null)
const reminderMinutes = ref(90)
const hideTranslations = ref(false)
const saving = ref(false)
const deleting = ref(false)
const telegramId = ref<number | null>(null)
const savingTelegram = ref(false)

onMounted(async () => {
  profile.value = await userApi.getProfile()
  reminderMinutes.value = profile.value.reminderIntervalMinutes
  hideTranslations.value = profile.value.hideTranslations
})

const presets = [0, 30, 60, 90, 180, 360, 720, 1440]

function presetLabel(minutes: number): string {
  if (minutes === 0) return 'Выкл'
  return minutes >= 60 ? `${minutes / 60}ч` : `${minutes}м`
}

async function save() {
  saving.value = true
  try {
    await userApi.updateProfile({
      reminderIntervalMinutes: reminderMinutes.value,
      hideTranslations: hideTranslations.value,
    })
  } finally {
    saving.value = false
  }
}

async function saveTelegramId() {
  if (!telegramId.value) return
  savingTelegram.value = true
  try {
    profile.value = await userApi.updateProfile({ chatId: telegramId.value })
    telegramId.value = null
  } finally {
    savingTelegram.value = false
  }
}

function logout() {
  authStore.logout()
  router.push('/login')
}

async function deleteAllCards() {
  const cardsUserId = profile.value?.cardsUserId
  if (!cardsUserId) return
  if (!window.confirm('Удалить все карточки? Это действие нельзя откатить.')) return
  deleting.value = true
  try {
    await cardsApi.deleteAllByUser(cardsUserId)
    router.push('/deck')
  } finally {
    deleting.value = false
  }
}
</script>

<template>
  <section class="settings">
    <PageHeader eyebrow="Chapter · Apparatus" title="Настройки" />

    <div class="grid">
      <article class="panel">
        <span class="eyebrow">Аккаунт</span>
        <dl class="dl">
          <div>
            <dt>Email</dt>
            <dd class="mono">{{ profile?.email }}</dd>
          </div>
          <div>
            <dt>Имя</dt>
            <dd>{{ profile?.firstName }} {{ profile?.lastName }}</dd>
          </div>
          <div>
            <dt>Роль</dt>
            <dd><span class="chip ink">{{ profile?.role }}</span></dd>
          </div>
          <div>
            <dt>Дата регистрации</dt>
            <dd class="mono">
              {{ profile?.createdAt ? new Date(profile.createdAt).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short', year: 'numeric' }) : '—' }}
            </dd>
          </div>
        </dl>
        <div class="row-actions">
          <button class="btn ghost" disabled>Сменить пароль</button>
          <button class="btn ghost" @click="logout">Выйти</button>
        </div>
      </article>

      <article class="panel">
        <span class="eyebrow">Telegram</span>

        <template v-if="profile?.chatId">
          <dl class="dl">
            <div>
              <dt>Telegram ID</dt>
              <dd class="mono">{{ profile.chatId }}</dd>
            </div>
          </dl>
          <span class="chip sage">✓ привязан</span>
        </template>

        <template v-else>
          <p class="serif descr">
            Введите ваш Telegram ID, чтобы использовать бота. Бот сообщит ID при первом обращении к нему.
          </p>
          <div class="field">
            <label>Telegram ID</label>
            <input
              v-model.number="telegramId"
              type="number"
              class="tg-input"
              placeholder="123456789"
              min="1"
            />
          </div>
          <div class="row-actions">
            <button
              class="btn ochre"
              :disabled="!telegramId || savingTelegram"
              @click="saveTelegramId"
            >
              {{ savingTelegram ? 'Сохраняется…' : 'Сохранить' }}
            </button>
          </div>
        </template>
      </article>

      <article class="panel">
        <span class="eyebrow">Напоминания</span>
        <p class="serif descr">
          Бот присылает напоминания в Telegram, когда карточки готовы к повторению.
        </p>
        <div class="field">
          <label>Интервал</label>
          <div class="presets">
            <button
              v-for="p in presets"
              :key="p"
              class="preset"
              :class="{ active: reminderMinutes === p, off: p === 0 }"
              @click="reminderMinutes = p"
            >
              {{ presetLabel(p) }}
            </button>
          </div>
          <div class="custom-row">
            <label class="custom-label">или свой:</label>
            <input
              v-model.number="reminderMinutes"
              type="number"
              class="custom-input"
              min="1"
              step="1"
            />
            <span class="custom-unit">мин</span>
          </div>
          <span class="hint mono">
            <template v-if="reminderMinutes === 0">напоминания выключены</template>
            <template v-else>
              текущий: {{ reminderMinutes }} мин · следующее в
              {{
                profile?.nextReminderAt
                  ? new Date(profile.nextReminderAt).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })
                  : '—'
              }}
            </template>
          </span>
        </div>
        <div class="field row">
          <label class="toggle">
            <input v-model="hideTranslations" type="checkbox" />
            <span>Скрывать перевод в списке карточек</span>
          </label>
        </div>
        <div class="row-actions">
          <button class="btn ochre" :disabled="saving" @click="save">
            {{ saving ? 'Сохраняется…' : 'Сохранить' }}
          </button>
        </div>
      </article>

      <article class="panel danger">
        <span class="eyebrow rust">Danger zone</span>
        <p class="serif descr">
          Действия, которые нельзя откатить. Экспортируйте колоду перед тем, как что-то удалять.
        </p>
        <div class="row-actions">
          <button class="btn ghost" disabled>Экспорт колоды</button>
          <button class="btn rust" :disabled="deleting || !profile?.cardsUserId" @click="deleteAllCards">
            {{ deleting ? 'Удаляется…' : 'Удалить всю колоду' }}
          </button>
        </div>
      </article>
    </div>
  </section>
</template>

<style scoped>
.settings {
  display: flex;
  flex-direction: column;
  gap: 24px;
}
.grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
}
@media (max-width: 900px) {
  .grid {
    grid-template-columns: 1fr;
  }
}
.panel {
  background: var(--paper-elevated);
  border: 1px solid var(--rule);
  border-radius: var(--radius);
  padding: 22px 24px;
  display: flex;
  flex-direction: column;
  gap: 14px;
}
.panel.danger {
  border-color: rgba(164, 67, 47, 0.35);
  background: rgba(164, 67, 47, 0.04);
}
.eyebrow.rust {
  color: var(--rust);
}
.descr {
  color: var(--ink-soft);
  font-size: 15px;
  margin: 0;
}
.dl {
  display: grid;
  grid-template-columns: auto 1fr;
  gap: 4px 16px;
  margin: 0;
}
.dl div {
  display: contents;
}
.dl dt {
  font-size: 11px;
  color: var(--ink-mute);
  padding: 6px 0;
  border-bottom: 1px dashed var(--rule);
  text-transform: uppercase;
  letter-spacing: 0.14em;
  font-weight: 600;
}
.dl dd {
  margin: 0;
  padding: 6px 0;
  border-bottom: 1px dashed var(--rule);
  font-size: 14px;
  color: var(--ink);
  text-align: right;
}
.row-actions {
  display: flex;
  gap: 10px;
  padding-top: 4px;
  border-top: 1px solid var(--rule);
  padding-top: 12px;
  margin-top: auto;
}
.field {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.field.row {
  gap: 4px;
}
.field label {
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 0.14em;
  color: var(--ink-mute);
  font-weight: 600;
}
.hint {
  color: var(--ink-mute);
  font-size: 11px;
}
.presets {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}
.preset {
  padding: 6px 12px;
  background: var(--paper);
  border: 1px solid var(--rule);
  border-radius: var(--radius-sm);
  font-family: var(--sans);
  font-size: 13px;
  color: var(--ink-soft);
  cursor: pointer;
  transition: background 0.12s ease, color 0.12s ease, border-color 0.12s ease;
}
.preset:hover {
  border-color: var(--ink);
  color: var(--ink);
}
.preset.active {
  background: var(--ink);
  color: var(--paper);
  border-color: var(--ink);
}
.toggle {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  text-transform: none;
  letter-spacing: 0;
  font-weight: 400;
  color: var(--ink);
  font-size: 14px;
  cursor: pointer;
}
.toggle input {
  accent-color: var(--ochre);
  width: 16px;
  height: 16px;
}
.tg-input {
  width: 100%;
  padding: 8px 12px;
  background: var(--paper);
  border: 1px solid var(--rule);
  border-radius: var(--radius-sm);
  font-family: var(--mono);
  font-size: 14px;
  color: var(--ink);
  box-sizing: border-box;
}
.tg-input:focus {
  outline: none;
  border-color: var(--ochre);
}
.custom-row {
  display: flex;
  align-items: center;
  gap: 8px;
  text-transform: none;
  letter-spacing: 0;
}
.custom-label {
  font-size: 12px;
  color: var(--ink-mute);
  text-transform: none;
  letter-spacing: 0;
  font-weight: 400;
}
.custom-input {
  width: 80px;
  padding: 6px 10px;
  background: var(--paper);
  border: 1px solid var(--rule);
  border-radius: var(--radius-sm);
  font-family: var(--mono);
  font-size: 13px;
  color: var(--ink);
  box-sizing: border-box;
}
.custom-input:focus {
  outline: none;
  border-color: var(--ochre);
}
.custom-unit {
  font-family: var(--mono);
  font-size: 12px;
  color: var(--ink-mute);
}
</style>
