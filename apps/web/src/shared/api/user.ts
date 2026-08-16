import type { Profile } from '@/entities/user'

export type UpdateProfileDto = Partial<
  Pick<Profile, 'firstName' | 'lastName' | 'reminderIntervalMinutes' | 'hideTranslations'>
>

const _profile: Profile = {
  email: 'german@yetiora.com',
  firstName: 'German',
  lastName: 'Pokrovskiy',
  role: 'User',
  chatId: 483920174,
  telegramUsername: 'gpokrovskiy',
  reminderIntervalMinutes: 90,
  hideTranslations: true,
  nextReminderAt: '2026-08-11T18:30:00Z',
  createdAt: '2026-05-14T12:04:00Z',
}

export const userApi = {
  async getProfile(): Promise<Profile> {
    return { ..._profile }
  },
  async updateProfile(dto: UpdateProfileDto): Promise<Profile> {
    Object.assign(_profile, dto)
    return { ..._profile }
  },
}
