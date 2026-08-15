<script setup lang="ts">
import { ref, computed } from 'vue'
import TodoBanner from '@/shared/ui/TodoBanner.vue'

const term = ref('')
const translation = ref('')
const transcription = ref('')
const example = ref('')
const autoTranslated = ref(false)

function autoFill() {
  // Заглушка автоперевода. Реальный вызов — POST /api/cards/translation.
  if (!term.value.trim()) return
  autoTranslated.value = true
  const t = term.value.toLowerCase().trim()
  const seed: Record<string, { tr: string; ipa: string; ex?: string }> = {
    lambent: { tr: 'мерцающий, играющий (о свете)', ipa: '/ˈlambənt/', ex: 'Lambent flames danced on the ceiling.' },
    obfuscate: { tr: 'затуманивать, запутывать', ipa: '/ˈɒbfʌskeɪt/', ex: 'The report was written to obfuscate rather than inform.' },
  }
  const guess = seed[t] ?? { tr: '(автоперевод сюда)', ipa: '/ˈautoːgen/', ex: 'A sentence hint would appear here.' }
  translation.value = guess.tr
  transcription.value = guess.ipa
  example.value = guess.ex ?? ''
}

const canSave = computed(() => term.value.trim() && translation.value.trim())
</script>

<template>
  <section class="add">
    <header class="head">
      <div>
        <span class="eyebrow">Chapter · Addition</span>
        <h1 class="display">
          Новая карточка
        </h1>
        <p class="lede serif">
          Впишите слово или фразу — система подставит перевод и транскрипцию из
          Google Translate, вы отредактируете и сохраните.
        </p>
      </div>
    </header>

    <div class="grid-2">
      <form class="form" @submit.prevent>
        <div class="field big">
          <label>Слово или фраза</label>
          <input
            v-model="term"
            type="text"
            placeholder="e.g. petrichor"
            @blur="autoFill"
          />
          <span class="hint mono">
            подсказка · автоперевод срабатывает при потере фокуса
          </span>
        </div>

        <div class="field">
          <label>Перевод</label>
          <textarea v-model="translation" rows="2" placeholder="запах земли после дождя"></textarea>
        </div>

        <div class="grid-2 sub">
          <div class="field">
            <label>Транскрипция</label>
            <input v-model="transcription" type="text" placeholder="/ˈpɛtrɪkɔːr/" />
          </div>
          <div class="field">
            <label>Начальный level</label>
            <select disabled>
              <option>1 — новая</option>
            </select>
            <span class="hint mono">новые карточки всегда стартуют с level 1</span>
          </div>
        </div>

        <div class="field">
          <label>Пример (опц.)</label>
          <textarea v-model="example" rows="3" placeholder="Predicted sentence with the word in context."></textarea>
        </div>

        <div class="form__actions">
          <button class="btn ochre lg" :disabled="!canSave">Сохранить карточку</button>
          <button type="button" class="btn ghost lg">Сохранить и добавить ещё</button>
        </div>
      </form>

      <aside class="preview">
        <span class="eyebrow">Предпросмотр</span>
        <div class="proof">
          <div class="proof__stamp mono">CARD № —</div>
          <h2 class="proof__term">{{ term || 'ваше слово' }}</h2>
          <span class="mono proof__trans">{{ transcription || '/ipa/' }}</span>
          <p class="proof__translation serif">
            {{ translation || 'перевод отобразится здесь' }}
          </p>
          <p v-if="example" class="proof__example">
            «{{ example }}»
          </p>
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
  </section>
</template>

<style scoped>
.add {
  display: flex;
  flex-direction: column;
  gap: 28px;
}
.lede {
  color: var(--ink-soft);
  font-size: 17px;
  max-width: 520px;
}

.grid-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 40px;
  align-items: start;
}
.grid-2.sub {
  gap: 20px;
}
@media (max-width: 900px) {
  .grid-2 {
    grid-template-columns: 1fr;
    gap: 24px;
  }
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
.field {
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.field.big input {
  font-family: var(--serif);
  font-size: 28px;
  padding: 12px 14px;
}
.field label {
  font-size: 11px;
  letter-spacing: 0.14em;
  text-transform: uppercase;
  color: var(--ink-mute);
  font-weight: 600;
}
input,
textarea,
select {
  font-family: var(--sans);
  font-size: 15px;
  color: var(--ink);
  background: var(--paper);
  border: 1px solid var(--rule);
  border-radius: var(--radius-sm);
  padding: 10px 12px;
  outline: none;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}
select:disabled {
  color: var(--ink-mute);
  background: var(--paper-sunk);
}
input:focus,
textarea:focus,
select:focus {
  border-color: var(--ink);
  box-shadow: 0 0 0 3px rgba(55, 118, 126, 0.14);
}
textarea {
  resize: vertical;
  font-family: var(--sans);
}
.hint {
  color: var(--ink-mute);
  font-size: 11px;
}
.form__actions {
  display: flex;
  gap: 12px;
  padding-top: 8px;
  border-top: 1px dashed var(--rule);
}
.btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.preview {
  display: flex;
  flex-direction: column;
  gap: 16px;
}
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
  color: var(--ink);
  line-height: 1.05;
  letter-spacing: -0.02em;
  font-variation-settings: 'opsz' 144;
}
.proof__trans {
  color: var(--ink-mute);
  font-size: 14px;
}
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
