<script setup>
import { computed, useId } from 'vue'

const props = defineProps({
  modelValue: { type: [String, Number], default: '' },
  label: { type: String, default: '' },
  type: { type: String, default: 'text' },
  placeholder: { type: String, default: '' },
  hint: { type: String, default: '' },
  /** Field errors arrive from the API as an array of messages. */
  error: { type: [Array, String], default: null },
  disabled: { type: Boolean, default: false },
  required: { type: Boolean, default: false },
  autocomplete: { type: String, default: 'off' },
  min: { type: [String, Number], default: undefined },
  max: { type: [String, Number], default: undefined },
  step: { type: [String, Number], default: undefined },
  inputmode: { type: String, default: undefined }
})

const emit = defineEmits(['update:modelValue', 'blur'])

const id = useId()
const messages = computed(() => {
  if (!props.error) return []
  return Array.isArray(props.error) ? props.error : [props.error]
})
const hasError = computed(() => messages.value.length > 0)

/** Number inputs emit strings; coerce so callers never post "12" where 12 is meant. */
function onInput(event) {
  const raw = event.target.value

  if (props.type === 'number') {
    emit('update:modelValue', raw === '' ? null : Number(raw))
    return
  }

  emit('update:modelValue', raw)
}
</script>

<template>
  <div class="field">
    <label v-if="label" :for="id" class="field__label">
      {{ label }}
      <span v-if="required" class="field__required" aria-hidden="true">*</span>
    </label>

    <div :class="['field__control', { 'field__control--error': hasError, 'field__control--disabled': disabled }]">
      <span v-if="$slots.prefix" class="field__affix"><slot name="prefix" /></span>

      <input
        :id="id"
        class="field__input"
        :type="type"
        :value="modelValue"
        :placeholder="placeholder"
        :disabled="disabled"
        :required="required"
        :autocomplete="autocomplete"
        :min="min"
        :max="max"
        :step="step"
        :inputmode="inputmode"
        :aria-invalid="hasError"
        :aria-describedby="hasError ? `${id}-error` : hint ? `${id}-hint` : undefined"
        @input="onInput"
        @blur="$emit('blur', $event)"
      />

      <span v-if="$slots.suffix" class="field__affix"><slot name="suffix" /></span>
    </div>

    <p v-if="hasError" :id="`${id}-error`" class="field__error">{{ messages[0] }}</p>
    <p v-else-if="hint" :id="`${id}-hint`" class="field__hint">{{ hint }}</p>
  </div>
</template>

<style scoped lang="scss" src="@/assets/styles/components/BaseInput.scss"></style>
