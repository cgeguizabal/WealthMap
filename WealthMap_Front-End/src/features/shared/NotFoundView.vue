<script setup>
import { RouterLink } from 'vue-router'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()
</script>

<template>
  <div class="notfound">
    <h1>404</h1>
    <p>{{ t('common.notFoundPage') }}</p>
    <RouterLink to="/">{{ t('common.goToDashboard') }}</RouterLink>
  </div>
</template>

<style scoped>
.notfound {
  min-height: 100vh;
  display: grid;
  place-content: center;
  text-align: center;
  gap: var(--sp-4);
}
</style>