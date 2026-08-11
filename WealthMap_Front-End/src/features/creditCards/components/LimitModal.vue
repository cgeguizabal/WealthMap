<script setup>
import { ref, watch } from 'vue'
import { creditCardsApi } from '@/api/creditCards.api'
import { useForm } from '@/composables/useForm'
import { useToast } from '@/composables/useToast'
import { useMoney } from '@/composables/useMoney'
import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

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
  <BaseModal v-model="open" :title="t('cards.updateLimit')" size="sm">
    <form id="limit-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <p v-if="card" class="form__context">
        {{ t('cards.currentlyOwed') }}
        <strong class="numeric">{{ format(card.usedCredit, { currency: card.currency }) }}</strong>
      </p>

      <BaseInput
        v-model="values.newLimit"
        :label="t('cards.newLimit')"
        type="number"
        step="0.01"
        min="0"
        required
        :hint="t('cards.limitHint')"
        :error="fieldError('newLimit')"
      >
        <template #prefix>{{ card?.currency }}</template>
      </BaseInput>
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="limit-form" variant="primary" :loading="submitting">
        {{ t('cards.updateLimit') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss" src="./LimitModal.scss"></style>
