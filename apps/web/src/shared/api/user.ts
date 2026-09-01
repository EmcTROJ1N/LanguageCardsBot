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

type CardsUserDto = {
  id: number
  keycloakId: string | null
  chatId: number | null
  username: string | null
  createdAt: string
  reminderIntervalMinutes: number
  nextReminderAtUtc: string | null
  hideTranslations: boolean
}

export type TokenResponse = {
  accessToken: string
  refreshToken: string
  expiresIn: number
  tokenType: string
}

let _cardsUser: CardsUserDto | null = null

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
    const [me, cardsResp] = await Promise.all([
      apiFetch<PassportMeDto>('/api/passport/v1/auth/me'),
      apiFetch<{ user: CardsUserDto }>('/api/cards/v3/users/me').catch(() => null),
    ])

    if (cardsResp?.user) {
      _cardsUser = cardsResp.user
    }

    return {
      email: me.email,
      firstName: me.firstName,
      lastName: me.lastName,
      role: me.role === 'Admin' ? 'Admin' : 'User',
      chatId: _cardsUser?.chatId ?? null,
      telegramUsername: _cardsUser?.username ?? null,
      reminderIntervalMinutes: _cardsUser?.reminderIntervalMinutes ?? 90,
      hideTranslations: _cardsUser?.hideTranslations ?? false,
      nextReminderAt: _cardsUser?.nextReminderAtUtc ?? null,
      createdAt: me.createdAt,
      cardsUserId: _cardsUser?.id ?? null,
    }
  },

  async updateProfile(dto: UpdateProfileDto): Promise<Profile> {
    if (_cardsUser !== null) {
      const merged: CardsUserDto = {
        ..._cardsUser,
        reminderIntervalMinutes: dto.reminderIntervalMinutes ?? _cardsUser.reminderIntervalMinutes,
        hideTranslations: dto.hideTranslations ?? _cardsUser.hideTranslations,
      }
      await apiFetch(`/api/cards/v3/users/${_cardsUser.id}`, {
        method: 'PUT',
        body: JSON.stringify({
          keycloakId: merged.keycloakId,
          chatId: merged.chatId,
          username: merged.username,
          reminderIntervalMinutes: merged.reminderIntervalMinutes,
          nextReminderAtUtc: merged.nextReminderAtUtc,
          hideTranslations: merged.hideTranslations,
        }),
      })
      _cardsUser = merged
    }
    return userApi.getProfile()
  },
}
