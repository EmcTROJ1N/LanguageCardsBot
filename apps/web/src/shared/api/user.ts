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

export type TokenResponse = {
  accessToken: string
  refreshToken: string
  expiresIn: number
  tokenType: string
}

export const userApi = {
  async login(email: string, password: string): Promise<TokenResponse> {
    return apiFetch<TokenResponse>('/api/passport/v1/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    })
  },

  async register(
    email: string,
    password: string,
    firstName: string,
    lastName: string,
  ): Promise<void> {
    await apiFetch<void>('/api/passport/v1/auth/register', {
      method: 'POST',
      body: JSON.stringify({ email, password, firstName, lastName }),
    })
  },

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
      chatId: null,
      telegramUsername: null,
      reminderIntervalMinutes: 90,
      hideTranslations: false,
      nextReminderAt: null,
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
