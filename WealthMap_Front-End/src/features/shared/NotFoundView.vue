<script setup>
import { RouterLink } from 'vue-router'
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()
</script>

<template>
  <motion.div class="notfound" v-bind="fadeUp()">
    <h1>404</h1>
    <p>{{ t('common.notFoundPage') }}</p>
    <RouterLink to="/">{{ t('common.goToDashboard') }}</RouterLink>
  </motion.div>
</template>

<style scoped src="@/assets/styles/features/shared/NotFoundView.scss"></style>