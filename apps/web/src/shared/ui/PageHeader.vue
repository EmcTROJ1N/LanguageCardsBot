<script setup lang="ts">
defineProps<{ eyebrow: string; title: string }>()
</script>

<template>
  <header class="page-header">
    <span class="eyebrow">{{ eyebrow }}</span>
    <h1 class="display">{{ title }}</h1>
    <p v-if="$slots.default" class="page-header__lede serif">
      <slot />
    </p>
  </header>
</template>

<style scoped>
.page-header {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.page-header__lede {
  color: var(--ink-soft);
  font-size: 17px;
  margin: 4px 0 0;
  max-width: 640px;
}
</style>
