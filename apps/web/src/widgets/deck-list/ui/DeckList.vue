<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { cards, type Card, statusOf, accuracyOf, nextReviewTimestamp, statusRank } from '@/entities/card'
import { CardRow } from '@/entities/card'
import TodoBanner from '@/shared/ui/TodoBanner.vue'

const route = useRoute()
const router = useRouter()

type Filter = 'all' | 'due' | 'learned' | 'new'
type SortKey = 'status' | 'term' | 'translation' | 'level' | 'accuracy' | 'next'
type SortDir = 'asc' | 'desc'
type Group = 'none' | 'status' | 'level' | 'letter'

const filter = ref<Filter>((route.query.filter as Filter) || 'all')
const sortKey = ref<SortKey>('term')
const sortDir = ref<SortDir>('asc')
const group = ref<Group>('none')
const query = ref('')

function setFilter(f: Filter) {
  filter.value = f
  router.replace({ query: { ...route.query, filter: f } })
}

function toggleSort(key: SortKey) {
  if (sortKey.value === key) {
    sortDir.value = sortDir.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortKey.value = key
    // sensible default direction per column
    sortDir.value = key === 'accuracy' || key === 'level' ? 'desc' : 'asc'
  }
}

function compare(a: Card, b: Card): number {
  const dir = sortDir.value === 'asc' ? 1 : -1
  let cmp = 0
  switch (sortKey.value) {
    case 'status':
      cmp = statusRank(a) - statusRank(b)
      break
    case 'term':
      cmp = a.term.localeCompare(b.term)
      break
    case 'translation':
      cmp = a.translation.localeCompare(b.translation, 'ru')
      break
    case 'level':
      cmp = a.level - b.level
      break
    case 'accuracy':
      cmp = accuracyOf(a) - accuracyOf(b)
      break
    case 'next':
      cmp = nextReviewTimestamp(a) - nextReviewTimestamp(b)
      break
  }
  if (cmp === 0) cmp = a.term.localeCompare(b.term)
  return cmp * dir
}

const filtered = computed(() => {
  let result = cards.slice()
  if (filter.value === 'due') {
    result = result.filter((c) => statusOf(c) === 'due' || statusOf(c) === 'new')
  } else if (filter.value === 'learned') {
    result = result.filter((c) => statusOf(c) === 'learned')
  } else if (filter.value === 'new') {
    result = result.filter((c) => statusOf(c) === 'new')
  }
  if (query.value.trim()) {
    const q = query.value.trim().toLowerCase()
    result = result.filter(
      (c) =>
        c.term.toLowerCase().includes(q) ||
        c.translation.toLowerCase().includes(q),
    )
  }
  result.sort(compare)
  return result
})

// Grouping: emit rows as { type: 'group', ... } | { type: 'card', card }
type Row =
  | { type: 'group'; key: string; label: string; count: number }
  | { type: 'card'; card: Card }

const rows = computed<Row[]>(() => {
  if (group.value === 'none') {
    return filtered.value.map((c) => ({ type: 'card', card: c }) as Row)
  }
  const keyOf = (c: Card): { key: string; label: string } => {
    if (group.value === 'status') {
      const s = statusOf(c)
      const labels: Record<string, string> = {
        due: 'К повтору',
        new: 'Новые',
        queued: 'В очереди',
        learned: 'Выучено',
      }
      return { key: s, label: labels[s] ?? s }
    }
    if (group.value === 'level') {
      if (c.learned) return { key: 'learned', label: 'Выучено (level 10)' }
      return { key: `lvl-${c.level}`, label: `Level ${c.level}` }
    }
    if (group.value === 'letter') {
      const first = c.term.charAt(0).toUpperCase()
      return { key: first, label: first }
    }
    return { key: '', label: '' }
  }

  // stable order: honor current sort inside each group, group order by min index or alpha for letter
  const buckets = new Map<string, { label: string; items: Card[] }>()
  for (const c of filtered.value) {
    const { key, label } = keyOf(c)
    if (!buckets.has(key)) buckets.set(key, { label, items: [] })
    buckets.get(key)!.items.push(c)
  }

  // Group-order sort
  const groupOrder = (a: [string, { label: string; items: Card[] }], b: [string, { label: string; items: Card[] }]) => {
    if (group.value === 'status') {
      const rank: Record<string, number> = { due: 0, new: 1, queued: 2, learned: 3 }
      return (rank[a[0]] ?? 99) - (rank[b[0]] ?? 99)
    }
    if (group.value === 'level') {
      const av = a[0] === 'learned' ? 100 : parseInt(a[0].replace('lvl-', ''), 10)
      const bv = b[0] === 'learned' ? 100 : parseInt(b[0].replace('lvl-', ''), 10)
      return av - bv
    }
    if (group.value === 'letter') {
      return a[0].localeCompare(b[0])
    }
    return 0
  }

  const result: Row[] = []
  for (const [key, bucket] of Array.from(buckets.entries()).sort(groupOrder)) {
    result.push({
      type: 'group',
      key,
      label: bucket.label,
      count: bucket.items.length,
    })
    for (const card of bucket.items) result.push({ type: 'card', card })
  }
  return result
})

