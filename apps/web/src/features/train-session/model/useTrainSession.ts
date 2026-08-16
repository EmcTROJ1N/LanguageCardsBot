import { ref, computed, onMounted } from 'vue'
import { cardsApi, reviewIntervalDays } from '@/shared/api'
import { getDueCards } from '@/entities/card'
import type { Card } from '@/entities/card'

// Module-level singleton state — shared across all callers (TrainPage + TrainCard)
const queue = ref<Card[]>([])
const results = ref<{ id: number; correct: boolean }[]>([])
const isFlipped = ref(false)

export function useTrainSession() {
  onMounted(async () => {
    const all = await cardsApi.getAll()
    queue.value = getDueCards(all)
  })

  const current = computed(() => queue.value[0] ?? null)
  const isFinished = computed(() => !current.value && results.value.length > 0)
  const correctCount = computed(() => results.value.filter((r) => r.correct).length)
  const progress = computed(() => {
    const total = queue.value.length + results.value.length
    return `${results.value.length + 1} / ${total}`
  })
  const nextIntervalDays = computed(() => {
    if (!current.value) return null
    const nextLevel = Math.min(current.value.level + 1, 10)
    return reviewIntervalDays[nextLevel - 1]
  })

  function flip() {
    isFlipped.value = true
  }

  function answer(correct: boolean) {
    if (!current.value) return
    results.value.push({ id: current.value.id, correct })
    isFlipped.value = false
    queue.value.shift()
  }

  async function restart() {
    const all = await cardsApi.getAll()
    queue.value = getDueCards(all)
    results.value = []
    isFlipped.value = false
  }

  return { queue, current, results, isFlipped, isFinished, correctCount, progress, nextIntervalDays, flip, answer, restart }
}
