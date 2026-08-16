<script setup lang="ts">
withDefaults(defineProps<{
  tone?: 'default' | 'sage' | 'ochre' | 'rust' | 'ink'
}>(), { tone: 'default' })
</script>

<template>
  <span class="chip" :class="tone !== 'default' ? tone : undefined">
    <slot />
  </span>
</template>
