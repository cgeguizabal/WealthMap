<script setup>
import { computed } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { useI18n } from '@/composables/useI18n'
import { renderMarkdown } from '../renderMarkdown'
import { LEGAL_DOCS_ARE_DRAFT, LEGAL_ROUTES } from '@/config/legal'

// The canonical text, imported from docs/legal as raw strings. Both are pulled
// in rather than loaded per route: together they are a few kilobytes, and a
// dynamic import keyed on the route would earn nothing but a loading state.
import privacySource from '../../../../../docs/legal/PRIVACY_POLICY.md?raw'
import termsSource from '../../../../../docs/legal/TERMS_OF_SERVICE.md?raw'

const { t } = useI18n()
const route = useRoute()

const isPrivacy = computed(() => route.path === LEGAL_ROUTES.privacy)

const body = computed(() => renderMarkdown(isPrivacy.value ? privacySource : termsSource))
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
      The same warning the markdown carries as an HTML comment, said out loud.
      A draft policy that looks finished is worse than no policy: someone reads
      it as a commitment that nobody has checked.
    -->
    <p v-if="LEGAL_DOCS_ARE_DRAFT" class="legal__draft" role="note">
      <strong>{{ t('legal.draftLabel') }}</strong>
      {{ t('legal.draftNotice') }}
    </p>

    <!--
      eslint-disable-next-line vue/no-v-html — the input is two files in this
      repository, and renderMarkdown escapes before it adds any markup.
    -->
    <article class="legal-doc" v-html="body"></article>
  </motion.div>
</template>

<style scoped src="@/assets/styles/features/legal/LegalDocumentView.scss"></style>
