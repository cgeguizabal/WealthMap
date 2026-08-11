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

<style scoped lang="scss" src="@/assets/styles/components/BaseSelect.scss"></style>
