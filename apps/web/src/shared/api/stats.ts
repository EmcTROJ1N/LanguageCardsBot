import type { StatsToday } from '@/entities/stats'

const _statsToday: StatsToday = {
  due: 17, learned: 84, totalCards: 213,
  streakDays: 12, reviewsToday: 22, correctToday: 18,
}

const _levelDistribution: { level: number; count: number }[] = [
  { level: 1, count: 21 }, { level: 2, count: 18 }, { level: 3, count: 24 },
  { level: 4, count: 19 }, { level: 5, count: 22 }, { level: 6, count: 15 },
  { level: 7, count: 12 }, { level: 8, count: 9 },  { level: 9, count: 6 },
  { level: 10, count: 84 },
]

function _generateHitmap(): number[][] {
  const seed = 42
  let s = seed
  const rand = () => {
    s = (s * 1664525 + 1013904223) % 4294967296
    return s / 4294967296
  }
  const weeks: number[][] = []
  for (let w = 0; w < 12; w++) {
    const week: number[] = []
    for (let d = 0; d < 7; d++) {
      const base = rand()
      const boost = w >= 10 ? 0.35 : 0
      const val = base + boost
      week.push(val < 0.35 ? 0 : val < 0.55 ? 1 : val < 0.75 ? 2 : val < 0.9 ? 3 : 4)
    }
    weeks.push(week)
  }
  return weeks
}

export const statsApi = {
  async getToday(): Promise<StatsToday> {
    return { ..._statsToday }
  },
  async getLevelDistribution(): Promise<{ level: number; count: number }[]> {
    return [..._levelDistribution]
  },
  async getHitmap(): Promise<number[][]> {
    return _generateHitmap()
  },
}
