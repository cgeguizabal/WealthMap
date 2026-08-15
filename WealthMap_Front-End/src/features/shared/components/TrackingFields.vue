<script setup>
/**
 * The two identifying fields an account and a credit card share.
 *
 * One component rather than the same markup twice: the pair is governed by a
 * single rule (sync needs digits), and two copies would drift the moment that
 * rule changes — including the disabled state that currently keeps EmailSync
 * out of reach.
 */
import { computed } from 'vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import { useI18n } from '@/composables/useI18n'
import { TRACKING_MODE } from '@/api/tracking'

const { t } = useI18n()

const props = defineProps({
  lastFour: { type: String, default: '' },
  /** Named by the caller: an account's number and a card's number are not the same thing. */
  lastFourLabel: { type: String, default: '' },
  trackingMode: { type: Number, default: TRACKING_MODE.MANUAL },
  error: { type: [Array, String], default: null }
})

const emit = defineEmits(['update:lastFour', 'update:trackingMode'])

const MODES = computed(() => [
  {
    value: TRACKING_MODE.MANUAL,
    label: t('tracking.manual'),
    description: t('tracking.manualHint'),
    disabled: false
  },
  {
    value: TRACKING_MODE.EMAIL_SYNC,
    label: t('tracking.automatic'),
    description: t('tracking.automaticHint'),
    // Persisted and round-tripped, but not selectable: the ingestion that would
    // honour it does not exist yet, and an option that silently does nothing is
    // worse than one that says so.
    disabled: true
  }
])

/** Digits only, capped at four. Mirrors the server's `^\d{4}$`. */
function onDigits(value) {
  emit('update:lastFour', String(value ?? '').replace(/\D/g, '').slice(0, 4))
}
</script>

<template>
  <div class="tracking">
    <BaseInput
      :model-value="lastFour"
      :label="lastFourLabel || t('tracking.lastFour')"
      :hint="t('tracking.lastFourHint')"
      :error="error"
      inputmode="numeric"
      maxlength="4"
      placeholder="7765"
      @update:model-value="onDigits"
    />

    <fieldset class="tracking__modes">
      <legend class="tracking__legend">{{ t('tracking.mode') }}</legend>

      <label
        v-for="mode in MODES"
        :key="mode.value"
        :class="['tracking__option', { 'tracking__option--disabled': mode.disabled }]"
      >
        <input
          type="radio"
          class="tracking__radio"
          :value="mode.value"
          :checked="trackingMode === mode.value"
          :disabled="mode.disabled"
          @change="emit('update:trackingMode', mode.value)"
        />

        <span class="tracking__body">
          <span class="tracking__label">
            {{ mode.label }}
            <BaseBadge v-if="mode.disabled" size="sm">{{ t('tracking.comingSoon') }}</BaseBadge>
          </span>
          <span class="tracking__description">{{ mode.description }}</span>
        </span>
      </label>
    </fieldset>
  </div>
</template>

<style scoped lang="scss" src="@/assets/styles/features/shared/TrackingFields.scss"></style>
