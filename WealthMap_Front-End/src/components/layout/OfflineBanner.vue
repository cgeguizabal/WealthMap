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

<style scoped lang="scss" src="@/assets/styles/layout/OfflineBanner.scss"></style>
