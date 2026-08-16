import { ref, computed } from 'vue'

export function useLinkTelegram() {
  const step = ref<1 | 2 | 3>(1)
  const code = ref('AURORA · 4128 · MURMUR')
  const stepLabel = computed(() => `Шаг ${step.value} из 3`)

  function goToBot() {
    step.value = 2
  }

  function confirmSuccess(onDone: () => void) {
    step.value = 3
    setTimeout(onDone, 800)
  }

  function reset() {
    step.value = 1
  }

  return { step, code, stepLabel, goToBot, confirmSuccess, reset }
}
