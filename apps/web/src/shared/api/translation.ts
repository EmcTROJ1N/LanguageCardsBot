import { apiFetch } from './http'

export type TranslationResult = {
  translation: string
  transcription: string
  example: string
}

export const translationApi = {
  async translate(term: string): Promise<TranslationResult> {
    const data = await apiFetch<{ result: TranslationResult }>('/api/cards/v3/translation', {
      method: 'POST',
      body: JSON.stringify({ term }),
    })
    return data.result
  },
}
