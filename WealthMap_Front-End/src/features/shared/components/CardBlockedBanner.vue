<script setup>
import { computed } from 'vue'
import { useDateTime } from '@/composables/useDateTime'
import { useI18n } from '@/composables/useI18n'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseButton from '@/components/base/BaseButton.vue'

/**
 * Says the card is out of service, and offers the two ways back into it.
 *
 * Deliberately loud. A user who forgot they reported a card would otherwise see a
 * safe-to-spend figure quietly missing several thousand and have nothing to
 * connect it to.
 */
const { t } = useI18n()
const { formatDate } = useDateTime()

const props = defineProps({
  /** "Lost" | "Stolen" | "Damaged" | "Compromised", as the server names them. */
  reason: { type: String, default: null },
  blockedOn: { type: String, default: null },
  /** Credit cards lose spending headroom; a debit card does not. */
  kind: { type: String, required: true }
})

defineEmits(['replace', 'recover'])

const reasonLabel = computed(() => {
  const key = {
    Lost: 'cardLoss.reasonLost',
    Stolen: 'cardLoss.reasonStolen',
    Damaged: 'cardLoss.reasonDamaged',
    Compromised: 'cardLoss.reasonCompromised'
  }[props.reason]

  return key ? t(key) : props.reason
})
</script>

<template>
  <div class="blocked" role="status">
    <BaseIcon name="alert" :size="18" class="blocked__icon" />

    <div class="blocked__body">
      <p class="blocked__title">
        {{ t('cardLoss.bannerTitle', { reason: reasonLabel.toLowerCase() }) }}
        <span v-if="blockedOn" class="blocked__when">{{ formatDate(blockedOn) }}</span>
      </p>

      <p class="blocked__message">
        {{ kind === 'DebitCard' ? t('cardLoss.bannerDebit') : t('cardLoss.bannerCredit') }}
      </p>
    </div>

    <div class="blocked__actions">
      <BaseButton size="sm" variant="primary" @click="$emit('replace')">
        {{ t('cardLoss.recordReplacement') }}
      </BaseButton>
      <BaseButton size="sm" variant="ghost" @click="$emit('recover')">
        {{ t('cardLoss.foundIt') }}
      </BaseButton>
    </div>
  </div>
</template>

<style scoped lang="scss" src="@/assets/styles/features/shared/CardBlockedBanner.scss"></style>
