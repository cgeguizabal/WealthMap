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

<style scoped lang="scss">
.notice {
  display: flex;
  align-items: flex-start;
  gap: var(--sp-3);

  padding: var(--sp-3) var(--sp-4);
  background: var(--warning-soft);
  border: 1px solid var(--warning);
  border-radius: var(--radius);
}

.notice__icon { color: var(--warning); margin-top: 1px; flex: none; }

.notice__text {
  font-size: var(--fs-sm);
  color: var(--text);
  line-height: 1.5;

  strong { font-weight: var(--fw-semibold); }
}
</style>
