<script setup>
import BaseIcon from '@/components/base/BaseIcon.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

defineProps({
  currencies: { type: Array, default: () => [] },
  currency: { type: String, required: true }
})
</script>

<template>
  <!--
    Deliberately visible rather than hidden: the totals above are incomplete and
    the user is entitled to know which holdings were left out. There are no FX
    rates in the system, so converting would be inventing numbers.
  -->
  <aside v-if="currencies.length" class="notice" role="note">
    <BaseIcon name="info" :size="17" class="notice__icon" />

    <p class="notice__text">
      {{ t('dashboard.totalsCover') }} <strong>{{ currency }}</strong> only.
      Holdings in <strong>{{ currencies.join(', ') }}</strong>
      {{ currencies.length === 1 ? 'is' : 'are' }} excluded — WealthMap does not convert between
      currencies, so mixing them would produce a number that looks right and is not.
    </p>
  </aside>
</template>

<style scoped lang="scss" src="@/assets/styles/features/dashboard/ExcludedCurrenciesNotice.scss"></style>
