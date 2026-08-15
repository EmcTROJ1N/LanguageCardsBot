<script setup lang="ts">
defineProps<{ label?: string; text: string }>()
</script>

<template>
  <aside class="todo">
    <span class="mono-tag">TODO</span>
    <div class="todo__body">
      <strong v-if="label" class="todo__label">{{ label }}</strong>
      <span class="todo__text">{{ text }}</span>
    </div>
  </aside>
</template>

<style scoped>
.todo {
  display: flex;
  gap: 12px;
  align-items: flex-start;
  padding: 9px 14px;
  background: repeating-linear-gradient(
    45deg,
    rgba(198, 143, 75, 0.05),
    rgba(198, 143, 75, 0.05) 6px,
    rgba(198, 143, 75, 0.11) 6px,
    rgba(198, 143, 75, 0.11) 12px
  );
  border: 1px dashed rgba(198, 143, 75, 0.4);
  border-radius: var(--radius-sm);
  color: var(--ink-soft);
  font-size: 12.5px;
}
.todo__body {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}
.todo__label {
  font-family: var(--serif);
  font-weight: 500;
  color: var(--rust);
}
.todo__text {
  color: var(--ink-soft);
}
.mono-tag {
  color: var(--rust);
  border-color: rgba(198, 143, 75, 0.5);
}
</style>