const counts = computed(() => ({
  all: cards.length,
  due: cards.filter((c) => statusOf(c) === 'due' || statusOf(c) === 'new').length,
  learned: cards.filter((c) => statusOf(c) === 'learned').length,
  new: cards.filter((c) => statusOf(c) === 'new').length,
}))

const columns: {
  key: SortKey
  label: string
  align?: 'right'
  className: string
  iconOnly?: boolean
}[] = [
  { key: 'status', label: 'статус', className: 'col-status-h', iconOnly: true },
  { key: 'term', label: 'слово', className: 'col-term-h' },
  { key: 'translation', label: 'перевод', className: 'col-translation-h' },
  { key: 'level', label: 'level', className: 'col-level-h' },
  { key: 'accuracy', label: 'точн.', align: 'right', className: 'col-acc-h' },
  { key: 'next', label: 'следующий', align: 'right', className: 'col-next-h' },
]
</script>

<template>
  <section class="deck">
    <header class="head">
      <div class="head__title">
        <span class="eyebrow">Vol. 1 · Colophon</span>
        <h1 class="display">Колода</h1>
      </div>
      <div class="head__actions">
        <router-link to="/add" class="btn ochre">Новая карточка</router-link>
      </div>
    </header>

    <div class="toolbar">
      <div class="filters">
        <button
          v-for="f in (['all', 'due', 'learned', 'new'] as const)"
          :key="f"
          class="filter"
          :class="{ active: filter === f }"
          @click="setFilter(f)"
        >
          <span class="filter__label">
            {{
              f === 'all'
                ? 'все'
                : f === 'due'
                  ? 'к повтору'
                  : f === 'learned'
                    ? 'выучено'
                    : 'новые'
            }}
          </span>
          <span class="filter__count mono">{{ counts[f] }}</span>
        </button>
      </div>
      <div class="right">
        <label class="ctrl">
          <span class="mono ctrl__label">группировка</span>
          <select v-model="group">
            <option value="none">без группировки</option>
            <option value="status">по статусу</option>
            <option value="level">по уровню</option>
            <option value="letter">по первой букве</option>
          </select>
        </label>
        <div class="search">
          <input v-model="query" type="text" placeholder="поиск: слово или перевод…" />
        </div>
      </div>
    </div>

    <TodoBanner
      text="Клиентский поиск / сортировка / группировка. При 200+ карточках — серверный GET /api/cards/search?q=&sort=&filter=&group=."
    />

    <div class="table">
      <div class="table__header">
        <button
          v-for="col in columns"
          :key="col.key"
          class="th"
          :class="[col.className, { active: sortKey === col.key, 'align-right': col.align === 'right', 'th--icon-only': col.iconOnly }]"
          :title="`сортировка по «${col.label}»`"
          :aria-label="`сортировка по «${col.label}»`"
          @click="toggleSort(col.key)"
        >
          <span v-if="!col.iconOnly" class="th__label">{{ col.label }}</span>
          <span class="th__arrow mono">
            <template v-if="sortKey === col.key">{{ sortDir === 'asc' ? '↑' : '↓' }}</template>
            <template v-else>·</template>
          </span>
        </button>
      </div>

      <div v-if="filtered.length === 0" class="empty">
        <p class="serif">Ничего не найдено. Попробуйте другой фильтр или запрос.</p>
      </div>
      <div v-else class="table__body">
        <template v-for="r in rows" :key="r.type === 'group' ? `g-${r.key}` : `c-${r.card.id}`">
          <div v-if="r.type === 'group'" class="group-header">
            <span class="group-header__line" />
            <span class="group-header__label">
              {{ r.label }}
              <span class="mono group-header__count">· {{ r.count }}</span>
            </span>
            <span class="group-header__line" />
          </div>
          <CardRow v-else-if="r.type === 'card'" :card="r.card" />
        </template>
      </div>

      <div class="table__footer mono">
        показано {{ filtered.length }} из {{ counts.all }} · сортировка:
        <span class="table__footer-hi">{{ columns.find((c) => c.key === sortKey)?.label }} {{ sortDir === 'asc' ? '↑' : '↓' }}</span>
        <template v-if="group !== 'none'"> · группировка: <span class="table__footer-hi">{{ group === 'status' ? 'статус' : group === 'level' ? 'уровень' : 'первая буква' }}</span></template>
      </div>
    </div>
  </section>
</template>

