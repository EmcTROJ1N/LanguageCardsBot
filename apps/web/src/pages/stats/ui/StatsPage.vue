<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { statsApi } from '@/shared/api'
import type { StatsToday } from '@/entities/stats'
import { PageHeader, StatTile } from '@/shared/ui'

const stats = ref<StatsToday | null>(null)
const levelDist = ref<{ level: number; count: number }[]>([])

onMounted(async () => {
  stats.value = await statsApi.getToday()
  levelDist.value = await statsApi.getLevelDistribution()
})

const max = computed(() => Math.max(...levelDist.value.map((l) => l.count), 1))
const totalAccuracy = computed(() => {
  if (!stats.value) return 0
  const total = stats.value.reviewsToday
  const correct = stats.value.correctToday
  return total === 0 ? 0 : Math.round((correct / total) * 100)
})

const bestDayLabel = computed(() => {
  if (!stats.value?.bestDay) return '—'
  return new Date(stats.value.bestDay).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' })
})
</script>

<template>
  <section class="stats">
    <PageHeader eyebrow="Chapter · Chronicle" title="Статистика" />

    <div class="grid tiles">
      <StatTile label="Точность всех повторений" :value="totalAccuracy" unit="%" tone="ochre" />
      <StatTile label="Лучший день" :value="stats?.bestCount ?? 0" :unit="bestDayLabel" tone="rust" />
      <StatTile label="Всего карточек" :value="stats?.totalCards ?? 0" />
      <StatTile label="Выучено" :value="stats?.learned ?? 0" tone="sage" />
    </div>

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
