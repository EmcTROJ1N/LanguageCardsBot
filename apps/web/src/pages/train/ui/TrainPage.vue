<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { useTrainSession } from '@/features/train-session'
import { TrainCard } from '@/features/train-session'
import { TodoBanner } from '@/shared/ui'

const { current, isFinished, correctCount, results, progress, restart } = useTrainSession()
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

    <TrainCard />

    <div v-if="isFinished" class="finish">
      <span class="eyebrow">Coda</span>
      <h2 class="display finish__title">
        Готово. <em class="serif">{{ correctCount }}/{{ results.length }} верно.</em>
      </h2>
      <p class="finish__lede">
        Возвращайтесь через настроенный интервал напоминаний — карточки уже поставлены на новый уровень.
      </p>
      <div class="finish__cta">
        <button class="btn ochre lg" @click="restart">Ещё круг</button>
        <RouterLink to="/" class="btn ghost lg">В кабинет</RouterLink>
      </div>
    </div>

    <TodoBanner
      text="Быстрые клавиши 1 / 2 / space пока только визуально подсказаны — привязку keydown-обработчиков делаем при интеграции."
    />
  </section>
</template>

<style scoped>
.train { display: flex; flex-direction: column; gap: 32px; min-height: 60vh; }
.head { display: flex; justify-content: space-between; align-items: flex-end; }
.progress { text-align: right; display: flex; flex-direction: column; gap: 2px; }
.progress__num { font-size: 16px; color: var(--ink); }
.progress__hint { font-size: 13px; color: var(--ink-mute); }
.finish { display: flex; flex-direction: column; gap: 12px; align-items: flex-start; max-width: 640px; }
.finish__title { font-size: clamp(46px, 5vw, 66px); }
.finish__title em { color: var(--sage); }
.finish__lede { font-family: var(--serif); color: var(--ink-soft); font-size: 18px; }
.finish__cta { display: flex; gap: 12px; margin-top: 12px; }
</style>
