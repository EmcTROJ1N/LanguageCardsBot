import { ref } from 'vue'
import { cardsApi } from '@/shared/api'

const CHUNK_SIZE = 500

type ImportCard = {
  term?: string
  translation?: string
  transcription?: string
  example?: string | null
}

type ImportResult = {
  imported: number
  skipped: number
}

function extractCards(fileText: string): ImportCard[] {
  let parsed: unknown
  try {
    parsed = JSON.parse(fileText)
  } catch {
    throw new Error('Файл не является валидным JSON')
  }

  if (Array.isArray(parsed)) return parsed as ImportCard[]

  if (parsed && typeof parsed === 'object' && 'cards' in parsed) {
    const { cards } = parsed as { cards: unknown }
    if (Array.isArray(cards)) return cards as ImportCard[]
  }

  throw new Error('В файле не найден массив карточек `cards`. Используйте /export для примера.')
}

export function useImportCards() {
  const file = ref<File | null>(null)
  const isDragging = ref(false)
  const importing = ref(false)
  const result = ref<ImportResult | null>(null)
  const error = ref<string | null>(null)

  function onDrop(e: DragEvent) {
    isDragging.value = false
    const f = e.dataTransfer?.files?.[0]
    if (f) {
      file.value = f
      result.value = null
      error.value = null
    }
  }

  function onFileSelect(e: Event) {
    const input = e.target as HTMLInputElement
    file.value = input.files?.[0] ?? null
    result.value = null
    error.value = null
  }

  async function doImport() {
    if (!file.value || importing.value) return

    importing.value = true
    result.value = null
    error.value = null

    try {
      const fileText = await file.value.text()
      const cards = extractCards(fileText)

      const userId = await cardsApi.getMyUserId()

      let totalImported = 0
      let totalSkipped = 0

      for (let offset = 0; offset < cards.length; offset += CHUNK_SIZE) {
        const chunk = cards.slice(offset, offset + CHUNK_SIZE)
        const chunkJson = JSON.stringify({ cards: chunk })
        const chunkResult = await cardsApi.importJsonChunk(chunkJson, userId)
        totalImported += chunkResult.imported
        totalSkipped += chunkResult.skipped
      }

      result.value = { imported: totalImported, skipped: totalSkipped }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Неизвестная ошибка'
    } finally {
      importing.value = false
    }
  }

  function reset() {
    file.value = null
    isDragging.value = false
    result.value = null
    error.value = null
  }

  return { file, isDragging, importing, result, error, onDrop, onFileSelect, doImport, reset }
}
