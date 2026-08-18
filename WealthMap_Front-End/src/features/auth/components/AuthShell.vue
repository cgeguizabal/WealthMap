<script setup>
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { RouterLink } from 'vue-router'
import { useI18n } from '@/composables/useI18n'
import { LEGAL_ROUTES } from '@/config/legal'
import WealthMapIcon from '@/components/brand/WealthMapIcon.vue'
import WealthMapLogo from '@/components/brand/WealthMapLogo.vue'
import { VERSION_LABEL } from '@/config/app'

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
      <!--
        The tile replaces the WM lettermark here and only here. This is the one
        screen with room for the actual application icon, and the one where a
        person is deciding whether to trust the thing in front of them.
      -->
      <!--
        Two columns, two rows. The icon and the wordmark share the first row and
        centre on each other; the version sits in the second, under the icon,
        because it describes the application rather than the brand.
      -->
      <header class="auth__brand">
        <WealthMapIcon class="auth__icon" :size="64" />
        <WealthMapLogo class="auth__logo" :width="196" />
        <span class="auth__version">{{ VERSION_LABEL }}</span>
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

    <!-- Reachable before signing up, not only from inside the app. Someone
         deciding whether to create an account is exactly who needs to read
         these. -->
    <nav class="auth__links">
      <RouterLink :to="LEGAL_ROUTES.privacy">{{ t('legal.privacy') }}</RouterLink>
      <span aria-hidden="true">·</span>
      <RouterLink :to="LEGAL_ROUTES.terms">{{ t('legal.terms') }}</RouterLink>
    </nav>
  </main>
</template>

<style scoped lang="scss" src="@/assets/styles/features/auth/AuthShell.scss"></style>
