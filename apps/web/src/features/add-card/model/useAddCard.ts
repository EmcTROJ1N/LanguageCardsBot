import { ref, computed } from 'vue'
import { cardsApi, translationApi } from '@/shared/api'
import type { CreateCardDto } from '@/shared/api'

export function useAddCard() {
  const term = ref('')
  const translation = ref('')
  const transcription = ref('')
  const example = ref('')
  const autoTranslated = ref(false)
  const saving = ref(false)

  const canSave = computed(() => term.value.trim() !== '' && translation.value.trim() !== '')

  async function autoTranslate() {
    if (!term.value.trim()) return
    const result = await translationApi.translate(term.value)
    translation.value = result.translation
    transcription.value = result.transcription
    example.value = result.example
    autoTranslated.value = true
  }

  async function save() {
    if (!canSave.value || saving.value) return
    saving.value = true
    const dto: CreateCardDto = {
      term: term.value.trim(),
      translation: translation.value.trim(),
      transcription: transcription.value.trim(),
      example: example.value.trim() || undefined,
    }
    await cardsApi.create(dto)
    saving.value = false
    term.value = ''
    translation.value = ''
    transcription.value = ''
    example.value = ''
    autoTranslated.value = false
  }

  return { term, translation, transcription, example, autoTranslated, canSave, saving, autoTranslate, save }
}
