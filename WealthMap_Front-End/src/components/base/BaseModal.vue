<script setup>
import { ref, watch, nextTick, onUnmounted } from 'vue'
import BaseIcon from './BaseIcon.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  title: { type: String, default: '' },
  size: { type: String, default: 'md', validator: (v) => ['sm', 'md', 'lg'].includes(v) },
  /** Set false for destructive flows where a stray click should not dismiss. */
  closeOnBackdrop: { type: Boolean, default: true }
})

const emit = defineEmits(['update:modelValue', 'close'])

const panel = ref(null)
let lastFocused = null

const FOCUSABLE = [
  'a[href]', 'button:not([disabled])', 'input:not([disabled])',
  'select:not([disabled])', 'textarea:not([disabled])', '[tabindex]:not([tabindex="-1"])'
].join(',')

function close() {
  emit('update:modelValue', false)
  emit('close')
}

function onBackdrop() {
  if (props.closeOnBackdrop) close()
}

/**
 * Keeps Tab inside the dialog. Without this, tabbing walks into the page behind
 * the modal, which is invisible to sighted users but not to keyboard or screen
 * reader users.
 */
function onKeydown(event) {
  if (event.key === 'Escape') {
    close()
    return
  }

  if (event.key !== 'Tab' || !panel.value) return

  const focusable = Array.from(panel.value.querySelectorAll(FOCUSABLE))
  if (focusable.length === 0) return

  const first = focusable[0]
  const last = focusable[focusable.length - 1]

  if (event.shiftKey && document.activeElement === first) {
    event.preventDefault()
    last.focus()
  } else if (!event.shiftKey && document.activeElement === last) {
    event.preventDefault()
    first.focus()
  }
}

function lockScroll(locked) {
  document.body.style.overflow = locked ? 'hidden' : ''
}

watch(() => props.modelValue, async (open) => {
  lockScroll(open)

  if (open) {
    lastFocused = document.activeElement
    await nextTick()
    const target = panel.value?.querySelector(FOCUSABLE) ?? panel.value
    target?.focus()
  } else {
    // Returning focus to whatever opened the modal keeps keyboard context.
    lastFocused?.focus?.()
    lastFocused = null
  }
})

onUnmounted(() => lockScroll(false))
</script>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="modelValue"
        class="modal"
        role="dialog"
        aria-modal="true"
        :aria-label="title || undefined"
        @keydown="onKeydown"
      >
        <div class="modal__backdrop" @click="onBackdrop" />

        <div ref="panel" :class="['modal__panel', `modal__panel--${size}`]" tabindex="-1">
          <header class="modal__header">
            <slot name="header">
              <h3 class="modal__title">{{ title }}</h3>
            </slot>

            <button class="modal__close" type="button" :aria-label="t('common.closeDialog')" @click="close">
              <BaseIcon name="x" :size="18" />
            </button>
          </header>

          <div class="modal__body">
            <slot />
          </div>

          <footer v-if="$slots.footer" class="modal__footer">
            <slot name="footer" />
          </footer>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped lang="scss" src="@/assets/styles/components/BaseModal.scss"></style>
