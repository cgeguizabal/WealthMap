<script setup>
import { computed } from 'vue'
import BaseIcon from './BaseIcon.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const props = defineProps({
  page: { type: Number, required: true },
  pageSize: { type: Number, default: 20 },
  totalCount: { type: Number, default: 0 },
  totalPages: { type: Number, default: 0 },
  hasNextPage: { type: Boolean, default: false },
  hasPreviousPage: { type: Boolean, default: false }
})

const emit = defineEmits(['update:page'])

const from = computed(() => (props.totalCount === 0 ? 0 : (props.page - 1) * props.pageSize + 1))
const to = computed(() => Math.min(props.page * props.pageSize, props.totalCount))

/** A windowed page list with ellipses, so 400 pages do not render 400 buttons. */
const pages = computed(() => {
  const total = props.totalPages
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1)

  const current = props.page
  const result = [1]

  const start = Math.max(2, current - 1)
  const end = Math.min(total - 1, current + 1)

  if (start > 2) result.push('…')
  for (let i = start; i <= end; i++) result.push(i)
  if (end < total - 1) result.push('…')

  result.push(total)
  return result
})

function go(page) {
  if (page !== props.page && page >= 1 && page <= props.totalPages) {
    emit('update:page', page)
  }
}
</script>

<template>
  <nav v-if="totalPages > 1" class="pagination" :aria-label="t('common.pagination')">
    <p class="pagination__summary numeric">
      {{ from }}–{{ to }} of {{ totalCount }}
    </p>

    <div class="pagination__controls">
      <button
        class="pagination__btn"
        type="button"
        :disabled="!hasPreviousPage"
        :aria-label="t('common.previousPage')"
        @click="go(page - 1)"
      >
        <BaseIcon name="chevron-left" :size="16" />
      </button>

      <template v-for="(item, index) in pages" :key="`${item}-${index}`">
        <span v-if="item === '…'" class="pagination__gap">…</span>
        <button
          v-else
          class="pagination__btn pagination__btn--page numeric"
          :class="{ 'is-active': item === page }"
          type="button"
          :aria-current="item === page ? 'page' : undefined"
          @click="go(item)"
        >
          {{ item }}
        </button>
      </template>

      <button
        class="pagination__btn"
        type="button"
        :disabled="!hasNextPage"
        :aria-label="t('common.nextPage')"
        @click="go(page + 1)"
      >
        <BaseIcon name="chevron-right" :size="16" />
      </button>
    </div>
  </nav>
</template>

<style scoped lang="scss">
.pagination {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--sp-4);
  padding: var(--sp-3) var(--sp-4);
  border-top: var(--border-subtle);
}

.pagination__summary { font-size: var(--fs-sm); color: var(--text-muted); }

.pagination__controls { display: flex; align-items: center; gap: var(--sp-1); }

.pagination__btn {
  display: grid;
  place-items: center;
  min-width: 30px;
  height: 30px;
  padding: 0 var(--sp-2);

  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  background: transparent;
  color: var(--text);
  font-size: var(--fs-sm);
  cursor: pointer;

  @include focus-ring;

  &:hover:not(:disabled) { background: var(--canvas-alt); }
  &:disabled { opacity: 0.35; cursor: not-allowed; }

  &.is-active {
    background: var(--ink);
    border-color: var(--border-color);
    color: #fff;
    font-weight: var(--fw-semibold);
  }
}

.pagination__gap { color: var(--text-subtle); padding: 0 var(--sp-1); }

@media (max-width: 767px) {
  .pagination { flex-direction: column; align-items: stretch; gap: var(--sp-3); }
  .pagination__controls { justify-content: center; }
  .pagination__summary { text-align: center; }
}
</style>
