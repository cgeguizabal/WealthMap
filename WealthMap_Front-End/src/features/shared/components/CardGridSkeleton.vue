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

<style scoped lang="scss">
.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(360px, 1fr));
  gap: var(--sp-4);
}

.card {
  display: flex;
  flex-direction: column;
  gap: var(--sp-3);

  padding: var(--sp-4) var(--sp-5);
  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
}

.card__head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--sp-3);
}

.card__foot {
  display: flex;
  gap: var(--sp-2);
  padding-top: var(--sp-3);
  border-top: var(--border-subtle);
}

@media (max-width: 640px) {
  .grid { grid-template-columns: 1fr; }
}
</style>
