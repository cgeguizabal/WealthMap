<script setup>
import { computed, useId } from 'vue'
import BaseIcon from './BaseIcon.vue'

const props = defineProps({
  modelValue: { type: [String, Number, null], default: null },
  label: { type: String, default: '' },
  /** `[{ value, label, disabled? }]` */
  options: { type: Array, default: () => [] },
  placeholder: { type: String, default: 'Select…' },
  hint: { type: String, default: '' },
  error: { type: [Array, String], default: null },
  disabled: { type: Boolean, default: false },
  required: { type: Boolean, default: false }
})

const emit = defineEmits(['update:modelValue'])

const id = useId()
const messages = computed(() => {
  if (!props.error) return []
  return Array.isArray(props.error) ? props.error : [props.error]
})
const hasError = computed(() => messages.value.length > 0)

/**
 * Backend enums are integers. A <select> value is always a string, so restore
 * the original option's type rather than posting "2" where 2 is required.
 */
function onChange(event) {
  const raw = event.target.value

  if (raw === '') {
    emit('update:modelValue', null)
    return
  }

  const match = props.options.find((option) => String(option.value) === raw)
  emit('update:modelValue', match ? match.value : raw)
}
</script>

<template>
  <div class="field">
    <label v-if="label" :for="id" class="field__label">
      {{ label }}
      <span v-if="required" class="field__required" aria-hidden="true">*</span>
    </label>

    <div :class="['field__control', { 'field__control--error': hasError, 'field__control--disabled': disabled }]">
      <select
        :id="id"
        class="field__select"
        :value="modelValue === null || modelValue === undefined ? '' : String(modelValue)"
        :disabled="disabled"
        :required="required"
        :aria-invalid="hasError"
        @change="onChange"
      >
        <option value="" :disabled="required">{{ placeholder }}</option>
        <option
          v-for="option in options"
          :key="String(option.value)"
          :value="String(option.value)"
          :disabled="option.disabled"
        >
          {{ option.label }}
        </option>
      </select>

      <BaseIcon name="chevron-down" :size="16" class="field__chevron" />
    </div>

    <p v-if="hasError" class="field__error">{{ messages[0] }}</p>
    <p v-else-if="hint" class="field__hint">{{ hint }}</p>
  </div>
</template>

<style scoped lang="scss">
/*
 * A native <select> is as wide as its longest option. Grid and flex items
 * default to a min-size of `auto`, so that intrinsic width becomes a floor the
 * column cannot go below — one long option then widens the whole row. The
 * `min-width: 0` chain below lets it shrink, and the label ellipsises instead.
 */
.field {
  display: flex;
  flex-direction: column;
  gap: var(--sp-2);
  min-width: 0;
}

.field__label { font-size: var(--fs-sm); font-weight: var(--fw-medium); }
.field__required { color: var(--negative); margin-left: 2px; }

.field__control {
  position: relative;
  display: flex;
  align-items: center;

  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  height: 38px;
  padding: 0 var(--sp-3);
  min-width: 0;

  &:focus-within { box-shadow: var(--shadow-sm); }

  &--error { border-color: var(--negative); background: var(--negative-soft); }
  &--disabled { opacity: 0.55; background: var(--canvas-alt); }
}

.field__select {
  flex: 1 1 auto;
  width: 100%;
  min-width: 0;
  max-width: 100%;
  height: 100%;

  border: none;
  outline: none;
  background: transparent;
  font-size: var(--fs-base);
  cursor: pointer;

  /* Long option labels truncate rather than force the control wider */
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;

  appearance: none;
  -webkit-appearance: none;
  -moz-appearance: none;
  padding-right: var(--sp-5);
}

.field__chevron {
  position: absolute;
  right: var(--sp-3);
  pointer-events: none;
  color: var(--text-muted);
}

.field__error { font-size: var(--fs-xs); color: var(--negative); }
.field__hint { font-size: var(--fs-xs); color: var(--text-muted); }
</style>
