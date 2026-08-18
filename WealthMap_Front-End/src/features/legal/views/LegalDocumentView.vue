<script setup>
import { computed } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { useI18n } from '@/composables/useI18n'
import { renderMarkdown } from '../renderMarkdown'
import { LEGAL_ROUTES } from '@/config/legal'
import { VERSION_LABEL } from '@/config/app'
import WealthMapIcon from '@/components/brand/WealthMapIcon.vue'
import WealthMapLogo from '@/components/brand/WealthMapLogo.vue'

// The canonical text, imported from docs/legal as raw strings — four documents,
// two per language. All are pulled in rather than fetched per route: together
// they are a few kilobytes, and a dynamic import keyed on route and locale would
// earn nothing but a loading state on a page made entirely of text.
import privacyEn from '../../../../../docs/legal/PRIVACY_POLICY.md?raw'
import privacyEs from '../../../../../docs/legal/PRIVACY_POLICY.es.md?raw'
import termsEn from '../../../../../docs/legal/TERMS_OF_SERVICE.md?raw'
import termsEs from '../../../../../docs/legal/TERMS_OF_SERVICE.es.md?raw'

const { t, locale } = useI18n()
const route = useRoute()

const isPrivacy = computed(() => route.path === LEGAL_ROUTES.privacy)

/**
 * Falls back to English for any locale without its own translation.
 *
 * A missing translation must never mean a missing policy — an empty privacy
 * page is worse than one in the wrong language, and these are the two documents
 * where showing nothing is not an option.
 */
const source = computed(() => {
  const spanish = locale.value?.startsWith('es')

  if (isPrivacy.value) return spanish ? privacyEs : privacyEn

  return spanish ? termsEs : termsEn
})

const body = computed(() => renderMarkdown(source.value))
</script>

<template>
  <motion.div class="legal" v-bind="fadeUp()">
    <div class="legal__nav">
      <RouterLink to="/">{{ t('legal.backToApp') }}</RouterLink>

      <RouterLink :to="isPrivacy ? LEGAL_ROUTES.terms : LEGAL_ROUTES.privacy">
        {{ isPrivacy ? t('legal.terms') : t('legal.privacy') }}
      </RouterLink>
    </div>

    <!--
      The full brand lockup, and the one place in the app with room for the
      tagline to be legible. These two documents are read by someone deciding
      whether to trust the thing, often before they have an account — so they
      should say plainly whose documents they are.
    -->
    <header class="legal__brand">
      <WealthMapIcon :size="56" />
      <WealthMapLogo :width="240" />
    </header>

    <p class="legal__beta" role="note">
      <strong>{{ VERSION_LABEL }}</strong>
      {{ t('legal.betaNotice') }}
    </p>

    <!--
      eslint-disable-next-line vue/no-v-html — the input is four files in this
      repository, and renderMarkdown escapes before it adds any markup.
    -->
    <article class="legal-doc" v-html="body"></article>
  </motion.div>
</template>

<style scoped src="@/assets/styles/features/legal/LegalDocumentView.scss"></style>
