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

<style scoped lang="scss">
/* min-width: 0 so the field can shrink inside a grid or flex row (see BaseSelect) */
.field {
  display: flex;
  flex-direction: column;
  gap: var(--sp-2);
  min-width: 0;
}

.field__label {
  font-size: var(--fs-sm);
  font-weight: var(--fw-medium);
  color: var(--text);
}

.field__required { color: var(--negative); margin-left: 2px; }

.field__control {
  display: flex;
  align-items: center;
  gap: var(--sp-2);

  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  padding: 0 var(--sp-3);
  height: 38px;
  min-width: 0;

  transition: box-shadow var(--dur-fast) var(--ease);

  &:focus-within {
    box-shadow: var(--shadow-sm);
  }

  &--error {
    border-color: var(--negative);
    background: var(--negative-soft);
  }

  &--disabled { opacity: 0.55; background: var(--canvas-alt); }
}

.field__input {
  flex: 1;
  min-width: 0;
  border: none;
  outline: none;
  background: transparent;
  font-size: var(--fs-base);

  &::placeholder { color: var(--text-subtle); }

  /* Number spinners fight the flat aesthetic and invite mis-clicks. */
  &[type='number'] {
    -moz-appearance: textfield;
    appearance: textfield;

    &::-webkit-outer-spin-button,
    &::-webkit-inner-spin-button { -webkit-appearance: none; margin: 0; }
  }
}

.field__affix {
  display: inline-flex;
  align-items: center;
  color: var(--text-muted);
  font-size: var(--fs-sm);
  flex: none;
}

.field__error { font-size: var(--fs-xs); color: var(--negative); }
.field__hint { font-size: var(--fs-xs); color: var(--text-muted); }
</style>
