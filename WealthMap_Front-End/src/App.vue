<script setup>
import { computed } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import AppShell from '@/components/layout/AppShell.vue'
import BaseToast from '@/components/base/BaseToast.vue'
import BaseConfirmDialog from '@/components/base/BaseConfirmDialog.vue'
import BaseErrorBoundary from '@/components/base/BaseErrorBoundary.vue'

const route = useRoute()

/**
 * Auth screens opt out of the shell with `meta.layout: 'blank'`. Chrome is the
 * default because most routes are inside the app; forgetting the flag on a new
 * route gives you navigation, not a page stranded without it.
 */
const useShell = computed(() => route.meta.layout !== 'blank')
</script>

<template>
  <a class="skip-link" href="#main-content">Skip to content</a>

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

<style lang="scss">
.page-enter-active, .page-leave-active {
  transition: opacity var(--dur) var(--ease), transform var(--dur) var(--ease);
}

.page-enter-from { opacity: 0; transform: translateY(6px); }
.page-leave-to { opacity: 0; }

@media (prefers-reduced-motion: reduce) {
  .page-enter-active, .page-leave-active { transition: none; }
}
</style>
