<script setup lang="ts">
import { ref } from 'vue'
import { currentProfile } from '@/entities/user'
import TodoBanner from '@/shared/ui/TodoBanner.vue'

const reminderMinutes = ref(currentProfile.reminderIntervalMinutes)
const hideTranslations = ref(currentProfile.hideTranslations)

const presets = [30, 60, 90, 180, 360, 720, 1440]
</script>

<template>
  <section class="settings">
    <header>
      <span class="eyebrow">Section · Preferences</span>
      <h1 class="display">Настройки</h1>
    </header>

    <div class="grid">
      <article class="panel">
        <span class="eyebrow">Аккаунт</span>
        <dl class="dl">
          <div>
            <dt>Email</dt>
            <dd class="mono">{{ currentProfile.email }}</dd>
          </div>
          <div>
            <dt>Имя</dt>
            <dd>{{ currentProfile.firstName }} {{ currentProfile.lastName }}</dd>
          </div>
          <div>
            <dt>Роль</dt>
            <dd><span class="chip ink">{{ currentProfile.role }}</span></dd>
          </div>
          <div>
            <dt>Дата регистрации</dt>
            <dd class="mono">
              {{ new Date(currentProfile.createdAt).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short', year: 'numeric' }) }}
            </dd>
          </div>
        </dl>
        <div class="row-actions">
          <button class="btn ghost">Сменить пароль</button>
          <button class="btn ghost">Выйти</button>
        </div>
      </article>

      <article class="panel">
        <span class="eyebrow">Telegram</span>
        <dl class="dl">
          <div>
            <dt>ChatId</dt>
            <dd class="mono">{{ currentProfile.chatId }}</dd>
          </div>
          <div>
            <dt>Username</dt>
            <dd class="mono">@{{ currentProfile.telegramUsername }}</dd>
          </div>
          <div>
            <dt>Статус</dt>
            <dd><span class="chip ochre">заглушка · linked mock</span></dd>
          </div>
        </dl>
        <div class="row-actions">
          <router-link to="/link-telegram" class="btn ochre">Открыть модалку линковки</router-link>
        </div>
        <TodoBanner
          text="Реальной связки нет. Планируемый флоу: пользователь запускает /link {code} в боте → Passport записывает telegram_chat_id ↔ passport_user_id."
        />
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
              :class="{ active: reminderMinutes === p }"
              @click="reminderMinutes = p"
            >
              {{ p >= 60 ? `${p / 60}ч` : `${p}м` }}
            </button>
          </div>
          <span class="hint mono">
            текущий: {{ reminderMinutes }} мин · следующее в
            {{
              currentProfile.nextReminderAt
                ? new Date(currentProfile.nextReminderAt).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })
                : '—'
            }}
          </span>
        </div>
        <div class="field row">
          <label class="toggle">
            <input v-model="hideTranslations" type="checkbox" />
            <span>Скрывать перевод в списке карточек</span>
          </label>
        </div>
        <div class="row-actions">
          <button class="btn ochre">Сохранить</button>
        </div>
      </article>

      <article class="panel danger">
        <span class="eyebrow rust">Danger zone</span>
        <p class="serif descr">
          Действия, которые нельзя откатить. Экспортируйте колоду перед тем, как что-то удалять.
        </p>
        <div class="row-actions">
          <router-link to="/import" class="btn ghost">Экспорт колоды</router-link>
          <button class="btn rust">Удалить всю колоду</button>
        </div>
        <TodoBanner
          text="DELETE /api/cards/by-user/{userId} уже есть. Экспорт — endpoint отсутствует, реализовать отдельно."
        />
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
</style>
