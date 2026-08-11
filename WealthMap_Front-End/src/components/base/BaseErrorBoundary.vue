<script setup>
import { ref, onErrorCaptured, watch } from 'vue'
import { useRoute } from 'vue-router'
import BaseEmptyState from './BaseEmptyState.vue'
import BaseButton from './BaseButton.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

/**
 * Catches render and lifecycle errors from the routed view. Without this a
 * single component throwing leaves the user staring at a blank page with the
 * reason only in the console.
 *
 * It cannot catch async rejections — those are handled by `useAsync` and the
 * axios interceptor, which is where API failures belong anyway.
 */
const route = useRoute()
const error = ref(null)

onErrorCaptured((err) => {
  error.value = err
  // Swallowed here so the app keeps running; the global handler still logs it.
  return false
})

// A navigation is the user's own attempt to recover — honour it.
watch(() => route.fullPath, () => { error.value = null })

function retry() {
  error.value = null
}
</script>

<template>
  <BaseEmptyState
    v-if="error"
    icon="alert"
    :title="t('common.screenFailedTitle')"
    :message="t('common.screenFailedMessage')"
  >
    <template #action>
      <div class="actions">
        <BaseButton variant="primary" @click="retry">{{ t('common.tryAgain') }}</BaseButton>
        <BaseButton variant="secondary" @click="$router.push('/')">{{ t('common.goToDashboard') }}</BaseButton>
      </div>
    </template>
  </BaseEmptyState>

  <slot v-else />
</template>

<style scoped lang="scss" src="./BaseErrorBoundary.scss"></style>
