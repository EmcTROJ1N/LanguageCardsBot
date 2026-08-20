<script setup lang="ts">
import { RouterLink, RouterView, useRoute, useRouter } from 'vue-router'
import { computed, onMounted, ref } from 'vue'
import { statsApi } from '@/shared/api'
import { useAuthStore } from '@/features/auth'
import type { StatsToday } from '@/entities/stats'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const statsToday = ref<StatsToday | null>(null)

onMounted(async () => {
  statsToday.value = await statsApi.getToday()
})

const nav = [
  { to: '/', label: 'Кабинет', section: 'ежедневно' },
  { to: '/train', label: 'Тренировка', section: 'ежедневно' },
  { to: '/deck', label: 'Колода', section: 'ежедневно' },
  { to: '/add', label: 'Новая карточка', section: 'коллекция' },
  { to: '/import', label: 'Импорт / Экспорт', section: 'коллекция' },
  { to: '/stats', label: 'Статистика', section: 'коллекция' },
  { to: '/settings', label: 'Настройки', section: 'аккаунт' },
]

const sections = computed(() => {
  const grouped = new Map<string, typeof nav>()
  for (const item of nav) {
    if (!grouped.has(item.section)) grouped.set(item.section, [] as typeof nav)
    grouped.get(item.section)!.push(item)
  }
  return Array.from(grouped.entries()).map(([section, items]) => ({ section, items }))
})

const showChrome = computed(() => route.path !== '/link-telegram')

async function logout() {
  authStore.logout()
  await router.push('/login')
}
</script>

<template>
  <div v-if="!showChrome" class="bare">
    <RouterView />
  </div>

  <div v-else class="shell">
    <aside class="rail rise rise-1">
      <div class="brand">
        <div class="brand__mark">
          <span class="serif">Lc</span>
        </div>
        <div class="brand__title">
          <span class="serif">Language</span>
          <br />
          <span>Cards Cabinet</span>
        </div>
      </div>

      <nav class="nav">
        <section v-for="s in sections" :key="s.section" class="nav__section">
          <span class="nav__section-label">{{ s.section }}</span>
          <ul>
            <li v-for="item in s.items" :key="item.to">
              <RouterLink :to="item.to" :class="{ active: route.path === item.to }">
                <span class="dot" aria-hidden="true" />
                {{ item.label }}
              </RouterLink>
            </li>
          </ul>
        </section>
      </nav>

      <footer class="rail__foot">
        <div class="user">
          <div class="user__avatar">
            {{ authStore.user?.firstName?.[0] }}{{ authStore.user?.lastName?.[0] }}
          </div>
          <div class="user__meta">
            <span class="user__name">{{ authStore.user?.firstName }} {{ authStore.user?.lastName }}</span>
            <span class="user__hint mono">@{{ authStore.user?.telegramUsername }}</span>
          </div>
        </div>
        <button class="logout-btn mono" type="button" @click="logout">Выйти</button>
      </footer>
    </aside>

    <main class="main">
      <div class="top-band rise rise-2">
        <div class="masthead">
          <span class="eyebrow">Sub №{{ statsToday?.streakDays ?? '—' }} ·
            Volume 1 · Weekly Edition</span>
          <span class="masthead__date">
            {{
              new Date().toLocaleDateString('ru-RU', {
                weekday: 'long',
                day: 'numeric',
                month: 'long',
                year: 'numeric',
              })
            }}
          </span>
        </div>
        <div class="top-band__right">
          <span class="chip">
            <span class="dot dot--sage" /> API online
          </span>
          <span class="chip ochre">
            {{ statsToday?.due ?? '—' }} к повтору
          </span>
        </div>
      </div>

      <div class="page rise rise-3">
        <RouterView />
      </div>

      <div class="footer">
        <span class="mono">© 2026 · Cabinet edition · v0.1.0-mockup</span>
      </div>
    </main>
  </div>
</template>

