<script setup>
import { ref, watch, computed } from 'vue'
import { cardIncidentsApi, CARD_LOSS_REASON } from '@/api/cardIncidents.api'
import { useForm } from '@/composables/useForm'
import { useToast } from '@/composables/useToast'
import { useDashboardStore } from '@/stores/dashboard.store'

import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useI18n } from '@/composables/useI18n'

/**
 * Takes a card out of service and says why.
 *
 * Serves credit cards and debit cards alike — the kind decides the endpoint and
 * nothing else, because losing one is the same event as losing the other.
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

const reasons = computed(() => [
  { value: CARD_LOSS_REASON.LOST, label: t('cardLoss.reasonLost') },
  { value: CARD_LOSS_REASON.STOLEN, label: t('cardLoss.reasonStolen') },
  { value: CARD_LOSS_REASON.DAMAGED, label: t('cardLoss.reasonDamaged') },
  { value: CARD_LOSS_REASON.COMPROMISED, label: t('cardLoss.reasonCompromised') }
])

function blank() {
  return {
    reason: CARD_LOSS_REASON.LOST,
    // Today, because a card is usually reported the day it goes missing. Editable
    // for the times someone notices later.
    reportedOn: new Date().toISOString().slice(0, 10),
    notes: ''
  }
}

const { values, submitting, formError, submit, reset, fieldError } = useForm(blank(), (payload) =>
  cardIncidentsApi.report(props.kind, props.card.id, {
    reason: Number(payload.reason),
    reportedOn: payload.reportedOn,
    notes: payload.notes || null
  })
)

const open = ref(props.modelValue)

watch(() => props.modelValue, (value) => {
  open.value = value
  if (value) reset(blank())
})

watch(open, (value) => emit('update:modelValue', value))

async function onSubmit() {
  const result = await submit()
  if (!result) return

  toast.success(t('cardLoss.reportedToast', { card: result.cardName }))

  // A blocked credit card stops offering headroom, so safe-to-spend has changed.
  dashboard.invalidate()

  emit('saved', result)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" :title="t('cardLoss.reportTitle')" size="sm">
    <form id="report-card-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <p v-if="card" class="form__note">
        {{ t('cardLoss.reportIntro', { card: card.name }) }}
        <span v-if="card.lastFour" class="numeric">••••{{ card.lastFour }}</span>
      </p>

      <BaseSelect
        v-model="values.reason"
        :label="t('cardLoss.reason')"
        :options="reasons"
        required
        :error="fieldError('reason')"
      />

      <BaseInput
        v-model="values.reportedOn"
        :label="t('cardLoss.reportedOn')"
        type="date"
        required
        :hint="t('cardLoss.reportedOnHint')"
        :error="fieldError('reportedOn')"
      />

      <BaseInput
        v-model="values.notes"
        :label="t('common.notes')"
        :placeholder="t('cardLoss.notesPlaceholder')"
        :error="fieldError('notes')"
      />

      <p class="form__note form__note--muted">{{ t('cardLoss.reportConsequence') }}</p>
    </form>

    <template #footer>
      <BaseButton variant="ghost" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="report-card-form" variant="danger" :loading="submitting">
        {{ t('cardLoss.confirmReport') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss" src="@/assets/styles/features/shared/CardLossModal.scss"></style>
