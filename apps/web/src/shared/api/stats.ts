import type { StatsToday } from '@/entities/stats'
import { apiFetch } from './http'

type TodayStatsDto = {
  newToday: number
  totalReviewsToday: number
  correctReviewsToday: number
  totalCards: number
  learnedCards: number
  bestDay: string | null
  bestCount: number
}

type GetTodayStatsResponseDto = {
  stats: TodayStatsDto
}

export const statsApi = {
  async getToday(): Promise<StatsToday> {
    // TODO: replace userId:0 with real user ID from auth session
    const data = await apiFetch<GetTodayStatsResponseDto>('/api/cards/v3/stats/today/0')
    const s = data.stats
    return {
      // TODO: "due" count — backend has no dedicated endpoint; derive from cards or extend StatsController
      due: 0,
      learned: s.learnedCards,
      totalCards: s.totalCards,
      // TODO: streakDays — not in backend response; needs ReviewEntity aggregation
      streakDays: 0,
      reviewsToday: s.totalReviewsToday,
      correctToday: s.correctReviewsToday,
    }
  },

  async getLevelDistribution(): Promise<{ level: number; count: number }[]> {
    // TODO: no backend endpoint; needs GET /api/cards/v3/stats/level-distribution
    return []
  },

  async getHitmap(): Promise<number[][]> {
    // TODO: no backend endpoint; needs GET /api/cards/v3/stats/history?days=84
    return []
  },
}
