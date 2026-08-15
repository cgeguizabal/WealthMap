<script setup>
/**
 * Whether a debit card reaches this account, and its digits.
 *
 * Account-only, so it lives here rather than beside the shared TrackingFields:
 * a credit card has no account behind it and would never show this.
 */
import { computed } from 'vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import { useI18n } from '@/composables/useI18n'
import { DEBIT_CARD_TYPE } from '@/api/tracking'

const { t } = useI18n()

const props = defineProps({
  debitCardType: { type: Number, default: DEBIT_CARD_TYPE.NONE },
  debitCardLastFour: { type: String, default: '' },
  error: { type: [Array, String], default: null }
})

const emit = defineEmits(['update:debitCardType', 'update:debitCardLastFour'])

const OPTIONS = computed(() => [
  { value: DEBIT_CARD_TYPE.NONE, label: t('accounts.noDebitCard'), icon: 'x' },
  { value: DEBIT_CARD_TYPE.PHYSICAL, label: t('accounts.physicalCard'), icon: 'card' },
  { value: DEBIT_CARD_TYPE.DIGITAL, label: t('accounts.digitalCard'), icon: 'phone' }
])

/** The number belongs to a card, so it only makes sense once one exists. */
const hasCard = computed(() => props.debitCardType !== DEBIT_CARD_TYPE.NONE)

function choose(value) {
  emit('update:debitCardType', value)

  // Clearing here as well as on the server: leaving the digits on screen after
  // choosing "no card" would show the user a number that is about to be dropped.
  if (value === DEBIT_CARD_TYPE.NONE) emit('update:debitCardLastFour', '')
}

function onDigits(value) {
  emit('update:debitCardLastFour', String(value ?? '').replace(/\D/g, '').slice(0, 4))
}
</script>

<template>
  <div class="debit">
    <fieldset class="debit__types">
      <legend class="debit__legend">{{ t('accounts.debitCard') }}</legend>
      <p class="debit__hint">{{ t('accounts.debitCardHint') }}</p>

      <div class="debit__options">
        <label
          v-for="option in OPTIONS"
          :key="option.value"
          :class="['debit__option', { 'debit__option--on': debitCardType === option.value }]"
        >
          <input
            type="radio"
            class="debit__radio"
            :value="option.value"
            :checked="debitCardType === option.value"
            @change="choose(option.value)"
          />
          <BaseIcon :name="option.icon" :size="16" />
          <span>{{ option.label }}</span>
        </label>
      </div>
    </fieldset>

    <BaseInput
      v-if="hasCard"
      :model-value="debitCardLastFour"
      :label="t('accounts.debitCardLastFour')"
      :hint="t('accounts.debitCardLastFourHint')"
      :error="error"
      inputmode="numeric"
      maxlength="4"
      placeholder="4417"
      @update:model-value="onDigits"
    />
  </div>
</template>

<style scoped lang="scss" src="@/assets/styles/features/accounts/DebitCardFields.scss"></style>
