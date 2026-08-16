<script setup lang="ts">
import { useAddCard } from '../model/useAddCard'
import { AppField, AppBtn, TodoBanner } from '@/shared/ui'

const { term, translation, transcription, example, autoTranslated, canSave, saving, autoTranslate, save } = useAddCard()
</script>

<template>
  <div class="grid-2">
    <form class="form" @submit.prevent="save">
      <AppField label="Слово или фраза" hint="автоперевод срабатывает при потере фокуса">
        <input
          v-model="term"
          type="text"
          placeholder="e.g. petrichor"
          class="big-input"
          @blur="autoTranslate"
        />
      </AppField>

      <AppField label="Перевод">
        <textarea v-model="translation" rows="2" placeholder="запах земли после дождя" />
      </AppField>

      <div class="grid-2 sub">
        <AppField label="Транскрипция">
          <input v-model="transcription" type="text" placeholder="/ˈpɛtrɪkɔːr/" />
        </AppField>
        <AppField label="Начальный level" hint="новые карточки всегда стартуют с level 1">
          <select disabled>
            <option>1 — новая</option>
          </select>
        </AppField>
      </div>

      <AppField label="Пример (опц.)">
        <textarea v-model="example" rows="3" placeholder="Predicted sentence with the word in context." />
      </AppField>

      <div class="form__actions">
        <AppBtn variant="ochre" size="lg" type="submit" :disabled="!canSave || saving">
          Сохранить карточку
        </AppBtn>
        <AppBtn variant="ghost" size="lg" type="button">Сохранить и добавить ещё</AppBtn>
      </div>
    </form>

    <aside class="preview">
      <span class="eyebrow">Предпросмотр</span>
      <div class="proof">
        <div class="proof__stamp mono">CARD № —</div>
        <h2 class="proof__term">{{ term || 'ваше слово' }}</h2>
        <span class="mono proof__trans">{{ transcription || '/ipa/' }}</span>
        <p class="proof__translation serif">{{ translation || 'перевод отобразится здесь' }}</p>
        <p v-if="example" class="proof__example">«{{ example }}»</p>
        <div class="proof__foot">
          <span class="chip ochre">lvl 1 · новая</span>
          <span class="mono">до первого повтора — 1 день</span>
        </div>
      </div>
      <TodoBanner
        v-if="autoTranslated"
        text="Автоперевод сейчас работает от заглушки. При интеграции — POST /api/cards/translation с source=auto, target=ru."
      />
      <TodoBanner
        v-else
        text="При интеграции с бэком: POST /api/cards с userId (см. Passport ↔ Cards link)."
      />
    </aside>
  </div>
</template>

<style scoped>
.grid-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 40px;
  align-items: start;
}
.grid-2.sub { gap: 20px; }
@media (max-width: 900px) {
  .grid-2 { grid-template-columns: 1fr; gap: 24px; }
}
.form {
  display: flex;
  flex-direction: column;
  gap: 20px;
  background: var(--paper-elevated);
  border: 1px solid var(--rule);
  border-radius: var(--radius);
  padding: 24px;
}
.big-input {
  font-family: var(--serif);
  font-size: 28px;
  padding: 12px 14px;
}
.form__actions {
  display: flex;
  gap: 12px;
  padding-top: 8px;
  border-top: 1px dashed var(--rule);
}
.preview { display: flex; flex-direction: column; gap: 16px; }
.proof {
  padding: 32px;
  background: var(--paper);
  border: 1px solid var(--ink);
  border-radius: var(--radius);
  box-shadow: var(--shadow-paper);
  position: relative;
  min-height: 300px;
}
.proof__stamp {
  position: absolute;
  top: 12px;
  right: 16px;
  font-size: 10px;
  color: var(--ink-mute);
}
.proof__term {
  font-family: var(--serif);
  font-size: 44px;
  font-weight: 400;
  margin: 0 0 6px;
  line-height: 1.05;
  letter-spacing: -0.02em;
  font-variation-settings: 'opsz' 144;
}
.proof__trans { color: var(--ink-mute); font-size: 14px; }
.proof__translation {
  font-family: var(--serif);
  color: var(--ink-soft);
  font-size: 22px;
  line-height: 1.35;
  margin: 20px 0 12px;
  border-top: 1px dashed var(--rule);
  padding-top: 20px;
}
.proof__example {
  font-size: 14px;
  color: var(--ink-mute);
  border-left: 3px solid var(--ochre);
  padding-left: 12px;
  margin: 0 0 20px;
}
.proof__foot {
  display: flex;
  align-items: center;
  gap: 12px;
  padding-top: 16px;
  border-top: 1px solid var(--rule);
  font-size: 12px;
  color: var(--ink-mute);
}
</style>
