<script setup>
import BaseSkeleton from '@/components/base/BaseSkeleton.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

/**
 * Placeholder for the card grids used by accounts, cards, plans, debts and
 * goals. Shaped like the real card so the page does not reflow when data lands.
 */
defineProps({
  count: { type: Number, default: 3 }
})
</script>

<template>
  <div class="grid" role="status" :aria-label="t('common.loading')">
    <article v-for="n in count" :key="n" class="card">
      <div class="card__head">
        <BaseSkeleton width="55%" :height="15" />
        <BaseSkeleton width="60px" :height="18" />
      </div>

      <BaseSkeleton width="70%" :height="26" />
      <BaseSkeleton :height="10" />

      <div class="card__foot">
        <BaseSkeleton width="88px" :height="30" radius="var(--radius)" />
        <BaseSkeleton width="72px" :height="30" radius="var(--radius)" />
      </div>
    </article>
  </div>
</template>

<style scoped lang="scss" src="@/assets/styles/features/shared/CardGridSkeleton.scss"></style>
