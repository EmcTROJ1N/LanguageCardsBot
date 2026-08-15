<script setup lang="ts">
import { type Card, statusOf, accuracyOf } from '@/entities/card'
import { computed } from 'vue'

const props = defineProps<{ card: Card }>()

const accuracy = computed(() => {
  const v = accuracyOf(props.card)
  return v === -1 ? null : Math.round(v * 100)
})

const now = new Date('2026-08-11T15:22:00Z')
const status = computed(() => {
  const s = statusOf(props.card)
  const klasses: Record<string, 'sage' | 'ochre' | 'muted'> = {
    learned: 'sage', new: 'ochre', due: 'ochre', queued: 'muted',
  }
  return { label: s, klass: klasses[s] }
})

const nextLabel = computed(() => {
  if (props.card.learned) return '—'
  if (!props.card.nextReviewAt) return 'сегодня'
  const diff = new Date(props.card.nextReviewAt).getTime() - now.getTime()
  const days = Math.round(diff / 86400000)
  if (days < -1) return `просрочено ${-days}д`
  if (days <= 0) return 'сегодня'
  if (days === 1) return 'завтра'
  return `+${days}д`
})
</script>

<template>
  <router-link :to="`/card/${card.id}`" class="row">
    <span class="col-status">
      <span class="status-pill" :class="`s-${status.klass}`" :title="status.label" />
    </span>

    <span class="col-term">
      <span class="term">{{ card.term }}</span>
      <span class="mono trans">{{ card.transcription }}</span>
    </span>

    <span class="col-translation serif">{{ card.translation }}</span>

    <span class="col-level" :title="`level ${card.level} / 10`">
      <span class="lvl-mono mono">{{ String(card.level).padStart(2, '0') }}</span>
      <span class="lvl-bar" :class="{ learned: card.learned }">
        <span class="lvl-fill" :style="{ width: (card.level / 10) * 100 + '%' }" />
      </span>
    </span>

    <span class="col-acc mono">
      <span v-if="accuracy !== null">{{ accuracy }}%</span>
      <span v-else class="muted">—</span>
    </span>

    <span class="col-next mono">{{ nextLabel }}</span>
  </router-link>
</template>

<style scoped>
.row {
  display: grid;
  grid-template-columns: 22px minmax(0, 1.2fr) minmax(0, 2fr) 96px 44px 60px;
  gap: 16px;
  align-items: center;
  padding: 6px 16px;
  border-bottom: 1px solid var(--rule);
  border-radius: 0;
  color: var(--ink);
  transition: background 0.1s ease;
  min-height: 34px;
  font-size: 13px;
}
.row:nth-child(odd) {
  background: rgba(255, 255, 255, 0.008);
}
.row:hover {
  background: rgba(90, 160, 168, 0.06);
}

/* status pill */
.col-status {
  display: flex;
  align-items: center;
  justify-content: center;
}
.status-pill {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: var(--ink-mute);
  display: inline-block;
}
.status-pill.s-ochre {
  background: var(--ochre);
  box-shadow: 0 0 0 2px rgba(90, 160, 168, 0.16);
}
.status-pill.s-sage {
  background: var(--sage);
}
.status-pill.s-muted {
  background: var(--rule-strong);
}

.col-term {
  display: flex;
  align-items: baseline;
  gap: 8px;
  min-width: 0;
}
.term {
  font-family: var(--serif);
  font-size: 15.5px;
  font-weight: 500;
  color: var(--ink);
  letter-spacing: -0.005em;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.trans {
  color: var(--ink-mute);
  font-size: 10.5px;
  white-space: nowrap;
  flex-shrink: 0;
}

.col-translation {
  color: var(--ink-soft);
  font-size: 13px;
  line-height: 1.3;
  font-family: var(--serif);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.col-level {
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--ink-mute);
}
.lvl-mono {
  font-size: 10.5px;
  color: var(--ink-mute);
  min-width: 20px;
}
.lvl-bar {
  height: 4px;
  width: 60px;
  background: var(--paper-sunk);
  border-radius: 2px;
  overflow: hidden;
  display: inline-block;
}
.lvl-fill {
  display: block;
  height: 100%;
  background: var(--ink-soft);
}
.lvl-bar.learned .lvl-fill {
  background: var(--sage);
}

.col-acc {
  font-size: 11.5px;
  color: var(--ink);
  text-align: right;
}
.muted {
  color: var(--ink-mute);
}

.col-next {
  font-size: 11px;
  color: var(--ink-mute);
  text-align: right;
}
</style>
