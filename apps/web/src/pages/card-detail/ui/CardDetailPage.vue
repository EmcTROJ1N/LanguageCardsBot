<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import { cardsApi } from '@/shared/api'
import { PageHeader } from '@/shared/ui'
import TodoBanner from '@/shared/ui/TodoBanner.vue'
import type { Card } from '@/entities/card'

const route = useRoute()
const allCards = ref<Card[]>([])
onMounted(async () => { allCards.value = await cardsApi.getAll() })

const card = computed(() =>
  allCards.value.find((c) => c.id === Number(route.params.id)) ?? allCards.value[0],
)

const accuracy = computed(() =>
  !card.value || card.value.totalReviews === 0
    ? null
    : Math.round((card.value.correctReviews / card.value.totalReviews) * 100),
)

const history = computed(() => {
  // Мок истории повторений: 8 последних решений, чередующиеся
  const now = new Date('2026-08-11T15:22:00Z')
  const result: { at: Date; correct: boolean; levelBefore: number; levelAfter: number }[] = []
  if (!card.value) return result
  let level = 1
  for (let i = 0; i < Math.min(card.value.totalReviews, 8); i++) {
    const at = new Date(now.getTime() - (7 - i) * 86400000 * 2)
    const correct = i !== 2 // одна ошибка для наглядности
    const before = level
    level = correct ? Math.min(level + 1, card.value.level) : 1
    result.push({ at, correct, levelBefore: before, levelAfter: level })
  }
  return result.reverse()
})
</script>

