<script setup>
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

defineProps({
  title: { type: String, required: true },
  subtitle: { type: String, default: '' }
})
</script>

<template>
  <main class="auth">
    <motion.div
      class="auth__panel"
      v-bind="fadeUp()"
    >
      <header class="auth__brand">
        <span class="auth__mark">WM</span>
        <span class="auth__wordmark">WealthMap</span>
      </header>

      <div class="auth__heading">
        <h1 class="auth__title">{{ title }}</h1>
        <p v-if="subtitle" class="auth__subtitle">{{ subtitle }}</p>
      </div>

      <slot />

      <footer v-if="$slots.footer" class="auth__footer">
        <slot name="footer" />
      </footer>
    </motion.div>

    <p class="auth__legal">{{ t('auth.brandTagline') }}</p>
  </main>
</template>

<style scoped lang="scss" src="./AuthShell.scss"></style>
