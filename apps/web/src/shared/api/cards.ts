import type { Card } from '@/entities/card'

export type CreateCardDto = {
  term: string
  translation: string
  transcription: string
  example?: string
}

export type UpdateCardDto = Partial<CreateCardDto>

const _now = new Date('2026-08-11T15:22:00Z')

function isoOffset(hours: number): string {
  return new Date(_now.getTime() + hours * 3600 * 1000).toISOString()
}

export const reviewIntervalDays = [1, 1, 2, 4, 7, 14, 21, 21, 19, 0]

const _mockCards: Card[] = [
  {
    id: 1,
    term: 'petrichor',
    translation: 'запах земли после дождя',
    transcription: '/ˈpɛtrɪkɔːr/',
    example: 'The petrichor filled the air after the summer storm passed.',
    level: 3,
    learned: false,
    nextReviewAt: isoOffset(-3),
    lastReviewAt: isoOffset(-72),
    createdAt: '2026-07-22T09:14:00Z',
    totalReviews: 4,
    correctReviews: 3,
  },
  {
    id: 2,
    term: 'sonder',
    translation: 'осознание, что каждый прохожий проживает свою жизнь',
    transcription: '/ˈsɒndər/',
    example: 'She felt a sudden sonder while watching the crowd at the station.',
    level: 2,
    learned: false,
    nextReviewAt: isoOffset(-1),
    lastReviewAt: isoOffset(-24),
    createdAt: '2026-08-03T22:41:00Z',
    totalReviews: 2,
    correctReviews: 1,
  },
  {
    id: 3,
    term: 'ephemeral',
    translation: 'мимолётный, недолговечный',
    transcription: '/ɪˈfɛm(ə)r(ə)l/',
    example: 'Fame in the digital age is often ephemeral.',
    level: 6,
    learned: false,
    nextReviewAt: isoOffset(96),
    lastReviewAt: isoOffset(-240),
    createdAt: '2026-06-11T18:00:00Z',
    totalReviews: 8,
    correctReviews: 7,
  },
  {
    id: 4,
    term: 'quiescent',
    translation: 'спокойный, находящийся в состоянии покоя',
    transcription: '/kwɪˈɛsnt/',
    example: 'The volcano has been quiescent for over a century.',
    level: 10,
    learned: true,
    nextReviewAt: null,
    lastReviewAt: isoOffset(-720),
    createdAt: '2026-05-01T10:00:00Z',
    totalReviews: 12,
    correctReviews: 12,
  },
  {
    id: 5,
    term: 'susurrus',
    translation: 'тихий шелест, шёпот',
    transcription: '/sʊˈsʌrəs/',
    example: 'A susurrus of leaves accompanied their walk through the grove.',
    level: 1,
    learned: false,
    nextReviewAt: isoOffset(-0.5),
    lastReviewAt: null,
    createdAt: '2026-08-10T20:12:00Z',
    totalReviews: 0,
    correctReviews: 0,
  },
  {
    id: 6,
    term: 'liminal',
    translation: 'пороговый, промежуточный',
    transcription: '/ˈlɪmɪn(ə)l/',
    example: 'Airports are liminal spaces — neither departure nor arrival.',
    level: 4,
    learned: false,
    nextReviewAt: isoOffset(72),
    lastReviewAt: isoOffset(-96),
    createdAt: '2026-07-05T14:00:00Z',
    totalReviews: 5,
    correctReviews: 4,
  },
  {
    id: 7,
    term: 'meander',
    translation: 'извиваться, блуждать',
    transcription: '/miˈandər/',
    example: 'We meandered through the old town without a map.',
    level: 5,
    learned: false,
    nextReviewAt: isoOffset(-8),
    lastReviewAt: isoOffset(-168),
    createdAt: '2026-06-20T11:30:00Z',
    totalReviews: 6,
    correctReviews: 5,
  },
  {
    id: 8,
    term: 'redolent',
    translation: 'пахнущий, напоминающий (о чём-либо)',
    transcription: '/ˈrɛdələnt/',
    example: 'The room was redolent of pipe tobacco and old books.',
    level: 7,
    learned: false,
    nextReviewAt: isoOffset(48),
    lastReviewAt: isoOffset(-336),
    createdAt: '2026-06-01T09:00:00Z',
    totalReviews: 9,
    correctReviews: 8,
  },
  {
    id: 9,
    term: 'palimpsest',
    translation: 'палимпсест; наслоение смыслов',
    transcription: '/ˈpalɪm(p)sɛst/',
    example: 'The city is a palimpsest of every era it has lived through.',
    level: 8,
    learned: false,
    nextReviewAt: isoOffset(160),
    lastReviewAt: isoOffset(-504),
    createdAt: '2026-05-18T16:00:00Z',
    totalReviews: 10,
    correctReviews: 9,
  },
  {
    id: 10,
    term: 'saudade',
    translation: 'тоска, светлая грусть (порт.)',
    transcription: '/saʊˈdɑːdə/',
    example: 'A saudade for summers past crept into every song on the record.',
    level: 3,
    learned: false,
    nextReviewAt: isoOffset(-2),
    lastReviewAt: isoOffset(-48),
    createdAt: '2026-07-28T21:00:00Z',
    totalReviews: 3,
    correctReviews: 2,
  },
  {
    id: 11,
    term: 'tessellate',
    translation: 'выкладывать мозаикой, замащивать',
    transcription: '/ˈtɛsəleɪt/',
    example: 'These tiles tessellate perfectly across the floor.',
    level: 2,
    learned: false,
    nextReviewAt: isoOffset(24),
    lastReviewAt: isoOffset(-72),
    createdAt: '2026-08-05T13:00:00Z',
    totalReviews: 2,
    correctReviews: 1,
  },
  {
    id: 12,
    term: 'apricity',
    translation: 'тёплое зимнее солнце',
    transcription: '/əˈprɪsɪti/',
    example: 'The apricity on the bench made January bearable.',
    level: 1,
    learned: false,
    nextReviewAt: isoOffset(0),
    lastReviewAt: null,
    createdAt: '2026-08-11T09:00:00Z',
    totalReviews: 0,
    correctReviews: 0,
  },
  ...(
    [
      ['numinous', 'потусторонний, священный', '/ˈnjuːmɪnəs/', 5, false, -6, 168, 6, 5],
      ['ineffable', 'невыразимый', '/ɪnˈɛfəbl/', 4, false, 24, 96, 5, 4],
      ['soporific', 'снотворный, усыпляющий', '/sɒpəˈrɪfɪk/', 3, false, -4, 72, 3, 2],
      ['obfuscate', 'запутывать, затуманивать', '/ˈɒbfʌskeɪt/', 6, false, 120, 240, 7, 6],
      ['lambent', 'играющий, мерцающий (о свете)', '/ˈlambənt/', 2, false, -2, 48, 2, 1],
      ['penumbra', 'полутень', '/pɪˈnʌmbrə/', 7, false, 96, 168, 8, 7],
      ['halcyon', 'безмятейный, спокойный (о времени)', '/ˈhalsɪən/', 10, true, null, 720, 12, 12],
      ['sonorous', 'звучный, мелодичный', '/səˈnɔːrəs/', 5, false, -1, 120, 6, 5],
      ['perspicacious', 'проницательный', '/pɜːspɪˈkeɪʃəs/', 4, false, 48, 96, 5, 4],
      ['recondite', 'малопонятный, эзотерический', '/ˈrɛkəndaɪt/', 3, false, 12, 72, 3, 2],
      ['insouciance', 'беззаботность', '/ɪnˈsuːsɪəns/', 6, false, -12, 240, 7, 6],
      ['querulous', 'ворчливый, жалующийся', '/ˈkwɛr(j)ʊləs/', 2, false, 12, 48, 2, 1],
      ['sanguine', 'жизнерадостный', '/ˈsaŋɡwɪn/', 8, false, 336, 336, 10, 9],
      ['loquacious', 'словоохотливый, разговорчивый', '/ləˈkweɪʃəs/', 5, false, -20, 168, 6, 5],
      ['taciturn', 'молчаливый, немногословный', '/ˈtasɪtɜːn/', 7, false, 72, 336, 8, 7],
      ['ephemeron', 'нечто мимолётное', '/ɪˈfɛmərɒn/', 1, false, 0, null, 0, 0],
      ['obstreperous', 'шумный, крикливый', '/əbˈstrɛpərəs/', 3, false, -3, 72, 4, 3],
      ['peregrination', 'странствие', '/ˌpɛrɪɡrɪˈneɪʃn/', 6, false, 120, 336, 7, 6],
      ['sesquipedalian', 'длиннословный', '/sɛskwɪpɪˈdeɪlɪən/', 2, false, -1, 48, 2, 1],
      ['recalcitrant', 'непокорный', '/rɪˈkalsɪtrənt/', 5, false, 48, 168, 6, 5],
      ['crepuscular', 'сумеречный', '/krɪˈpʌskjələr/', 10, true, null, 900, 11, 11],
      ['limerence', 'острая влюблённость', '/ˈlɪmərəns/', 4, false, 24, 96, 5, 4],
      ['diaphanous', 'полупрозрачный, тонкий', '/dʌɪˈafənəs/', 3, false, -8, 72, 3, 2],
      ['abnegation', 'самоотречение', '/ˌabnɪˈɡeɪʃn/', 7, false, 168, 336, 8, 7],
      ['pellucid', 'прозрачный, ясный', '/pɪˈl(j)uːsɪd/', 6, false, 96, 168, 7, 6],
      ['halberd', 'алебарда', '/ˈhalbəd/', 1, false, 8, null, 0, 0],
      ['peripatetic', 'странствующий, бродячий', '/ˌpɛrɪpəˈtɛtɪk/', 5, false, -6, 120, 6, 4],
      ['cerulean', 'лазурный', '/sɪˈruːlɪən/', 8, false, 240, 336, 9, 8],
      ['solipsism', 'солипсизм', '/ˈsɒlɪpsɪz(ə)m/', 2, false, 4, 48, 2, 1],
      ['effulgent', 'сияющий, лучезарный', '/ɪˈfʌldʒ(ə)nt/', 9, false, 336, 504, 11, 10],
      ['penumbra', 'полутень (краевая зона)', '/pɪˈnʌmbrə/', 4, false, 12, 96, 5, 4],
      ['sussurration', 'шёпот, шелест (устар.)', '/ˌsʌsəˈreɪʃn/', 1, false, 2, null, 0, 0],
      ['limn', 'изображать, рисовать (лит.)', '/lɪm/', 3, false, -5, 72, 3, 2],
      ['inchoate', 'зарождающийся, неоформленный', '/ɪnˈkəʊət/', 7, false, 96, 336, 8, 7],
      ['cognoscente', 'знаток', '/kɒnjəʊˈʃɛnti/', 5, false, 24, 168, 6, 5],
      ['fugacious', 'мимолётный', '/fjuːˈɡeɪʃəs/', 10, true, null, 800, 13, 13],
    ] as const
  ).map((row, i) => ({
    id: 100 + i,
    term: row[0] as string,
    translation: row[1] as string,
    transcription: row[2] as string,
    example: undefined,
    level: row[3] as number,
    learned: row[4] as boolean,
    nextReviewAt: row[5] === null ? null : isoOffset(row[5] as number),
    lastReviewAt: row[6] === null ? null : isoOffset(-(row[6] as number)),
    createdAt: new Date(
      new Date('2026-05-01T00:00:00Z').getTime() + i * 86400000 * 2,
    ).toISOString(),
    totalReviews: row[7] as number,
    correctReviews: row[8] as number,
  })),
]

export const cardsApi = {
  async getAll(): Promise<Card[]> {
    return [..._mockCards]
  },
  async getById(id: number): Promise<Card | undefined> {
    return _mockCards.find((c) => c.id === id)
  },
  async create(dto: CreateCardDto): Promise<Card> {
    const card: Card = {
      id: Date.now(),
      term: dto.term,
      translation: dto.translation,
      transcription: dto.transcription,
      example: dto.example,
      level: 1,
      learned: false,
      nextReviewAt: null,
      lastReviewAt: null,
      createdAt: new Date().toISOString(),
      totalReviews: 0,
      correctReviews: 0,
    }
    _mockCards.push(card)
    return card
  },
  async update(id: number, dto: UpdateCardDto): Promise<Card> {
    const idx = _mockCards.findIndex((c) => c.id === id)
    if (idx === -1) throw new Error(`Card ${id} not found`)
    _mockCards[idx] = { ..._mockCards[idx], ...dto }
    return _mockCards[idx]
  },
  async delete(id: number): Promise<void> {
    const idx = _mockCards.findIndex((c) => c.id === id)
    if (idx !== -1) _mockCards.splice(idx, 1)
  },
}
