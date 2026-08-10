<script setup>
import { ref, watch, onMounted } from 'vue'
import { paymentsApi, PAYMENT_TARGET_OPTIONS } from '@/api/payments.api'
import { usePagination } from '@/composables/usePagination'
import { useToast } from '@/composables/useToast'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BasePagination from '@/components/base/BasePagination.vue'
import PaymentsTable from '@/features/shared/components/PaymentsTable.vue'

const toast = useToast()
const pagination = usePagination({ pageSize: 20 })

const payments = ref([])
const loading = ref(false)

const filters = ref({ from: '', to: '', targetType: null })

async function load() {
  loading.value = true

  try {
    const response = await paymentsApi.list({
      page: pagination.page.value,
      pageSize: pagination.size.value,
      from: filters.value.from || undefined,
      to: filters.value.to || undefined,
      targetType: filters.value.targetType || undefined
    })
    payments.value = pagination.apply(response)
  } catch (err) {
    toast.error(err.message)
    payments.value = []
  } finally {
    loading.value = false
  }
}

/** A filter change makes the current page number meaningless. */
function applyFilters() {
  pagination.reset()
  load()
}

function clearFilters() {
  filters.value = { from: '', to: '', targetType: null }
  applyFilters()
}

watch(pagination.page, load)
onMounted(load)
</script>

<template>
  <div>
    <PageHeader
      title="Payments"
      subtitle="Everything you have paid against cards, debts and installment plans — whatever the money came from."
    />

    <BaseCard :padded="false">
      <template #header>
        <div class="filters">
          <BaseSelect
            v-model="filters.targetType"
            :options="PAYMENT_TARGET_OPTIONS"
            placeholder="All types"
            label="Type"
          />

          <BaseInput v-model="filters.from" label="From" type="date" />
          <BaseInput v-model="filters.to" label="To" type="date" />

          <div class="filters__actions">
            <BaseButton variant="secondary" @click="applyFilters">Apply</BaseButton>
            <BaseButton variant="ghost" @click="clearFilters">Clear</BaseButton>
          </div>
        </div>
      </template>

      <PaymentsTable :payments="payments" :loading="loading" />

      <BasePagination
        :page="pagination.page.value"
        :page-size="pagination.size.value"
        :total-count="pagination.totalCount.value"
        :total-pages="pagination.totalPages.value"
        :has-next-page="pagination.hasNextPage.value"
        :has-previous-page="pagination.hasPreviousPage.value"
        @update:page="pagination.goTo($event)"
      />
    </BaseCard>
  </div>
</template>

<style scoped lang="scss">
.filters {
  display: grid;
  grid-template-columns: 180px 160px 160px auto;
  align-items: end;
  gap: var(--sp-3);
  width: 100%;
}

.filters__actions {
  display: flex;
  gap: var(--sp-2);
  justify-content: flex-end;
}

@media (max-width: 900px) {
  .filters { grid-template-columns: 1fr 1fr; }
  .filters__actions { grid-column: span 2; justify-content: flex-start; }
}

@media (max-width: 480px) {
  .filters { grid-template-columns: 1fr; }
  .filters__actions { grid-column: span 1; }
}
</style>