<style scoped>
.shell {
  display: grid;
  grid-template-columns: var(--rail-w) 1fr;
  min-height: 100vh;
}
.bare {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
}

/* ---- Rail ---- */
.rail {
  padding: 32px 24px;
  border-right: 1px solid var(--rule);
  background: var(--paper);
  display: flex;
  flex-direction: column;
  gap: 32px;
  position: sticky;
  top: 0;
  height: 100vh;
}
.brand {
  display: flex;
  align-items: center;
  gap: 14px;
}
.brand__mark {
  width: 42px;
  height: 42px;
  border: 1px solid var(--ink);
  border-radius: var(--radius-sm);
  display: flex;
  align-items: center;
  justify-content: center;
  font-family: var(--serif);
  font-size: 22px;
  background: var(--ink);
  color: var(--paper);
  letter-spacing: -0.03em;
}
.brand__title {
  font-family: var(--serif);
  line-height: 1.05;
  font-size: 16px;
  font-weight: 500;
  color: var(--ink);
}
.brand__title span:first-child {
  color: var(--ochre);
}

.nav {
  display: flex;
  flex-direction: column;
  gap: 22px;
  flex: 1;
}
.nav__section {
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.nav__section-label {
  text-transform: uppercase;
  font-size: 10px;
  letter-spacing: 0.16em;
  color: var(--ink-mute);
  font-weight: 600;
  padding: 0 6px;
}
.nav__section ul {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 2px;
}
.nav__section a {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 7px 8px;
  color: var(--ink-soft);
  border: 0;
  border-radius: var(--radius-sm);
  font-size: 14.5px;
  transition: color 0.12s ease, background 0.12s ease;
}
.nav__section a .dot {
  width: 5px;
  height: 5px;
  border-radius: 50%;
  background: transparent;
  border: 1px solid var(--ink-mute);
  transition: background 0.12s ease, border-color 0.12s ease;
}
.nav__section a:hover {
  color: var(--ink);
  background: var(--paper-elevated);
}
.nav__section a.active {
  color: var(--ink);
  font-weight: 600;
  background: var(--paper-elevated);
}
.nav__section a.active .dot {
  background: var(--ochre);
  border-color: var(--ochre);
}

.rail__foot {
  padding-top: 20px;
  border-top: 1px solid var(--rule);
}
.user {
  display: flex;
  align-items: center;
  gap: 12px;
}
.user__avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--ochre);
  color: var(--paper);
  display: flex;
  align-items: center;
  justify-content: center;
  font-family: var(--serif);
  font-size: 14px;
  font-weight: 500;
}
.user__meta {
  display: flex;
  flex-direction: column;
  line-height: 1.2;
}
.user__name {
  font-size: 13px;
  font-weight: 600;
  color: var(--ink);
}
.user__hint {
  font-size: 11px;
  color: var(--ink-mute);
}

/* ---- Main ---- */
.main {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}
.top-band {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 22px 48px;
  border-bottom: 1px solid var(--rule);
  background: var(--paper);
  position: sticky;
  top: 0;
  z-index: 10;
  gap: 24px;
}
.masthead {
  display: flex;
  flex-direction: column;
  line-height: 1.2;
}
.masthead__date {
  font-family: var(--serif);
  font-size: 15px;
  color: var(--ink-soft);
  margin-top: 3px;
}
.top-band__right {
  display: flex;
  gap: 12px;
  align-items: center;
}
.top-band__right .dot--sage {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: var(--sage);
  display: inline-block;
}

.page {
  padding: 40px 48px 64px;
  flex: 1;
  max-width: var(--page-max);
  width: 100%;
}

.footer {
  padding: 20px 48px;
  border-top: 1px solid var(--rule);
  color: var(--ink-mute);
  font-size: 12px;
}

.logout-btn {
  margin-top: 12px;
  border: 0;
  background: transparent;
  color: var(--ink-mute);
  font-size: 11px;
  cursor: pointer;
  padding: 0;
  text-align: left;
}
.logout-btn:hover { color: var(--ink); }
</style>
