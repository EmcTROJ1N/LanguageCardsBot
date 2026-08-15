export type Profile = {
  email: string
  firstName: string
  lastName: string
  role: 'User' | 'Admin'
  chatId: number | null
  telegramUsername: string | null
  reminderIntervalMinutes: number
  hideTranslations: boolean
  nextReminderAt: string | null
  createdAt: string
}
