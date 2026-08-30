import type { Card } from '@/entities/card'
import { apiFetch, apiDownload } from './http'

export type ImportChunkResult = {
  imported: number
  skipped: number
  errors: string[]
}

export type CreateCardDto = {
  term: string
  translation: string
  transcription: string
  example?: string
}

export type UpdateCardDto = Partial<CreateCardDto>

export const reviewIntervalDays = [1, 1, 2, 4, 7, 14, 21, 21, 19, 0]

type CardDto = {
  id: number
  userId: number
  term: string
  translation: string
  transcription: string
  example?: string | null
  level: number
  nextReviewAt: string | null
  learned: boolean
  createdAt: string
  lastReviewAt: string | null
  totalReviews: number
  correctReviews: number
}

function toCard(dto: CardDto): Card {
  return {
    id: dto.id,
    term: dto.term,
    translation: dto.translation,
    transcription: dto.transcription,
    example: dto.example ?? undefined,
    level: dto.level,
    learned: dto.learned,
    nextReviewAt: dto.nextReviewAt,
    lastReviewAt: dto.lastReviewAt,
    createdAt: dto.createdAt,
    totalReviews: dto.totalReviews,
    correctReviews: dto.correctReviews,
  }
}

export const cardsApi = {
  async getAll(): Promise<Card[]> {
    const data = await apiFetch<{ cards: CardDto[] }>('/api/cards/cards')
    return data.cards.map(toCard)
  },

  async getById(id: number): Promise<Card | undefined> {
    const data = await apiFetch<{ card: CardDto | null }>(`/api/cards/cards/${id}`)
    return data.card ? toCard(data.card) : undefined
  },

  async create(dto: CreateCardDto): Promise<Card> {
    const data = await apiFetch<{ card: CardDto }>('/api/cards/cards', {
      method: 'POST',
      body: JSON.stringify({
        term: dto.term,
        translation: dto.translation,
        transcription: dto.transcription,
        example: dto.example ?? null,
      }),
    })
    return toCard(data.card)
  },

  async update(id: number, dto: UpdateCardDto): Promise<Card> {
    await apiFetch<{ updated: boolean }>(`/api/cards/cards/${id}`, {
      method: 'PUT',
      body: JSON.stringify({
        term: dto.term ?? '',
        translation: dto.translation ?? '',
        transcription: dto.transcription ?? '',
        example: dto.example ?? null,
        hasExample: 'example' in dto,
        learned: null,
      }),
    })
    // Backend returns only bool; refetch to get the updated card
    const refreshed = await apiFetch<{ card: CardDto | null }>(`/api/cards/cards/${id}`)
    if (!refreshed.card) throw new Error(`Card ${id} not found after update`)
    return toCard(refreshed.card)
  },

  async export(format: 'json' | 'csv'): Promise<void> {
    await apiDownload(`/api/cards/cards/export?format=${format}`, `cards.${format}`)
  },

  async delete(id: number): Promise<void> {
    await apiFetch<{ deleted: boolean }>(`/api/cards/cards/${id}`, { method: 'DELETE' })
  },

  async recordReview(cardId: number, isCorrect: boolean): Promise<void> {
    await apiFetch<unknown>(`/api/cards/cards/${cardId}/review`, {
      method: 'POST',
      body: JSON.stringify({ isCorrect }),
    })
  },

  async getMyUserId(): Promise<number> {
    const data = await apiFetch<{ user: { id: number } }>('/api/cards/v3/users/me')
    return data.user.id
  },

  async importJsonChunk(json: string, userId: number): Promise<ImportChunkResult> {
    const data = await apiFetch<{
      isSuccess: boolean
      data: { imported: number; skipped: number; errors: string[] } | null
      errors: { message: string }[]
    }>('/api/cards/import/json', {
      method: 'POST',
      body: JSON.stringify({ json, userId }),
    })
    if (!data.isSuccess) {
      throw new Error(data.errors.map(e => e.message).join('; ') || 'Import failed')
    }
    return {
      imported: data.data?.imported ?? 0,
      skipped: data.data?.skipped ?? 0,
      errors: data.data?.errors ?? [],
    }
  },
}
