<script setup>
import { ref, onErrorCaptured, watch } from 'vue'
import { useRoute } from 'vue-router'
import BaseEmptyState from './BaseEmptyState.vue'
import BaseButton from './BaseButton.vue'

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
    title="This screen ran into a problem"
    message="The rest of the app is still fine. Try again, or move to another section."
  >
    <template #action>
      <div class="actions">
        <BaseButton variant="primary" @click="retry">Try again</BaseButton>
        <BaseButton variant="secondary" @click="$router.push('/')">Go to dashboard</BaseButton>
      </div>
    </template>
  </BaseEmptyState>

  <slot v-else />
</template>

<style scoped lang="scss">
.actions { display: flex; gap: var(--sp-2); flex-wrap: wrap; justify-content: center; }
</style>
