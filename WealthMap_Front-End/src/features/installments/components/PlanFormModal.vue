<script setup>
import { ref, watch, computed } from 'vue'
import { installmentsApi, previewSchedule } from '@/api/installments.api'
import { creditCardsApi } from '@/api/creditCards.api'
import { useForm } from '@/composables/useForm'
import { useToast } from '@/composables/useToast'
import { useMoney } from '@/composables/useMoney'

import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import StorePicker from '@/features/shared/components/StorePicker.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const props = defineProps({
  modelValue: { type: Boolean, default: false }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const { format } = useMoney()

const cards = ref([])

const MONTH_OPTIONS = [3, 6, 9, 12, 18, 24, 36].map((n) => ({ value: n, label: `${n} months` }))

function today() {
  return new Date().toISOString().slice(0, 10)
}

function blank() {
  return {
    productName: '',
    totalPrice: null,
    creditCardId: null,
    monthsCount: 12,
    purchasedAt: today(),
    storeId: null
  }
}

const { values, submitting, formError, submit, reset, fieldError } = useForm(blank(), (payload) =>
  installmentsApi.create({
    productName: payload.productName,
    totalPrice: payload.totalPrice,
    storeId: payload.storeId || null,
    creditCardId: payload.creditCardId,
    monthsCount: payload.monthsCount,
    purchasedAt: payload.purchasedAt || null
  })
)

const selectedCard = computed(() => cards.value.find((c) => c.id === values.creditCardId) ?? null)

const cardOptions = computed(() =>
  cards.value.map((card) => ({
    value: card.id,
    label: `${card.cardName} — ${format(card.availableCredit, { currency: card.currency })} available`
  }))
)

const schedule = computed(() => previewSchedule(values.totalPrice, values.monthsCount))

/**
 * The whole price is charged on day one, so a plan that does not fit the card's
 * available credit is rejected. Better to say so before the request.
 */
const exceedsCredit = computed(() => {
  if (!selectedCard.value || !values.totalPrice) return false
  return Number(values.totalPrice) > selectedCard.value.availableCredit
})

const open = ref(props.modelValue)
watch(() => props.modelValue, async (value) => {
  open.value = value

  if (value) {
    reset(blank())
    try {
      cards.value = await creditCardsApi.list()
    } catch {
      cards.value = []
    }
  }
})
watch(open, (value) => emit('update:modelValue', value))

async function onSubmit() {
  const plan = await submit()
  if (!plan) return

  toast.success(`${plan.productName} split into ${plan.monthsCount} payments.`)
  emit('saved', plan)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" :title="t('installments.planTitle')">
    <form id="plan-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <BaseInput
        v-model="values.productName"
        :label="t('installments.productLabel')"
        :placeholder="t('installments.productPlaceholder')"
        required
        :error="fieldError('productName')"
      />

      <BaseSelect
        v-model="values.creditCardId"
        :label="t('installments.card')"
        :options="cardOptions"
        :placeholder="cardOptions.length ? t('installments.chooseCard') : t('installments.noCards')"
        required
        :hint="t('installments.cardHint')"
        :error="fieldError('creditCardId')"
      />

      <div class="form__row">
        <BaseInput
          v-model="values.totalPrice"
          :label="t('installments.totalPrice')"
          type="number"
          step="0.01"
          min="0"
          placeholder="1200.00"
          required
          :error="fieldError('totalPrice')"
        >
          <template #prefix>{{ selectedCard?.currency ?? '' }}</template>
        </BaseInput>

        <BaseSelect
          v-model="values.monthsCount"
          :label="t('installments.months')"
          :options="MONTH_OPTIONS"
          required
          :error="fieldError('monthsCount')"
        />
      </div>

      <!-- Computed here with the same rule the backend uses, so the number shown
           before submitting is the number that gets created. -->
      <div v-if="schedule" class="preview">
        <BaseIcon name="layers" :size="16" class="preview__icon" />
        <div>
          <p class="preview__headline numeric">
            <template v-if="schedule.isEven">
              {{ schedule.months }} × {{ format(schedule.base, { currency: selectedCard?.currency }) }}
            </template>
            <template v-else>
              {{ schedule.months - 1 }} × {{ format(schedule.base, { currency: selectedCard?.currency }) }}
              + {{ format(schedule.last, { currency: selectedCard?.currency }) }}
            </template>
          </p>
          <p class="preview__note">
            {{ t('installments.interestFreeHint') }}
          </p>
        </div>
      </div>

      <p v-if="exceedsCredit" class="warning" role="alert">
        <BaseIcon name="alert" :size="16" />
        This exceeds the card's available credit
        ({{ format(selectedCard.availableCredit, { currency: selectedCard.currency }) }}).
        The plan will be declined.
      </p>

      <div class="form__row">
        <BaseInput
          v-model="values.purchasedAt"
          :label="t('installments.purchaseDate')"
          type="date"
          :max="today()"
          :error="fieldError('purchasedAt')"
        />

        <StorePicker v-model="values.storeId" :error="fieldError('storeId')" />
      </div>
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="plan-form" variant="primary" :loading="submitting">
        {{ t('installments.createPlan') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss" src="@/assets/styles/features/installments/PlanFormModal.scss"></style>
