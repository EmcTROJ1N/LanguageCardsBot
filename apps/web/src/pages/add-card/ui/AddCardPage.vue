<script setup lang="ts">
import { PageHeader } from '@/shared/ui'
import { AddCardForm } from '@/features/add-card'
</script>

<template>
  <section class="add">
    <PageHeader eyebrow="Chapter · Addition" title="Новая карточка">
      Впишите слово или фразу — система подставит перевод и транскрипцию из
      Google Translate, вы отредактируете и сохраните.
    </PageHeader>
    <AddCardForm />
  </section>
</template>

<style scoped>
.add { display: flex; flex-direction: column; gap: 28px; }
</style>
