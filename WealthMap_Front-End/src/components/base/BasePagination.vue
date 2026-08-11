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

<style scoped lang="scss" src="./BasePagination.scss"></style>
