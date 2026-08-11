<script setup>
import { computed } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import AppShell from '@/components/layout/AppShell.vue'
import BaseToast from '@/components/base/BaseToast.vue'
import BaseConfirmDialog from '@/components/base/BaseConfirmDialog.vue'
import BaseErrorBoundary from '@/components/base/BaseErrorBoundary.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const route = useRoute()

/**
 * Auth screens opt out of the shell with `meta.layout: 'blank'`. Chrome is the
 * default because most routes are inside the app; forgetting the flag on a new
 * route gives you navigation, not a page stranded without it.
 */
const useShell = computed(() => route.meta.layout !== 'blank')
</script>

<template>
  <a class="skip-link" href="#main-content">{{ t('nav.skipToContent') }}</a>

  <AppShell v-if="useShell">
    <BaseErrorBoundary>
      <RouterView v-slot="{ Component }">
        <Transition name="page" mode="out-in">
          <component :is="Component" />
        </Transition>
      </RouterView>
    </BaseErrorBoundary>
  </AppShell>

  <RouterView v-else />

  <!-- Global singletons: any component raises these through the UI store -->
  <BaseToast />
  <BaseConfirmDialog />
</template>

<style lang="scss" src="@/assets/styles/base/App.scss"></style>
