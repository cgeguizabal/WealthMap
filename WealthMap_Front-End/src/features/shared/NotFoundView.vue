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

<style scoped src="./NotFoundView.scss"></style>