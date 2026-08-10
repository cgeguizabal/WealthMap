import { ref, computed } from 'vue'

/**
 * Mirrors the backend's paging envelope
 * `{ items, page, pageSize, totalCount, totalPages, hasNextPage, hasPreviousPage }`.
 * Call `apply(response)` after each fetch and the derived state stays in step.
 */
export function usePagination({ pageSize = 20 } = {}) {
  const page = ref(1)
  const size = ref(pageSize)
  const totalCount = ref(0)
  const totalPages = ref(0)
  const hasNextPage = ref(false)
  const hasPreviousPage = ref(false)

  const isEmpty = computed(() => totalCount.value === 0)

  function apply(response) {
    if (!response) return []

    page.value = response.page ?? page.value
    size.value = response.pageSize ?? size.value
    totalCount.value = response.totalCount ?? 0
    totalPages.value = response.totalPages ?? 0
    hasNextPage.value = response.hasNextPage ?? false
    hasPreviousPage.value = response.hasPreviousPage ?? false

    return response.items ?? []
  }

  function goTo(next) {
    const target = Math.min(Math.max(1, next), Math.max(1, totalPages.value))
    if (target === page.value) return false

    page.value = target
    return true
  }

  /** Filters change the result set, so the current page number is no longer meaningful. */
  function reset() {
    page.value = 1
  }

  return {
    page, size, totalCount, totalPages, hasNextPage, hasPreviousPage, isEmpty,
    apply, goTo, reset
  }
}
