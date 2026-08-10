<script setup>
import { computed } from 'vue'
import { storeToRefs } from 'pinia'
import { useUiStore } from '@/stores/ui.store'
import BaseModal from './BaseModal.vue'
import BaseButton from './BaseButton.vue'

/**
 * Mounted once near the app root. Callers use `await ui.confirm({...})` and get
 * a boolean, so a destructive action reads top-to-bottom instead of splitting
 * across callbacks.
 */
const ui = useUiStore()
const { confirmState } = storeToRefs(ui)

const isOpen = computed({
  get: () => confirmState.value !== null,
  set: (open) => { if (!open) ui.resolveConfirm(false) }
})
</script>

<template>
  <BaseModal
    v-model="isOpen"
    :title="confirmState?.title"
    size="sm"
    :close-on-backdrop="false"
  >
    <p v-if="confirmState?.message" class="confirm__message">{{ confirmState.message }}</p>

    <template #footer>
      <BaseButton variant="secondary" @click="ui.resolveConfirm(false)">
        {{ confirmState?.cancelLabel }}
      </BaseButton>
      <BaseButton :variant="confirmState?.variant" @click="ui.resolveConfirm(true)">
        {{ confirmState?.confirmLabel }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss">
.confirm__message {
  font-size: var(--fs-base);
  color: var(--text-muted);
  line-height: 1.6;
}
</style>
