<script setup>
import { useOnlineStatus } from '@/composables/useOnlineStatus'
import BaseIcon from '@/components/base/BaseIcon.vue'

const { isOnline } = useOnlineStatus()
</script>

<template>
  <Transition name="offline">
    <div v-if="!isOnline" class="offline" role="status">
      <BaseIcon name="alert" :size="16" />
      <span>
        You are offline. WealthMap never shows cached balances, so figures will not load
        until the connection is back.
      </span>
    </div>
  </Transition>
</template>

<style scoped lang="scss">
.offline {
  display: flex;
  align-items: center;
  gap: var(--sp-2);

  padding: var(--sp-2) var(--sp-5);
  background: var(--warning-soft);
  border-bottom: 1px solid var(--warning);
  color: var(--text);
  font-size: var(--fs-sm);
}

.offline-enter-active, .offline-leave-active { transition: opacity var(--dur) var(--ease); }
.offline-enter-from, .offline-leave-to { opacity: 0; }

@media (max-width: 767px) {
  .offline { padding: var(--sp-2) var(--sp-4); font-size: var(--fs-xs); }
}
</style>
