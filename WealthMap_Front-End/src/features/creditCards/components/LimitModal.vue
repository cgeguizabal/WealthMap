<script setup>
import { ref, watch } from 'vue'
import { creditCardsApi } from '@/api/creditCards.api'
import { useForm } from '@/composables/useForm'
import { useToast } from '@/composables/useToast'
import { useMoney } from '@/composables/useMoney'
import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseButton from '@/components/base/BaseButton.vue'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  card: { type: Object, default: null }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const { format } = useMoney()

const { values, submitting, formError, submit, reset, fieldError } = useForm(
  { newLimit: null },
  (payload) => creditCardsApi.updateLimit(props.card.id, payload.newLimit)
)

const open = ref(props.modelValue)
watch(() => props.modelValue, (value) => {
  open.value = value
  if (value) reset({ newLimit: props.card?.creditLimit ?? null })
})
watch(open, (value) => emit('update:modelValue', value))

async function onSubmit() {
  const result = await submit()
  if (!result) return

  toast.success(`Limit is now ${format(result.creditLimit, { currency: result.currency })}.`)
  emit('saved', result)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" title="Update credit limit" size="sm">
    <form id="limit-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <p v-if="card" class="form__context">
        Currently owed
        <strong class="numeric">{{ format(card.usedCredit, { currency: card.currency }) }}</strong>
      </p>

      <BaseInput
        v-model="values.newLimit"
        label="New limit"
        type="number"
        step="0.01"
        min="0"
        required
        hint="Cannot be set below what is currently owed."
        :error="fieldError('newLimit')"
      >
        <template #prefix>{{ card?.currency }}</template>
      </BaseInput>
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">Cancel</BaseButton>
      <BaseButton type="submit" form="limit-form" variant="primary" :loading="submitting">
        Update limit
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss">
.form { display: flex; flex-direction: column; gap: var(--sp-4); }

.form__context {
  padding: var(--sp-3);
  background: var(--canvas-alt);
  border-radius: var(--radius-sm);
  font-size: var(--fs-sm);
  color: var(--text-muted);

  strong { color: var(--text); font-weight: var(--fw-semibold); }
}

.form__error {
  padding: var(--sp-3);
  border: 1px solid var(--negative);
  border-radius: var(--radius);
  background: var(--negative-soft);
  color: var(--negative);
  font-size: var(--fs-sm);
}
</style>
