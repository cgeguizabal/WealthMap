<script setup>
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'

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

    <p class="auth__legal">Your money, mapped. Figures are computed, never guessed.</p>
  </main>
</template>

<style scoped lang="scss">
.auth {
  min-height: 100dvh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--sp-5);
  padding: var(--sp-6) var(--sp-4);
  background: var(--canvas);
}

.auth__panel {
  width: 100%;
  max-width: 420px;
  padding: var(--sp-8);

  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow-lg);
}

.auth__brand {
  display: flex;
  align-items: center;
  gap: var(--sp-3);
  margin-bottom: var(--sp-8);
}

.auth__mark {
  display: grid;
  place-items: center;
  width: 30px;
  height: 30px;

  background: var(--ink);
  color: var(--canvas);
  border-radius: var(--radius-sm);

  font-size: var(--fs-xs);
  font-weight: var(--fw-bold);
  letter-spacing: 0.02em;
}

.auth__wordmark {
  font-size: var(--fs-md);
  font-weight: var(--fw-semibold);
  letter-spacing: -0.01em;
}

.auth__heading { margin-bottom: var(--sp-6); }

.auth__title {
  font-size: var(--fs-xl);
  font-weight: var(--fw-semibold);
  letter-spacing: -0.02em;
}

.auth__subtitle {
  margin-top: var(--sp-1);
  font-size: var(--fs-sm);
  color: var(--text-muted);
}

.auth__footer {
  margin-top: var(--sp-6);
  padding-top: var(--sp-5);
  border-top: var(--border-subtle);
  font-size: var(--fs-sm);
  color: var(--text-muted);
  text-align: center;
}

.auth__legal {
  font-size: var(--fs-xs);
  color: var(--text-subtle);
}

@media (max-width: 480px) {
  .auth__panel { padding: var(--sp-5); box-shadow: var(--shadow); }
}
</style>
