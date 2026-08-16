import type { Profile } from '@/entities/user'
import { apiFetch } from './http'

export type UpdateProfileDto = Partial<
  Pick<Profile, 'firstName' | 'lastName' | 'reminderIntervalMinutes' | 'hideTranslations'>
>

type PassportMeDto = {
  id: string
  email: string
  firstName: string
  lastName: string
  role: string
  createdAt: string
}

export const userApi = {
  async getProfile(): Promise<Profile> {
    // TODO: compose with Cards user (GET /api/cards/v3/users/{id}) for chatId, telegramUsername,
    //       reminderIntervalMinutes, hideTranslations, nextReminderAt.
    //       Requires Passport↔Cards user mapping (integer ID federation).
    const me = await apiFetch<PassportMeDto>('/api/passport/v1/auth/me')
    return {
      email: me.email,
      firstName: me.firstName,
      lastName: me.lastName,
      role: me.role === 'Admin' ? 'Admin' : 'User',
      chatId: null,           // TODO: from Cards user
      telegramUsername: null, // TODO: from Cards user
      reminderIntervalMinutes: 90, // TODO: from Cards user
      hideTranslations: false,     // TODO: from Cards user
      nextReminderAt: null,        // TODO: from Cards user
      createdAt: me.createdAt,
    }
  },

  async updateProfile(_dto: UpdateProfileDto): Promise<Profile> {
    // TODO: split into two requests — PATCH /api/passport/v1/auth/me (firstName, lastName)
    //       and PUT /api/cards/v3/users/{id} (reminderIntervalMinutes, hideTranslations)
    //       once user ID federation is in place
    return userApi.getProfile()
  },
}
