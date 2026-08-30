<script setup lang="ts">
import { computed, ref, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { cardsApi, statsApi, userApi } from '@/shared/api'
import { getDueCards } from '@/entities/card'
import type { Card } from '@/entities/card'
import type { StatsToday } from '@/entities/stats'
import type { Profile } from '@/entities/user'
import { TodoBanner, AppBtn, AppChip } from '@/shared/ui'

const now = new Date('2026-08-11T15:22:00Z')

const allCards = ref<Card[]>([])
const statsToday = ref<StatsToday | null>(null)
const profile = ref<Profile | null>(null)
const hitmap = ref<number[][]>([])
const levelDistribution = ref<{ level: number; count: number }[]>([])

onMounted(async () => {
  const [cards, stats, prof, map, levels] = await Promise.all([
    cardsApi.getAll(),
    statsApi.getToday(),
    userApi.getProfile(),
    statsApi.getHitmap(),
    statsApi.getLevelDistribution(),
  ])
  allCards.value = cards
  statsToday.value = stats
  profile.value = prof
  hitmap.value = map
  levelDistribution.value = levels
})

const due = computed(() => getDueCards(allCards.value))
const dueSample = computed(() => due.value.slice(0, 4))

// Оценка длительности: 15 сек. на карточку — грубая эвристика для превью.
const trainEtaMin = computed(() => Math.max(1, Math.round((due.value.length * 15) / 60)))

const nextReminder = computed(() => {
  if (!profile.value?.nextReminderAt) return null
  const d = new Date(profile.value.nextReminderAt)
  const diffMin = Math.round((d.getTime() - now.getTime()) / 60000)
  return {
    time: d.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' }),
    inMin: diffMin,
  }
})

const streakState = computed(() => {
  // Мок: интервал напоминаний < 6ч и next в будущем — streak safe.
  if (!nextReminder.value) return { label: 'нет графика', klass: 'default' as const }
  if (nextReminder.value.inMin < 0)
    return { label: `просрочено на ${Math.abs(nextReminder.value.inMin)} мин`, klass: 'rust' as const }
  if (nextReminder.value.inMin < 240)
    return { label: `в безопасности до ${nextReminder.value.time}`, klass: 'sage' as const }
  return { label: 'без риска', klass: 'sage' as const }
})

const accuracyToday = computed(() =>
  (statsToday.value?.reviewsToday ?? 0) === 0
    ? 0
    : Math.round(((statsToday.value?.correctToday ?? 0) / (statsToday.value?.reviewsToday ?? 0)) * 100),
)

// Активность за 7 дней (последние 7 дней из hitmap)
const week = computed(() => {
  const map = hitmap.value
  const flat = map.flat()
  const last7 = flat.slice(-7)
  const maxV = Math.max(...last7, 1)
  const labels = ['ср', 'чт', 'пт', 'сб', 'вс', 'пн', 'вт']
  return last7.map((v, i) => ({
    day: labels[i],
    intensity: v,
    height: (v / maxV) * 100,
    reviews: v * 6 + 2, // мок значений повторений
    today: i === last7.length - 1,
  }))
})
const weekTotalReviews = computed(() =>
  week.value.reduce((a, d) => a + d.reviews, 0),
)

const recent = computed(() =>
  [...allCards.value].sort((a, b) => (a.createdAt < b.createdAt ? 1 : -1)).slice(0, 5),
)

// «Готовящиеся» напоминания (мок): сегодня следующие 3 напоминания
const upcomingReminders = computed(() => {
  const base = now.getTime()
  const interval = (profile.value?.reminderIntervalMinutes ?? 90) * 60 * 1000
  const start = new Date(profile.value?.nextReminderAt || now).getTime()
  return [0, 1, 2].map((i) => {
    const d = new Date(start + i * interval)
    const diffMin = Math.round((d.getTime() - base) / 60000)
    return {
      time: d.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' }),
      inLabel:
        diffMin < 0
          ? `${Math.abs(diffMin)} мин назад`
          : diffMin < 60
            ? `через ${diffMin} мин`
            : `через ${Math.round(diffMin / 60)}ч ${diffMin % 60 ? (diffMin % 60) + ' мин' : ''}`,
    }
  })
})

const pipeline = computed(() => {
  const max = Math.max(...levelDistribution.value.map((l) => l.count), 1)
  return levelDistribution.value.map((l) => ({
    ...l,
    pct: (l.count / max) * 100,
  }))
})
</script>

<template>
  <section class="dashboard">
    <!-- Cockpit strip: single-row summary + primary action -->
    <header class="cockpit rise rise-1">
      <div class="cockpit__lead">
        <span class="eyebrow">
          Кабинет · {{ new Date().toLocaleDateString('ru-RU', { weekday: 'long', day: 'numeric', month: 'long' }) }}
        </span>
        <h1 class="cockpit__headline">
          <span class="num">{{ due.length }}</span>
          <span class="serif tail">карточек ждут повтора</span>
        </h1>
        <div class="cockpit__facts">
          <span class="fact">
            <span class="fact__k">≈ {{ trainEtaMin }} мин</span>
            <span class="fact__l">сессия</span>
          </span>
          <span class="fact">
            <span class="fact__k">{{ accuracyToday }}%</span>
            <span class="fact__l">точность за сутки</span>
          </span>
          <span class="fact">
            <span class="fact__k">{{ statsToday?.streakDays ?? 0 }} дн</span>
            <span class="fact__l">streak</span>
            <AppChip :tone="streakState.klass">{{ streakState.label }}</AppChip>
          </span>
        </div>
      </div>
      <div class="cockpit__cta">
        <AppBtn variant="ochre" size="lg" to="/train">
          Начать сейчас
          <span class="mono">→</span>
        </AppBtn>
        <AppBtn variant="ghost" size="lg" to="/add">Новая карточка</AppBtn>
      </div>
    </header>

    <!-- Info grid -->
    <div class="grid-main rise rise-2">
      <!-- Ready to train -->
      <article class="panel span-2">
        <header class="panel__head">
          <h2>Готово к тренировке</h2>
          <span class="mono muted">{{ due.length }} к повтору · превью первых {{ dueSample.length }}</span>
        </header>
        <ul class="peek">
          <li v-for="c in dueSample" :key="c.id" class="peek__row">
            <span class="peek__num mono">{{ String(c.id).padStart(2, '0') }}</span>
            <div class="peek__body">
              <div class="peek__term">
                <span class="serif term">{{ c.term }}</span>
                <span class="mono trans">{{ c.transcription }}</span>
              </div>
              <span class="peek__hint mono">
                lvl {{ c.level }} · последний {{
                  c.lastReviewAt
                    ? new Date(c.lastReviewAt).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' })
                    : 'никогда'
                }}
              </span>
            </div>
            <span
              class="peek__due mono"
              :class="{ 'is-overdue': c.nextReviewAt && new Date(c.nextReviewAt) < now }"
            >
              {{
                c.nextReviewAt && new Date(c.nextReviewAt) < now
                  ? `просрочено ${Math.round((now.getTime() - new Date(c.nextReviewAt).getTime()) / 3600000)}ч`
                  : c.nextReviewAt
                    ? new Date(c.nextReviewAt).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' })
                    : 'новая'
              }}
            </span>
          </li>
        </ul>
        <footer class="panel__foot">
          <AppBtn variant="ochre" to="/train">Тренировать все {{ due.length }}</AppBtn>
          <RouterLink to="/deck?filter=due" class="link-more">развернуть список →</RouterLink>
        </footer>
      </article>

      <!-- Week activity -->
      <article class="panel">
        <header class="panel__head">
          <h2>Эта неделя</h2>
          <span class="mono muted">{{ weekTotalReviews }} повторений</span>
        </header>
        <div class="week">
          <div v-for="d in week" :key="d.day" class="week__col" :class="{ today: d.today }">
            <div class="week__bar-wrap">
              <div
                class="week__bar"
                :style="{ height: Math.max(4, d.height) + '%' }"
              />
            </div>
            <span class="week__day mono">{{ d.day }}</span>
            <span class="week__count mono">{{ d.reviews }}</span>
          </div>
        </div>
      </article>

      <!-- Level pipeline -->
      <article class="panel">
        <header class="panel__head">
          <h2>Pipeline</h2>
          <span class="mono muted">по уровням</span>
        </header>
        <ul class="pipeline">
          <li v-for="l in pipeline" :key="l.level" class="pipeline__row">
            <span class="pipeline__label mono">
              {{ l.level === 10 ? 'learned' : `lvl ${l.level}` }}
            </span>
            <div class="pipeline__bar" :class="{ learned: l.level === 10 }">
              <span :style="{ width: l.pct + '%' }" />
            </div>
            <span class="pipeline__count mono">{{ l.count }}</span>
          </li>
        </ul>
      </article>

      <!-- Recent additions -->
      <article class="panel">
        <header class="panel__head">
          <h2>Недавно</h2>
          <RouterLink to="/deck" class="link-more">все →</RouterLink>
        </header>
        <ul class="recent">
          <li v-for="c in recent" :key="c.id">
            <RouterLink :to="`/card/${c.id}`" class="recent__row">
              <span class="serif term">{{ c.term }}</span>
              <span class="serif tr">{{ c.translation }}</span>
              <span class="mono when">
                {{
                  new Date(c.createdAt).toLocaleDateString('ru-RU', {
                    day: 'numeric',
                    month: 'short',
                  })
                }}
              </span>
            </RouterLink>
          </li>
        </ul>
      </article>

      <!-- Reminders schedule -->
      <article class="panel">
        <header class="panel__head">
          <h2>Напоминания</h2>
          <router-link to="/settings" class="link-more">настройки →</router-link>
        </header>
        <ul class="reminders">
          <li v-for="(r, i) in upcomingReminders" :key="i" class="reminders__row" :class="{ active: i === 0 }">
            <span class="reminders__dot" />
            <span class="reminders__time mono">{{ r.time }}</span>
            <span class="reminders__hint">{{ r.inLabel }}</span>
          </li>
        </ul>
        <footer class="panel__foot subtle">
          <span class="mono muted">интервал {{ profile?.reminderIntervalMinutes ?? 90 }} мин</span>
        </footer>
      </article>

      <!-- TODO / system status -->
      <article class="panel span-2 todos">
        <header class="panel__head">
          <h2>Что не готово в API</h2>
          <span class="mono muted">заметки для интеграции</span>
        </header>
        <div class="todos__list">
          <TodoBanner
            label="Streak / история"
            text="Streak и недельная активность вычисляются по ReviewEntity — эндпоинта GET /api/cards/stats/history пока нет."
          />
        </div>
      </article>
    </div>
  </section>
</template>

<style scoped>
.dashboard {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

/* ---------- Cockpit ---------- */

.cockpit {
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 32px;
  align-items: center;
  padding: 22px 28px;
  background: linear-gradient(
      180deg,
      rgba(90, 160, 168, 0.05),
      rgba(90, 160, 168, 0) 60%
    ),
    var(--paper-elevated);
  border: 1px solid var(--rule);
  border-top: 3px solid var(--ochre);
  border-radius: var(--radius);
  position: relative;
}
.cockpit__lead {
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.cockpit__headline {
  font-family: var(--serif);
  display: flex;
  align-items: baseline;
  gap: 14px;
  margin: 0;
  line-height: 0.95;
}
.cockpit__headline .num {
  font-size: 68px;
  font-weight: 300;
  color: var(--ochre);
  letter-spacing: -0.04em;
  font-variation-settings: 'opsz' 144;
}
.cockpit__headline .tail {
  font-size: 24px;
  color: var(--ink);
}
.cockpit__facts {
  display: flex;
  gap: 24px;
  align-items: center;
  flex-wrap: wrap;
  margin-top: 4px;
}
.fact {
  display: flex;
  align-items: baseline;
  gap: 8px;
  font-size: 13px;
  color: var(--ink-soft);
}
.fact__k {
  font-family: var(--mono);
  color: var(--ink);
  font-size: 14px;
}
.fact__l {
  color: var(--ink-mute);
  font-size: 12px;
  letter-spacing: 0.02em;
}
.cockpit__cta {
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 220px;
}
@media (max-width: 900px) {
  .cockpit {
    grid-template-columns: 1fr;
  }
  .cockpit__cta {
    flex-direction: row;
  }
  .cockpit__headline .num {
    font-size: 52px;
  }
}

/* ---------- Grid ---------- */

.grid-main {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  grid-auto-rows: min-content;
  gap: 16px;
}
.span-2 {
  grid-column: span 2;
}
@media (max-width: 1100px) {
  .grid-main {
    grid-template-columns: repeat(2, 1fr);
  }
  .span-2 {
    grid-column: span 2;
  }
}
@media (max-width: 720px) {
  .grid-main {
    grid-template-columns: 1fr;
  }
  .span-2 {
    grid-column: span 1;
  }
}

.panel {
  background: var(--paper-elevated);
  border: 1px solid var(--rule);
  border-radius: var(--radius);
  padding: 18px 20px 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  min-height: 200px;
}
.panel__head {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  gap: 12px;
}
.panel__head h2 {
  margin: 0;
  font-size: 17px;
  font-weight: 500;
}
.muted {
  color: var(--ink-mute);
}
.link-more {
  border: 0;
  font-family: var(--mono);
  font-size: 11px;
  color: var(--ink-mute);
}
.link-more:hover {
  color: var(--ochre);
}

/* ---------- Peek (Ready-to-train) ---------- */

.peek {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
}
.peek__row {
  display: grid;
  grid-template-columns: 32px 1fr auto;
  gap: 14px;
  align-items: center;
  padding: 9px 0;
  border-bottom: 1px dashed var(--rule);
}
.peek__row:last-child {
  border-bottom: 0;
}
.peek__num {
  font-size: 11px;
  color: var(--ink-mute);
  text-align: center;
}
.peek__body {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}
.peek__term {
  display: flex;
  align-items: baseline;
  gap: 10px;
  min-width: 0;
}
.peek__term .term {
  font-family: var(--serif);
  font-size: 17px;
  font-weight: 500;
  color: var(--ink);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.peek__term .trans {
  color: var(--ink-mute);
  font-size: 11px;
  white-space: nowrap;
}
.peek__hint {
  color: var(--ink-mute);
  font-size: 11px;
}
.peek__due {
  font-size: 11px;
  color: var(--ink-soft);
  text-align: right;
}
.peek__due.is-overdue {
  color: var(--rust);
}

.panel__foot {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  padding-top: 10px;
  border-top: 1px solid var(--rule);
  margin-top: auto;
}
.panel__foot.subtle {
  border-top: 0;
  padding-top: 4px;
  justify-content: flex-end;
}

/* ---------- Week bars ---------- */

.week {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 8px;
  align-items: end;
  flex: 1;
  min-height: 140px;
}
.week__col {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
  height: 100%;
  justify-content: flex-end;
}
.week__bar-wrap {
  width: 100%;
  height: 100px;
  display: flex;
  align-items: flex-end;
  justify-content: center;
}
.week__bar {
  width: 100%;
  max-width: 22px;
  background: var(--ink-soft);
  border-radius: 2px 2px 0 0;
  transition: background 0.15s ease;
}
.week__col.today .week__bar {
  background: var(--ochre);
}
.week__col:hover .week__bar {
  background: var(--ochre);
}
.week__day {
  font-size: 10px;
  color: var(--ink-mute);
  text-transform: uppercase;
  letter-spacing: 0.04em;
}
.week__col.today .week__day {
  color: var(--ochre);
}
.week__count {
  font-size: 10px;
  color: var(--ink-mute);
}

/* ---------- Pipeline ---------- */

.pipeline {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.pipeline__row {
  display: grid;
  grid-template-columns: 52px 1fr 32px;
  align-items: center;
  gap: 8px;
}
.pipeline__label {
  font-size: 10.5px;
  color: var(--ink-mute);
}
.pipeline__bar {
  height: 10px;
  background: var(--paper-sunk);
  border-radius: 2px;
  overflow: hidden;
}
.pipeline__bar span {
  display: block;
  height: 100%;
  background: var(--ink-soft);
  transition: width 0.35s ease;
}
.pipeline__bar.learned span {
  background: var(--sage);
}
.pipeline__count {
  font-size: 11px;
  color: var(--ink);
  text-align: right;
}

/* ---------- Recent ---------- */

.recent {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
}
.recent__row {
  display: grid;
  grid-template-columns: minmax(80px, auto) 1fr auto;
  gap: 12px;
  align-items: baseline;
  padding: 6px 0;
  border-bottom: 1px dashed var(--rule);
  border-radius: 0;
  color: var(--ink);
  font-size: 13px;
}
.recent li:last-child .recent__row {
  border-bottom: 0;
}
.recent__row .term {
  font-family: var(--serif);
  font-size: 15px;
  font-weight: 500;
  white-space: nowrap;
}
.recent__row .tr {
  color: var(--ink-soft);
  font-size: 13px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.recent__row .when {
  color: var(--ink-mute);
  font-size: 11px;
}

/* ---------- Reminders ---------- */

.reminders {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.reminders__row {
  display: grid;
  grid-template-columns: 8px 60px 1fr;
  gap: 12px;
  align-items: center;
  font-size: 12.5px;
  color: var(--ink-soft);
  padding: 4px 0;
}
.reminders__dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--ink-mute);
  transition: background 0.15s ease;
}
.reminders__row.active .reminders__dot {
  background: var(--ochre);
  box-shadow: 0 0 0 3px rgba(90, 160, 168, 0.18);
}
.reminders__row.active .reminders__time,
.reminders__row.active .reminders__hint {
  color: var(--ink);
}
.reminders__time {
  font-size: 13px;
}

/* ---------- TODOs section ---------- */

.todos__list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
</style>
