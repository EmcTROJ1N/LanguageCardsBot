import { ref } from 'vue'

export function useImportCards() {
  const file = ref<File | null>(null)
  const isDragging = ref(false)

  function onDrop(e: DragEvent) {
    isDragging.value = false
    const f = e.dataTransfer?.files?.[0]
    if (f) file.value = f
  }

  function onFileSelect(e: Event) {
    const input = e.target as HTMLInputElement
    file.value = input.files?.[0] ?? null
  }

  async function doImport() {
    if (!file.value) return
    // POST /api/cards/import/json — implement when backend is ready
  }

  function reset() {
    file.value = null
    isDragging.value = false
  }

  return { file, isDragging, onDrop, onFileSelect, doImport, reset }
}
