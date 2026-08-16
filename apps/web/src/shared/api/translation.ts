export type TranslationResult = {
  translation: string
  transcription: string
  example: string
}

const _seed: Record<string, TranslationResult> = {
  lambent: {
    translation: 'мерцающий, играющий (о свете)',
    transcription: '/ˈlambənt/',
    example: 'Lambent flames danced on the ceiling.',
  },
  obfuscate: {
    translation: 'затуманивать, запутывать',
    transcription: '/ˈɒbfʌskeɪt/',
    example: 'The report was written to obfuscate rather than inform.',
  },
}

export const translationApi = {
  async translate(term: string): Promise<TranslationResult> {
    return (
      _seed[term.toLowerCase().trim()] ?? {
        translation: '(автоперевод сюда)',
        transcription: '/ˈautoːgen/',
        example: 'A sentence hint would appear here.',
      }
    )
  },
}