<template>
  <section class="detail">
    <div class="crumbs">
      <RouterLink to="/deck">← в колоду</RouterLink>
      <span class="mono">/ card № {{ card?.id }}</span>
    </div>

    <div class="grid-2">
      <article class="main-col">
        <PageHeader :eyebrow="`Chapter · Card № ${card?.id ?? '…'}`" :title="card?.term ?? '…'" />
        <div class="detail__meta">
          <span class="mono transcription">{{ card?.transcription }}</span>
          <span class="chip" :class="card?.learned ? 'sage' : 'ochre'">
            {{ card?.learned ? 'выучено' : `level ${card?.level}` }}
          </span>
        </div>
        <p class="translation serif">
          {{ card?.translation }}
        </p>
        <p v-if="card?.example" class="example">
          <span class="eyebrow">Sentence</span>
          «{{ card?.example }}»
        </p>

        <div class="detail__actions">
          <button class="btn ochre">Тренировать сейчас</button>
          <button class="btn ghost">Редактировать</button>
          <button class="btn ghost rust-inline">Удалить</button>
        </div>

        <TodoBanner
          text="Редактирование — PUT /api/cards/{id}; удаление — DELETE /api/cards/{id}. Оба уже реализованы в Cards REST, нужно только подключить."
        />
      </article>

      <aside class="side-col">
        <section class="side-block">
          <span class="eyebrow">Progress</span>
          <div class="progress-visual">
            <span
              v-for="i in 10"
              :key="i"
              class="pip"
              :class="{ filled: card && i <= card.level, learned: card?.learned }"
            />
          </div>
          <div class="progress__meta mono">
            level {{ card?.level }} / 10 · {{ card?.learned ? 'learned' : `${10 - (card?.level ?? 0)} шагов до конца` }}
          </div>
        </section>

        <section class="side-block">
          <span class="eyebrow">Numbers</span>
          <dl class="numbers">
            <div>
              <dt>Точность</dt>
              <dd>
                <span v-if="accuracy !== null" class="mono">{{ accuracy }}%</span>
                <span v-else class="mono muted">—</span>
              </dd>
            </div>
            <div>
              <dt>Повторений всего</dt>
              <dd class="mono">{{ card?.totalReviews }}</dd>
            </div>
            <div>
              <dt>Верных</dt>
              <dd class="mono">{{ card?.correctReviews }}</dd>
            </div>
            <div>
              <dt>Добавлена</dt>
              <dd class="mono">
                {{ card ? new Date(card.createdAt).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short', year: 'numeric' }) : '—' }}
              </dd>
            </div>
            <div>
              <dt>Следующий показ</dt>
              <dd class="mono">
                {{
                  card?.nextReviewAt
                    ? new Date(card.nextReviewAt).toLocaleString('ru-RU', {
                        day: 'numeric',
                        month: 'short',
                        hour: '2-digit',
                        minute: '2-digit',
                      })
                    : '—'
                }}
              </dd>
            </div>
          </dl>
        </section>

        <section class="side-block">
          <span class="eyebrow">History</span>
          <ul v-if="history.length" class="history">
            <li v-for="(h, i) in history" :key="i">
              <span class="mono when">
                {{ h.at.toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' }) }}
              </span>
              <span class="chip" :class="h.correct ? 'sage' : 'rust'">
                {{ h.correct ? '✓ верно' : '✗ ошибка' }}
              </span>
              <span class="mono transitions">
                lvl {{ h.levelBefore }} → {{ h.levelAfter }}
              </span>
            </li>
          </ul>
          <p v-else class="muted serif">Ещё ни одного повторения.</p>
          <TodoBanner
            text="История ревью не отдаётся отдельным endpoint'ом — сейчас нужно достать через агрегацию по ReviewEntity. Возможно, добавить GET /api/cards/{id}/reviews."
          />
        </section>
      </aside>
    </div>
  </section>
</template>

<style scoped>
.detail {
  display: flex;
  flex-direction: column;
  gap: 20px;
}
.crumbs {
  display: flex;
  gap: 12px;
  align-items: center;
  font-size: 13px;
  color: var(--ink-mute);
}
.crumbs a {
  border: 0;
}

.grid-2 {
  display: grid;
  grid-template-columns: minmax(0, 1.4fr) 340px;
  gap: 40px;
  align-items: start;
}
@media (max-width: 1000px) {
  .grid-2 {
    grid-template-columns: 1fr;
  }
}

.main-col {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.detail__term {
  font-size: clamp(72px, 8vw, 128px);
  font-variation-settings: 'opsz' 144, 'SOFT' 60;
  color: var(--ink);
  font-weight: 300;
  margin: 8px 0 4px;
  letter-spacing: -0.035em;
  line-height: 0.9;
}
.detail__meta {
  display: flex;
  align-items: center;
  gap: 14px;
  margin-bottom: 8px;
}
.transcription {
  color: var(--ink-mute);
  font-size: 16px;
}
.translation {
  font-family: var(--serif);
  font-size: 28px;
  color: var(--ink);
  font-weight: 400;
  line-height: 1.25;
  max-width: 620px;
  border-top: 1px solid var(--ink);
  padding-top: 20px;
  margin: 12px 0;
}
.example {
  color: var(--ink-soft);
  font-size: 16px;
  max-width: 620px;
  padding-left: 16px;
  border-left: 3px solid var(--ochre);
  line-height: 1.55;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.detail__actions {
  display: flex;
  gap: 10px;
  margin-top: 12px;
}
.btn.rust-inline {
  color: var(--rust);
}
.btn.rust-inline:hover {
  border-color: var(--rust);
}

.side-col {
  display: flex;
  flex-direction: column;
  gap: 20px;
}
.side-block {
  padding: 20px 22px;
  background: var(--paper-elevated);
  border: 1px solid var(--rule);
  border-radius: var(--radius);
}
.side-block .eyebrow {
  display: block;
  margin-bottom: 10px;
}

.progress-visual {
  display: flex;
  gap: 4px;
  margin-bottom: 8px;
}
.pip {
  flex: 1;
  height: 24px;
  background: var(--paper-sunk);
  border-radius: 2px;
}
.pip.filled {
  background: var(--ink);
}
.pip.learned.filled {
  background: var(--sage);
}
.progress__meta {
  font-size: 11px;
  color: var(--ink-mute);
}

.numbers {
  margin: 0;
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 6px 12px;
}
.numbers div {
  display: contents;
}
.numbers dt {
  font-size: 12px;
  color: var(--ink-mute);
}
.numbers dd {
  margin: 0;
  font-size: 13px;
  color: var(--ink);
  text-align: right;
}
.muted {
  color: var(--ink-mute);
}

.history {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.history li {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
  color: var(--ink-soft);
  padding: 6px 0;
  border-bottom: 1px dashed var(--rule);
}
.history li:last-child {
  border-bottom: 0;
}
.when {
  min-width: 68px;
  color: var(--ink-mute);
}
.transitions {
  margin-left: auto;
  color: var(--ink-mute);
}
</style>
