<script setup lang="ts">
defineProps<{
  label: string
  value: string | number
  unit?: string
  hint?: string
  tone?: 'default' | 'ochre' | 'sage' | 'rust'
}>()
</script>

<template>
  <article class="tile" :class="`tone-${tone ?? 'default'}`">
    <span class="tile__label">{{ label }}</span>
    <div class="tile__value">
      <span class="num">{{ value }}</span>
      <span v-if="unit" class="unit">{{ unit }}</span>
    </div>
    <span v-if="hint" class="tile__hint">{{ hint }}</span>
  </article>
</template>

<style scoped>
.tile {
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: 20px 22px 22px;
  background: var(--paper-elevated);
  border: 1px solid var(--rule);
  border-radius: var(--radius);
  position: relative;
  overflow: hidden;
}
.tile::before {
  content: '';
  position: absolute;
  inset: 0 auto 0 0;
  width: 3px;
  background: var(--ink);
}
.tile.tone-ochre::before {
  background: var(--ochre);
}
.tile.tone-sage::before {
  background: var(--sage);
}
.tile.tone-rust::before {
  background: var(--rust);
}
.tile__label {
  font-size: 10.5px;
  letter-spacing: 0.16em;
  text-transform: uppercase;
  color: var(--ink-mute);
  font-weight: 600;
}
.tile__value {
  display: flex;
  align-items: baseline;
  gap: 6px;
  margin-top: 8px;
}
.num {
  font-family: var(--serif);
  font-weight: 400;
  font-size: 44px;
  line-height: 1;
  color: var(--ink);
  font-variation-settings: 'opsz' 144;
}
.unit {
  font-family: var(--serif);
  color: var(--ink-mute);
  font-size: 15px;
}
.tile__hint {
  font-size: 12px;
  color: var(--ink-mute);
  margin-top: 4px;
}
</style>
