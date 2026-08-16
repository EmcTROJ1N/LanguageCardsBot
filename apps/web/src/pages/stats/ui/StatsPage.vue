<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { statsApi } from '@/shared/api'
import type { StatsToday } from '@/entities/stats'
import { PageHeader, StatTile, TodoBanner } from '@/shared/ui'

const stats = ref<StatsToday | null>(null)
const levelDist = ref<{ level: number; count: number }[]>([])
const hitmap = ref<number[][]>([])

onMounted(async () => {
  stats.value = await statsApi.getToday()
  levelDist.value = await statsApi.getLevelDistribution()
  hitmap.value = await statsApi.getHitmap()
})

const max = computed(() => Math.max(...levelDist.value.map((l) => l.count), 1))
const totalAccuracy = computed(() => {
  if (!stats.value) return 0
  const total = stats.value.reviewsToday
  const correct = stats.value.correctToday
  return total === 0 ? 0 : Math.round((correct / total) * 100)
})
</script>

<template>
  <section class="stats">
    <PageHeader eyebrow="Chapter · Chronicle" title="Статистика" />

    <div class="grid tiles">
      <StatTile label="Точность всех повторений" :value="totalAccuracy" unit="%" tone="ochre" />
      <StatTile label="Streak" :value="stats?.streakDays ?? 0" unit="дн." tone="rust" />
      <StatTile label="Всего карточек" :value="stats?.totalCards ?? 0" />
      <StatTile label="Выучено" :value="stats?.learned ?? 0" tone="sage" />
    </div>

    <div class="grid-2">
      <article class="panel">
        <header class="panel__head">
          <h2>Активность</h2>
          <span class="mono muted">повторений в день · 12 недель</span>
        </header>
        <div class="hitmap">
          <div v-for="(week, wi) in hitmap" :key="wi" class="hitmap__col">
            <span
              v-for="(v, di) in week"
              :key="di"
              class="cell"
              :data-v="v"
              :title="`неделя ${wi + 1}, день ${di + 1} · интенсивность ${v}/4`"
            />
          </div>
        </div>
        <div class="hitmap__legend">
          <span class="mono muted">меньше</span>
          <span class="cell" data-v="0" />
          <span class="cell" data-v="1" />
          <span class="cell" data-v="2" />
          <span class="cell" data-v="3" />
          <span class="cell" data-v="4" />
          <span class="mono muted">больше</span>
        </div>
      </article>

      <article class="panel">
        <header class="panel__head">
          <h2>По уровням</h2>
          <span class="mono muted">карточек в каждом level'е</span>
        </header>
        <div class="levels">
          <div v-for="l in levelDist" :key="l.level" class="lvl">
            <span class="lvl__label mono">lvl {{ l.level }}</span>
            <div class="lvl__bar" :class="{ learned: l.level === 10 }">
              <span
                class="lvl__fill"
                :style="{ width: (l.count / max) * 100 + '%' }"
              />
            </div>
            <span class="lvl__count mono">{{ l.count }}</span>
          </div>
        </div>
      </article>
    </div>

    <TodoBanner
      label="Streak, hitmap, level distribution"
      text="Streak и hitmap строятся из ReviewEntity — сейчас сгенерированы фронтом. Нужен GET /api/cards/stats/history?days=90 (нового endpoint нет)."
    />
  </section>
</template>

<style scoped>
.stats {
  display: flex;
  flex-direction: column;
  gap: 28px;
}
.lede {
  color: var(--ink-soft);
  font-size: 17px;
  max-width: 640px;
}
.tiles {
  grid-template-columns: repeat(4, 1fr);
}
@media (max-width: 900px) {
  .tiles {
    grid-template-columns: repeat(2, 1fr);
  }
}
.grid-2 {
  display: grid;
  grid-template-columns: minmax(0, 1.4fr) 1fr;
  gap: 24px;
}
@media (max-width: 900px) {
  .grid-2 {
    grid-template-columns: 1fr;
  }
}

.panel {
  background: var(--paper-elevated);
  border: 1px solid var(--rule);
  border-radius: var(--radius);
  padding: 24px 26px;
}
.panel__head {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  margin-bottom: 16px;
}
.panel__head h2 {
  margin: 0;
  font-size: 22px;
}
.muted {
  color: var(--ink-mute);
}

.hitmap {
  display: flex;
  gap: 4px;
  padding-bottom: 8px;
  overflow-x: auto;
}
.hitmap__col {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.cell {
  width: 18px;
  height: 18px;
  border-radius: 3px;
  background: var(--paper-sunk);
  transition: transform 0.12s ease;
  cursor: default;
}
.cell:hover {
  transform: scale(1.15);
}
.cell[data-v='0'] { background: var(--paper-sunk); }
.cell[data-v='1'] { background: rgba(90, 160, 168, 0.22); }
.cell[data-v='2'] { background: rgba(90, 160, 168, 0.48); }
.cell[data-v='3'] { background: var(--ochre); }
.cell[data-v='4'] { background: rgba(90, 160, 168, 0.85); }

.hitmap__legend {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 10px;
  font-size: 11px;
}
.hitmap__legend .cell {
  width: 14px;
  height: 14px;
}

.levels {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.lvl {
  display: grid;
  grid-template-columns: 56px 1fr 40px;
  align-items: center;
  gap: 12px;
}
.lvl__label {
  color: var(--ink-mute);
  font-size: 11px;
}
.lvl__bar {
  height: 14px;
  background: var(--paper-sunk);
  border-radius: 3px;
  overflow: hidden;
  position: relative;
}
.lvl__fill {
  display: block;
  height: 100%;
  background: var(--ink);
  transition: width 0.4s ease;
}
.lvl__bar.learned .lvl__fill {
  background: var(--sage);
}
.lvl__count {
  text-align: right;
  font-size: 12px;
  color: var(--ink);
}
</style>
