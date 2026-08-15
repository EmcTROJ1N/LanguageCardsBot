<script setup lang="ts">
import { ref } from 'vue'
import TodoBanner from '@/shared/ui/TodoBanner.vue'

const dragging = ref(false)
const fileName = ref<string | null>(null)

function onDrop(e: DragEvent) {
  dragging.value = false
  const f = e.dataTransfer?.files?.[0]
  if (f) fileName.value = f.name
}
</script>

<template>
  <section class="io">
    <header>
      <span class="eyebrow">Chapter · Correspondence</span>
      <h1 class="display">Импорт / Экспорт</h1>
      <p class="lede serif">
        Загружайте колоду из JSON или скачивайте её целиком — на случай смены устройства, бэкапа или обмена с друзьями.
      </p>
    </header>

    <div class="grid-2">
      <article class="panel">
        <span class="eyebrow">Импорт</span>
        <h2>Загрузите JSON</h2>
        <div
          class="dropzone"
          :class="{ dragging }"
          @dragover.prevent="dragging = true"
          @dragleave="dragging = false"
          @drop.prevent="onDrop"
        >
          <span class="dropzone__mark serif">JSON</span>
          <p v-if="!fileName" class="dropzone__hint">
            Перетащите файл сюда или <span class="link">выберите вручную</span>
          </p>
          <p v-else class="dropzone__hint">
            <span class="mono">{{ fileName }}</span>
          </p>
          <span class="mono muted">поддерживается формат экспорта из бота (/export)</span>
        </div>
        <div class="row-actions">
          <button class="btn ochre" :disabled="!fileName">Импортировать</button>
        </div>
        <TodoBanner
          text="Реальный вызов: POST /api/cards/import/json — уже есть, ожидает { userId, cards[] }. Клиент должен обрабатывать чанки при большой колоде."
        />
      </article>

      <article class="panel">
        <span class="eyebrow">Экспорт</span>
        <h2>Скачайте всю колоду</h2>
        <div class="export-visual">
          <div class="export-visual__stack">
            <div v-for="i in 4" :key="i" class="export-visual__paper" />
          </div>
          <div class="export-visual__meta">
            <span class="mono muted">213 карточек · ~ 34 KB</span>
            <span class="serif">
              JSON, совместимый с /import — сохраните и восстановите в любой момент.
            </span>
          </div>
        </div>
        <div class="row-actions">
          <button class="btn ghost">Скачать .json</button>
          <button class="btn ghost">Скачать .csv</button>
        </div>
        <TodoBanner
          text="Endpoint экспорта отсутствует. Нужен GET /api/cards/export?userId=…&format=json|csv — реализовать в CardsController."
        />
      </article>
    </div>
  </section>
</template>

<style scoped>
.io {
  display: flex;
  flex-direction: column;
  gap: 24px;
}
.lede {
  color: var(--ink-soft);
  font-size: 17px;
  max-width: 640px;
}
.grid-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
}
@media (max-width: 900px) {
  .grid-2 {
    grid-template-columns: 1fr;
  }
}
.panel {
  background: var(--paper-elevated);
  border: 1px solid var(--rule);
  border-radius: var(--radius);
  padding: 22px 24px;
  display: flex;
  flex-direction: column;
  gap: 14px;
}
.panel h2 {
  margin: 0;
  font-size: 22px;
}

.dropzone {
  padding: 32px;
  border: 2px dashed var(--rule);
  border-radius: var(--radius);
  background: var(--paper);
  text-align: center;
  display: flex;
  flex-direction: column;
  gap: 12px;
  align-items: center;
  min-height: 220px;
  justify-content: center;
  transition: border-color 0.15s ease, background 0.15s ease;
}
.dropzone.dragging {
  border-color: var(--ochre);
  background: rgba(55, 118, 126, 0.08);
}
.dropzone__mark {
  font-family: var(--serif);
  font-size: 60px;
  color: var(--ink-mute);
  letter-spacing: 0.02em;
  font-weight: 300;
  font-variation-settings: 'opsz' 144;
}
.dropzone__hint {
  margin: 0;
  color: var(--ink);
  font-size: 15px;
}
.dropzone .link {
  color: var(--ochre);
  border-bottom: 1px solid var(--ochre);
  cursor: pointer;
}
.muted {
  color: var(--ink-mute);
}

.export-visual {
  display: flex;
  align-items: center;
  gap: 20px;
  padding: 24px;
  background: var(--paper);
  border: 1px solid var(--rule);
  border-radius: var(--radius);
  min-height: 220px;
}
.export-visual__stack {
  position: relative;
  width: 130px;
  height: 160px;
  flex-shrink: 0;
}
.export-visual__paper {
  position: absolute;
  inset: 0;
  background: var(--paper-elevated);
  border: 1px solid var(--rule);
  border-radius: 4px;
  box-shadow: var(--shadow-paper);
}
.export-visual__paper:nth-child(1) {
  transform: rotate(-4deg) translateY(-4px);
  z-index: 1;
}
.export-visual__paper:nth-child(2) {
  transform: rotate(-1deg) translateY(-2px);
  z-index: 2;
}
.export-visual__paper:nth-child(3) {
  transform: rotate(2deg);
  z-index: 3;
}
.export-visual__paper:nth-child(4) {
  transform: rotate(6deg) translateY(2px);
  background: var(--ochre);
  border-color: var(--ochre);
  z-index: 4;
}
.export-visual__meta {
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-size: 15px;
  color: var(--ink);
}

.row-actions {
  display: flex;
  gap: 10px;
  padding-top: 12px;
  border-top: 1px solid var(--rule);
  margin-top: auto;
}
.btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}
</style>
