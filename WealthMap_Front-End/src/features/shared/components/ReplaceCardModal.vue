<script setup>
import { ref, watch } from 'vue'
import { cardIncidentsApi } from '@/api/cardIncidents.api'
import { useForm } from '@/composables/useForm'
import { useToast } from '@/composables/useToast'
import { useDashboardStore } from '@/stores/dashboard.store'

import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useI18n } from '@/composables/useI18n'

/**
 * Records the card the bank sent, and the number it arrived with.
 *
 * The digits are optional on purpose. Some banks reissue the same number after
 * damage, and leaving the field empty means "unchanged" rather than "forget which
 * card this is" — so the user is never forced to invent a number to close a report.
 */
const { t } = useI18n()

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  kind: { type: String, required: true },
  card: { type: Object, default: null }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const dashboard = useDashboardStore()

function blank() {
  return {
    newLastFour: '',
    replacedOn: new Date().toISOString().slice(0, 10),
    notes: ''
  }
}

const { values, submitting, formError, submit, reset, fieldError } = useForm(blank(), (payload) =>
  cardIncidentsApi.replace(props.kind, props.card.id, {
    newLastFour: payload.newLastFour || null,
    replacedOn: payload.replacedOn,
    notes: payload.notes || null
  })
)

const open = ref(props.modelValue)

watch(() => props.modelValue, (value) => {
  open.value = value
  if (value) reset(blank())
})

watch(open, (value) => emit('update:modelValue', value))

/** Digits only, four at most — the same shape the field on the card accepts. */
function onDigits(value) {
  values.newLastFour = String(value ?? '').replace(/\D/g, '').slice(0, 4)
}

async function onSubmit() {
  const result = await submit()
  if (!result) return

  toast.success(t('cardLoss.replacedToast', { card: result.cardName }))

  // The card is spendable again, so safe-to-spend has changed.
  dashboard.invalidate()

  emit('saved', result)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" :title="t('cardLoss.replaceTitle')" size="sm">
    <form id="replace-card-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <p v-if="card?.lastFour" class="form__note">
        {{ t('cardLoss.replacingNumber') }}
        <span class="numeric">••••{{ card.lastFour }}</span>
      </p>

      <BaseInput
        :model-value="values.newLastFour"
        :label="t('cardLoss.newLastFour')"
        inputmode="numeric"
        maxlength="4"
        :hint="t('cardLoss.newLastFourHint')"
        :error="fieldError('newLastFour')"
        @update:model-value="onDigits"
      />

      <BaseInput
        v-model="values.replacedOn"
        :label="t('cardLoss.replacedOn')"
        type="date"
        required
        :error="fieldError('replacedOn')"
      />

      <BaseInput
        v-model="values.notes"
        :label="t('common.notes')"
        :error="fieldError('notes')"
      />
    </form>

    <template #footer>
      <BaseButton variant="ghost" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="replace-card-form" variant="primary" :loading="submitting">
        {{ t('cardLoss.confirmReplace') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss" src="@/assets/styles/features/shared/CardLossModal.scss"></style>