<style scoped>
.deck {
  display: flex;
  flex-direction: column;
  gap: 16px;
}
.head {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: 16px;
}
.head__title h1 {
  margin: 0;
  font-size: 40px;
  line-height: 1;
}

.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}
.filters {
  display: inline-flex;
  border: 1px solid var(--rule);
  border-radius: var(--radius);
  overflow: hidden;
  background: var(--paper-elevated);
}
.filter {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 7px 12px;
  background: transparent;
  border: 0;
  border-right: 1px solid var(--rule);
  font-family: var(--sans);
  font-size: 12.5px;
  color: var(--ink-soft);
  cursor: pointer;
  transition: background 0.12s ease, color 0.12s ease;
}
.filter:last-child {
  border-right: 0;
}
.filter.active {
  background: var(--ochre);
  color: var(--ink);
}
.filter:not(.active):hover {
  background: var(--paper-lift);
  color: var(--ink);
}
.filter__count {
  font-size: 10px;
  color: currentColor;
  opacity: 0.75;
}

.right {
  display: flex;
  align-items: center;
  gap: 10px;
}
.ctrl {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 11px;
  color: var(--ink-mute);
}
.ctrl__label {
  text-transform: uppercase;
  letter-spacing: 0.08em;
}
.ctrl select {
  font-family: var(--sans);
  font-size: 12.5px;
  color: var(--ink);
  background: var(--paper-elevated);
  border: 1px solid var(--rule);
  border-radius: var(--radius-sm);
  padding: 5px 10px;
  cursor: pointer;
}

.search input {
  font-family: var(--sans);
  border: 1px solid var(--rule);
  background: var(--paper-elevated);
  border-radius: var(--radius-sm);
  padding: 7px 12px;
  font-size: 13px;
  min-width: 260px;
  color: var(--ink);
  outline: none;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}
.search input:focus {
  border-color: var(--ochre);
  box-shadow: 0 0 0 3px rgba(90, 160, 168, 0.15);
}

/* ---------- Table ---------- */

.table {
  background: var(--paper-elevated);
  border: 1px solid var(--rule);
  border-radius: var(--radius);
  overflow: hidden;
}
.table__header {
  display: grid;
  grid-template-columns: 22px minmax(0, 1.2fr) minmax(0, 2fr) 96px 44px 60px;
  gap: 16px;
  padding: 6px 16px 8px;
  border-bottom: 2px solid var(--ink-soft);
  background: var(--paper);
  align-items: center;
  position: sticky;
  top: 0;
  z-index: 3;
}
.th {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  background: transparent;
  border: 0;
  padding: 4px 0;
  font-family: var(--sans);
  font-size: 10px;
  letter-spacing: 0.14em;
  text-transform: uppercase;
  color: var(--ink-mute);
  font-weight: 600;
  cursor: pointer;
  transition: color 0.12s ease;
  text-align: left;
  min-width: 0;
  overflow: hidden;
  white-space: nowrap;
}
.th__label {
  overflow: hidden;
  text-overflow: ellipsis;
}
.th--icon-only {
  padding: 4px 0;
  justify-content: center;
  gap: 0;
}
.th:hover {
  color: var(--ink);
}
.th.active {
  color: var(--ink);
}
.th.align-right {
  justify-content: flex-end;
}
.th__arrow {
  font-size: 11px;
  color: var(--ink-mute);
  opacity: 0.7;
  min-width: 8px;
  text-align: center;
}
.th.active .th__arrow {
  color: var(--ochre);
  opacity: 1;
}

/* Sticky group header divider */
.group-header {
  display: grid;
  grid-template-columns: 1fr auto 1fr;
  align-items: center;
  gap: 12px;
  padding: 12px 16px 6px;
  background: var(--paper-elevated);
  border-top: 1px solid var(--rule);
}
.group-header:first-child {
  border-top: 0;
  padding-top: 8px;
}
.group-header__line {
  height: 1px;
  background: var(--rule);
  width: 100%;
}
.group-header__label {
  font-family: var(--serif);
  font-size: 13px;
  color: var(--ink);
  font-weight: 500;
  letter-spacing: -0.005em;
  white-space: nowrap;
}
.group-header__count {
  color: var(--ink-mute);
  font-size: 11px;
  margin-left: 4px;
}

.table__body {
  display: flex;
  flex-direction: column;
}
.table__body > :deep(.row:last-child) {
  border-bottom: 0;
}
.table__footer {
  padding: 6px 16px;
  font-size: 10.5px;
  color: var(--ink-mute);
  background: var(--paper);
  border-top: 1px solid var(--rule);
  text-align: right;
  display: flex;
  justify-content: flex-end;
  gap: 6px;
  align-items: center;
}
.table__footer-hi {
  color: var(--ink);
}
.empty {
  padding: 36px 20px;
  text-align: center;
  color: var(--ink-mute);
  font-size: 15px;
}
</style>
