<script setup lang="ts">
import { PageHeader } from '@/shared/ui'
import { ImportDropzone } from '@/features/import-cards'
</script>

<template>
  <section class="io">
    <PageHeader eyebrow="Chapter · Correspondence" title="Импорт / Экспорт">
      Загружайте колоду из JSON или скачивайте её целиком — на случай смены устройства, бэкапа или обмена с друзьями.
    </PageHeader>
    <ImportDropzone />
  </section>
</template>

<style scoped>
.io { display: flex; flex-direction: column; gap: 24px; }
</style>
