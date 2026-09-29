const TRANSLATE_URL = 'https://translate.googleapis.com/translate_a/single'
const TARGET_LANGUAGE = 'ru'

export type TranslationResult = {
  translation: string
  transcription: string
  example: string
}

export const translationApi = {
  async translate(term: string): Promise<TranslationResult> {
    const url = `${TRANSLATE_URL}?client=gtx&sl=auto&tl=${TARGET_LANGUAGE}&dt=t&q=${encodeURIComponent(term)}`
    const response = await fetch(url)

    if (!response.ok) {
      throw new Error(`Translation failed: HTTP ${response.status}`)
    }

    const data = await response.json()
    const translation = parseTranslation(data)

    if (!translation) {
      throw new Error('Translation provider returned an empty result.')
    }

    return { translation, transcription: '', example: '' }
  },
}

function parseTranslation(data: unknown): string {
  if (!Array.isArray(data) || !Array.isArray((data as unknown[][])[0])) return ''
  return (data[0] as unknown[][])
    .filter((seg): seg is [string, ...unknown[]] => Array.isArray(seg) && typeof seg[0] === 'string')
    .map((seg) => seg[0])
    .join('')
    .trim()
}
