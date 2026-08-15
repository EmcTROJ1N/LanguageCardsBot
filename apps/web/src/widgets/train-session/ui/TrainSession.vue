<script setup lang="ts">
import { computed, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { getDueCards, reviewIntervalDays } from '@/entities/card'
import TodoBanner from '@/shared/ui/TodoBanner.vue'

const queue = ref(getDueCards())
const flipped = ref(false)
const results = ref<{ id: number; correct: boolean }[]>([])

const current = computed(() => queue.value[0])
const progress = computed(() => {
  const total = queue.value.length + results.value.length
  return `${results.value.length + 1} / ${total}`
})

const correctCount = computed(() => results.value.filter((r) => r.correct).length)

function flip() {
  flipped.value = true
}

function answer(correct: boolean) {
  if (!current.value) return
  results.value.push({ id: current.value.id, correct })
  flipped.value = false
  queue.value.shift()
}

function restart() {
  queue.value = getDueCards()
  results.value = []
  flipped.value = false
}

const nextIntervalDays = computed(() => {
  if (!current.value) return null
  const nextLevel = Math.min(current.value.level + 1, 10)
  return reviewIntervalDays[nextLevel - 1]
})
</script>

<template>
  <section class="train">
    <header class="head">
      <div>
        <span class="eyebrow">Session · Repetitio</span>
        <h1 class="display">Тренировка</h1>
      </div>
      <div v-if="current" class="progress">
        <span class="mono progress__num">{{ progress }}</span>
        <span class="progress__hint serif">
          {{ correctCount }} верно · {{ results.length - correctCount }} с ошибкой
        </span>
      </div>
    </header>

    <div v-if="current" class="stage">
      <div class="stage__margin left">
        <span class="mono margin__label">card</span>
        <span class="mono margin__num">№ {{ current.id }}</span>
        <span class="mono margin__hint">
          streak · <span>{{ current.correctReviews }}/{{ current.totalReviews }}</span>
        </span>
      </div>

      <div class="card" :class="{ flipped }" @click="flip">
        <div class="card__face front">
          <div class="face__inner">
            <span class="eyebrow">Term</span>
            <h2 class="face__term">{{ current.term }}</h2>
            <span class="mono face__trans">{{ current.transcription }}</span>
            <span v-if="current.example" class="face__example serif">
              «{{ current.example }}»
            </span>
            <div class="face__hint">
              <span class="mono">tap · space</span>
              <span>показать перевод</span>
            </div>
          </div>
        </div>
        <div class="card__face back">
          <div class="face__inner">
            <span class="eyebrow">Translation</span>
            <h2 class="face__translation serif">{{ current.translation }}</h2>
            <span class="mono face__trans">{{ current.transcription }}</span>
            <div class="answer-hint">
              <span>верный ответ?</span>
              <span class="mono">
                lvl {{ current.level }} → lvl {{ Math.min(current.level + 1, 10) }} · +{{ nextIntervalDays }} дн.
              </span>
            </div>
          </div>
        </div>
      </div>

      <div class="stage__margin right">
        <span class="mono margin__label">progress</span>
        <span class="mono margin__num">{{ progress }}</span>
        <span class="mono margin__hint">daily · {{ correctCount }}✓</span>
      </div>
    </div>

    <div v-if="current" class="actions">
      <button class="btn rust lg" :disabled="!flipped" @click="answer(false)">
        Не помню
        <span class="mono">1</span>
      </button>
      <button class="btn sage lg" :disabled="!flipped" @click="answer(true)">
        Помню
        <span class="mono">2</span>
      </button>
      <button v-if="!flipped" class="btn ghost lg" @click="flip">
        Показать
        <span class="mono">space</span>
      </button>
    </div>

    <div v-else class="finish">
      <span class="eyebrow">Coda</span>
      <h2 class="display finish__title">
        Готово. <em class="serif">
          {{ correctCount }}/{{ results.length }} верно.
        </em>
      </h2>
      <p class="finish__lede">
        Возвращайтесь через настроенный интервал напоминаний — карточки уже
        поставлены на новый уровень.
      </p>
      <div class="finish__cta">
        <button class="btn ochre lg" @click="restart">Ещё круг</button>
        <RouterLink to="/" class="btn ghost lg">В кабинет</RouterLink>
      </div>
    </div>

    <TodoBanner
      text="Быстрые клавиши 1 / 2 / space пока только визуально подсказаны — привязку keydown-обработчиков делаем при интеграции. Кнопка &laquo;Ещё круг&raquo; перезаписывает очередь из мока."
    />
  </section>
</template>

<style scoped>
.train {
  display: flex;
  flex-direction: column;
  gap: 32px;
  min-height: 60vh;
}
.head {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
}
.progress {
  text-align: right;
  display: flex;
  flex-direction: column;
  gap: 2px;
}
.progress__num {
  font-size: 16px;
  color: var(--ink);
}
.progress__hint {
  font-size: 13px;
  color: var(--ink-mute);
}

.stage {
  display: grid;
  grid-template-columns: 100px 1fr 100px;
  align-items: center;
  min-height: 340px;
  perspective: 1400px;
  gap: 24px;
}
.stage__margin {
  display: flex;
  flex-direction: column;
  gap: 8px;
  font-size: 10.5px;
  color: var(--ink-mute);
}
.stage__margin.right {
  text-align: right;
  align-items: flex-end;
}
.margin__label {
  text-transform: uppercase;
  letter-spacing: 0.14em;
}
.margin__num {
  font-family: var(--serif);
  font-size: 28px;
  color: var(--ink);
  line-height: 1;
}
.margin__hint {
  font-size: 11px;
}

.card {
  position: relative;
  min-height: 340px;
  border-radius: var(--radius-lg);
  transform-style: preserve-3d;
  transition: transform 0.6s cubic-bezier(0.2, 0.7, 0.2, 1);
  cursor: pointer;
  box-shadow: var(--shadow-paper);
}
.card.flipped {
  transform: rotateY(180deg);
}
.card__face {
  position: absolute;
  inset: 0;
  padding: 48px 56px;
  background: var(--paper-elevated);
  border: 1px solid var(--rule);
  border-radius: var(--radius-lg);
  backface-visibility: hidden;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
}
.card__face.back {
  transform: rotateY(180deg);
  background: var(--ink);
  color: var(--paper);
}
.card__face.back .eyebrow,
.card__face.back .mono,
.card__face.back .answer-hint span {
  color: var(--ochre-deep);
}
.card__face::before,
.card__face::after {
  content: '';
  position: absolute;
  left: 40px;
  right: 40px;
  border-top: 1px solid var(--rule);
}
.card__face::before {
  top: 20px;
}
.card__face::after {
  bottom: 20px;
}
.card__face.back::before,
.card__face.back::after {
  border-color: rgba(90, 160, 168, 0.35);
}

.face__inner {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  text-align: center;
  max-width: 540px;
}
.face__term {
  font-family: var(--serif);
  font-size: clamp(52px, 5vw, 82px);
  font-weight: 400;
  font-variation-settings: 'opsz' 144, 'SOFT' 40;
  line-height: 1;
  margin: 0;
  color: var(--ink);
  letter-spacing: -0.03em;
}
.face__translation {
  font-family: var(--serif);
  font-size: clamp(38px, 3.8vw, 56px);
  font-weight: 400;
  color: var(--paper);
  line-height: 1.15;
  margin: 0;
}
.face__trans {
  font-size: 15px;
  color: var(--ink-mute);
}
.card__face.back .face__trans {
  color: var(--ochre-deep);
}
.face__example {
  font-size: 15px;
  color: var(--ink-soft);
  line-height: 1.4;
  max-width: 460px;
  border-top: 1px dashed var(--rule);
  padding-top: 12px;
  margin-top: 4px;
}
.face__hint {
  margin-top: 12px;
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 12px;
  color: var(--ink-mute);
}
.answer-hint {
  margin-top: 12px;
  display: flex;
  flex-direction: column;
  gap: 4px;
  font-size: 13px;
  color: var(--ochre-deep);
}

.actions {
  display: flex;
  gap: 16px;
  justify-content: center;
}
.actions .btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
  transform: none;
}
.actions .btn .mono {
  font-size: 11px;
  padding: 2px 6px;
  background: rgba(244, 239, 228, 0.2);
  border-radius: 3px;
}

.finish {
  display: flex;
  flex-direction: column;
  gap: 12px;
  align-items: flex-start;
  max-width: 640px;
}
.finish__title {
  font-size: clamp(46px, 5vw, 66px);
}
.finish__title em {
  color: var(--sage);
}
.finish__lede {
  font-family: var(--serif);
  color: var(--ink-soft);
  font-size: 18px;
}
.finish__cta {
  display: flex;
  gap: 12px;
  margin-top: 12px;
}
</style>
