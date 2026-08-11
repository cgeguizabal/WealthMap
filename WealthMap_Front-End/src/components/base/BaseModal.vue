<script setup>
import { ref, watch, nextTick, onUnmounted } from 'vue'
import BaseIcon from './BaseIcon.vue'

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

            <button class="modal__close" type="button" aria-label="Close dialog" @click="close">
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

<style scoped lang="scss">
.modal {
  position: fixed;
  inset: 0;
  z-index: 100;
  display: grid;
  place-items: center;
  padding: var(--sp-4);
}

.modal__backdrop {
  position: absolute;
  inset: 0;
  background: rgba(32, 31, 29, 0.45);
}

.modal__panel {
  position: relative;
  display: flex;
  flex-direction: column;
  width: 100%;
  max-height: calc(100vh - var(--sp-8));

  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow-raised);

  /* The footer paints its own background, and a square-cornered child would
     cover the panel's rounded corners without this. */
  overflow: hidden;

  &:focus { outline: none; }
}

.modal__panel--sm { max-width: 400px; }
.modal__panel--md { max-width: 560px; }
.modal__panel--lg { max-width: 780px; }

.modal__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--sp-4);
  padding: var(--sp-4) var(--sp-5);
  border-bottom: var(--border-subtle);
}

.modal__title { font-size: var(--fs-md); font-weight: var(--fw-semibold); }

.modal__close {
  display: grid;
  place-items: center;
  width: 28px;
  height: 28px;
  flex: none;

  border: none;
  border-radius: var(--radius-sm);
  background: transparent;
  color: var(--text-muted);
  cursor: pointer;

  @include focus-ring;

  &:hover { background: var(--canvas-alt); color: var(--ink); }
}

.modal__body {
  padding: var(--sp-5);
  overflow-y: auto;
}

.modal__footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--sp-2);
  padding: var(--sp-4) var(--sp-5);
  border-top: var(--border-subtle);
  background: var(--canvas-alt);
}

/* ── Transition ───────────────────────────── */
.modal-enter-active, .modal-leave-active { transition: opacity var(--dur) var(--ease); }
.modal-enter-from, .modal-leave-to { opacity: 0; }

.modal-enter-active .modal__panel {
  transition: transform var(--dur) var(--ease);
}
.modal-enter-from .modal__panel { transform: translateY(-8px); }

@media (max-width: 767px) {
  .modal { padding: var(--sp-3); align-items: flex-end; }
  .modal__panel { max-height: 88vh; }
  .modal__body { padding: var(--sp-4); }
  .modal__footer { padding: var(--sp-3) var(--sp-4); }
}

@media (prefers-reduced-motion: reduce) {
  .modal-enter-active, .modal-leave-active,
  .modal-enter-active .modal__panel { transition: none; }
}
</style>
