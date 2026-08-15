export type Card = {
  id: number
  term: string
  translation: string
  transcription: string
  example?: string
  level: number
  learned: boolean
  nextReviewAt: string | null
  lastReviewAt: string | null
  createdAt: string
  totalReviews: number
  correctReviews: number
}

export type CardStatus = 'new' | 'due' | 'queued' | 'learned'

// Frozen to mock date; replace with Date.now() when integrating real API.
const _mockNow = new Date('2026-08-11T15:22:00Z')

export function statusOf(card: Card): CardStatus {
  if (card.learned) return 'learned'
  if (card.totalReviews === 0 || !card.nextReviewAt) return 'new'
  return new Date(card.nextReviewAt) <= _mockNow ? 'due' : 'queued'
}

export function statusRank(card: Card): number {
  const ranks: Record<CardStatus, number> = { new: 0, due: 1, queued: 2, learned: 3 }
  return ranks[statusOf(card)]
}

export function accuracyOf(card: Card): number {
  return card.totalReviews === 0 ? -1 : card.correctReviews / card.totalReviews
}

export function nextReviewTimestamp(card: Card): number {
  if (card.learned) return Number.MAX_SAFE_INTEGER
  if (!card.nextReviewAt) return -1
  return new Date(card.nextReviewAt).getTime()
}
