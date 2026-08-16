<script setup lang="ts">
import type { RouteLocationRaw } from 'vue-router'
import { RouterLink } from 'vue-router'

withDefaults(defineProps<{
  variant?: 'default' | 'ghost' | 'ochre' | 'sage' | 'rust'
  size?: 'sm' | 'md' | 'lg'
  disabled?: boolean
  type?: 'button' | 'submit' | 'reset'
  to?: RouteLocationRaw
}>(), { variant: 'default', size: 'md', type: 'button' })
</script>

<template>
  <RouterLink
    v-if="to"
    :to="to"
    class="btn"
    :class="[variant !== 'default' && variant, size !== 'md' && size]"
  >
    <slot />
  </RouterLink>
  <button
    v-else
    class="btn"
    :class="[variant !== 'default' && variant, size !== 'md' && size]"
    :type="type"
    :disabled="disabled"
  >
    <slot />
  </button>
</template>
