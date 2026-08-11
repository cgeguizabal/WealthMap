<script setup>
import { computed, ref, watch, onBeforeUnmount } from 'vue'
import { storeToRefs } from 'pinia'
import { useUiStore } from '@/stores/ui.store'
import { useI18n } from '@/composables/useI18n'
import BaseModal from './BaseModal.vue'
import BaseButton from './BaseButton.vue'

/**
 * Mounted once near the app root. Callers use `await ui.confirm({...})` and get
 * a boolean, so a destructive action reads top-to-bottom instead of splitting
 * across callbacks.
 */
const ui = useUiStore()
const { t } = useI18n()
const { confirmState } = storeToRefs(ui)

/** Fallbacks live here, not in the store, so they follow the language selector. */
const title = computed(() => confirmState.value?.title ?? t('common.areYouSure'))
const confirmLabel = computed(() => confirmState.value?.confirmLabel ?? t('common.confirm'))
const cancelLabel = computed(() => confirmState.value?.cancelLabel ?? t('common.cancel'))

const isOpen = computed({
  get: () => confirmState.value !== null,
  set: (open) => { if (!open) ui.resolveConfirm(false) }
})

/**
 * A dialog opened with `confirmDelayMs` starts disarmed, so a click already in
 * flight when it mounted cannot confirm it. See the store for why.
 */
const armed = ref(true)
let armTimer = null

watch(confirmState, (state) => {
  clearTimeout(armTimer)

  const delay = state?.confirmDelayMs ?? 0
  armed.value = delay === 0

  if (delay > 0) armTimer = setTimeout(() => { armed.value = true }, delay)
})

onBeforeUnmount(() => clearTimeout(armTimer))
</script>

<template>
  <BaseModal
    v-model="isOpen"
    :title="title"
    size="sm"
    :close-on-backdrop="false"
  >
    <p v-if="confirmState?.message" class="confirm__message">{{ confirmState.message }}</p>

    <template #footer>
      <BaseButton variant="secondary" @click="ui.resolveConfirm(false)">
        {{ cancelLabel }}
      </BaseButton>
      <BaseButton
        :variant="confirmState?.variant"
        :disabled="!armed"
        @click="ui.resolveConfirm(true)"
      >
        {{ confirmLabel }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss" src="@/assets/styles/components/BaseConfirmDialog.scss"></style>
