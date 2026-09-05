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
  async getToday(userId: number): Promise<StatsToday> {
    const data = await apiFetch<GetTodayStatsResponseDto>(`/api/cards/v3/stats/today/${userId}`)
    const s = data.stats
    return {
      due: 0,
      learned: s.learnedCards,
      totalCards: s.totalCards,
      bestDay: s.bestDay,
      bestCount: s.bestCount,
      reviewsToday: s.totalReviewsToday,
      correctToday: s.correctReviewsToday,
    }
  },
}
