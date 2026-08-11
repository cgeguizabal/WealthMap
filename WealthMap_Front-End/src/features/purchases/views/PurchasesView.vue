<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { purchasesApi, PURCHASE_CATEGORIES } from '@/api/purchases.api'
import { usePagination } from '@/composables/usePagination'
import { useMoney } from '@/composables/useMoney'
import { useToast } from '@/composables/useToast'
import { useDashboardStore } from '@/stores/dashboard.store'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseTable from '@/components/base/BaseTable.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BasePagination from '@/components/base/BasePagination.vue'
import BaseTimestamp from '@/components/base/BaseTimestamp.vue'

import PurchaseFormModal from '../components/PurchaseFormModal.vue'

const { format } = useMoney()
const toast = useToast()
const dashboard = useDashboardStore()
const pagination = usePagination({ pageSize: 20 })

const purchases = ref([])
const loading = ref(false)
const formOpen = ref(false)

const now = new Date()
const filters = ref({ year: now.getFullYear(), month: now.getMonth() + 1, category: null })

const YEARS = Array.from({ length: 6 }, (_, i) => {
  const year = now.getFullYear() - i
  return { value: year, label: String(year) }
})

const MONTHS = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December'
].map((label, index) => ({ value: index + 1, label }))

const categoryOptions = PURCHASE_CATEGORIES.map((name) => ({ value: name, label: name }))

const COLUMNS = [
  { key: 'occurredAt', label: 'Date', width: '130px' },
  { key: 'productName', label: 'Item' },
  { key: 'storeName', label: 'Store', width: '160px' },
  { key: 'category', label: 'Category', width: '150px' },
  { key: 'paymentMethod', label: 'Method', width: '150px' },
  { key: 'amount', label: 'Amount', align: 'right', width: '130px' }
]

const METHOD_LABEL = { DebitAccount: 'Debit', CreditCard: 'Credit card', Cash: 'Cash' }
const METHOD_ICON = { DebitAccount: 'wallet', CreditCard: 'card', Cash: 'receipt' }

/** Only meaningful when the page shows a single month's worth of one currency. */
const pageTotal = computed(() =>
  purchases.value.reduce((sum, purchase) => sum + purchase.amount, 0)
)

async function load() {
  loading.value = true

  try {
    const response = await purchasesApi.list({
      page: pagination.page.value,
      pageSize: pagination.size.value,
      // The API requires a year whenever a month is given.
      year: filters.value.year ?? undefined,
      month: filters.value.year ? filters.value.month ?? undefined : undefined,
      category: filters.value.category ?? undefined
    })
    purchases.value = pagination.apply(response)
  } catch (err) {
    toast.error(err.message)
    purchases.value = []
  } finally {
    loading.value = false
  }
}

function applyFilters() {
  pagination.reset()
  load()
}

function clearFilters() {
  filters.value = { year: null, month: null, category: null }
  applyFilters()
}

function onSaved() {
  applyFilters()
  dashboard.invalidate()
}

watch(pagination.page, load)
onMounted(load)
</script>

<template>
  <div>
    <PageHeader title="Purchases" subtitle="Everything you have bought, however you paid for it.">
      <template #actions>
        <BaseButton variant="primary" @click="formOpen = true">
          <template #icon><BaseIcon name="plus" :size="15" /></template>
          Record purchase
        </BaseButton>
      </template>
    </PageHeader>

    <BaseCard :padded="false">
      <template #header>
        <div class="filters">
          <BaseSelect v-model="filters.year" label="Year" :options="YEARS" placeholder="All years" />

          <BaseSelect
            v-model="filters.month"
            label="Month"
            :options="MONTHS"
            placeholder="All months"
            :disabled="!filters.year"
            :hint="!filters.year ? 'Pick a year first' : ''"
          />

          <BaseSelect
            v-model="filters.category"
            label="Category"
            :options="categoryOptions"
            placeholder="All categories"
          />

          <div class="filters__actions">
            <BaseButton variant="secondary" @click="applyFilters">Apply</BaseButton>
            <BaseButton variant="ghost" @click="clearFilters">Clear</BaseButton>
          </div>
        </div>
      </template>

      <BaseTable
        :columns="COLUMNS"
        :rows="purchases"
        :loading="loading"
        empty-title="No purchases found"
        empty-message="Nothing matches these filters — or nothing has been recorded yet."
      >
        <template #cell-occurredAt="{ value }">
          <BaseTimestamp :value="value" :with-year="false" />
        </template>

        <template #cell-productName="{ row }">
          <div class="item">
            <span class="item__name">{{ row.productName }}</span>
            <span v-if="row.notes" class="item__notes">{{ row.notes }}</span>
          </div>
        </template>

        <!-- Cash purchases often name no store, so the dash is the normal case
             here rather than a sign of missing data. -->
        <template #cell-storeName="{ value }">
          <span v-if="value" class="store">
            <BaseIcon name="store" :size="13" />
            <span class="store__name">{{ value }}</span>
          </span>
          <span v-else class="store store--none">—</span>
        </template>

        <template #cell-category="{ value }">
          <BaseBadge size="sm">{{ value }}</BaseBadge>
        </template>

        <template #cell-paymentMethod="{ value }">
          <span class="method">
            <BaseIcon :name="METHOD_ICON[value] ?? 'receipt'" :size="14" />
            {{ METHOD_LABEL[value] ?? value }}
          </span>
        </template>

        <template #cell-amount="{ row }">
          <span class="numeric amount">{{ format(row.amount, { currency: row.currency }) }}</span>
        </template>
      </BaseTable>

      <template v-if="purchases.length" #footer>
        <div class="footer">
          <span class="footer__label">This page</span>
          <span class="numeric footer__total">{{ format(pageTotal) }}</span>
        </div>
      </template>
    </BaseCard>

    <BasePagination
      :page="pagination.page.value"
      :page-size="pagination.size.value"
      :total-count="pagination.totalCount.value"
      :total-pages="pagination.totalPages.value"
      :has-next-page="pagination.hasNextPage.value"
      :has-previous-page="pagination.hasPreviousPage.value"
      @update:page="pagination.goTo($event)"
    />

    <PurchaseFormModal v-model="formOpen" @saved="onSaved" />
  </div>
</template>

<style scoped lang="scss">
.filters {
  display: grid;
  grid-template-columns: 130px 160px 180px auto;
  align-items: end;
  gap: var(--sp-3);
  width: 100%;
}

.filters__actions {
  display: flex;
  gap: var(--sp-2);
  justify-content: flex-end;
}

.muted { color: var(--text-muted); font-size: var(--fs-sm); }

.item { display: flex; flex-direction: column; }
.item__name { font-weight: var(--fw-medium); }
.item__notes { font-size: var(--fs-xs); color: var(--text-muted); }

.store {
  display: inline-flex;
  align-items: center;
  gap: var(--sp-2);
  min-width: 0;
  font-size: var(--fs-sm);
  color: var(--text-muted);
}

/* The icon must not shrink away when a long store name truncates. */
.store :deep(svg) { flex: none; }
.store__name { @include truncate; }

.store--none { color: var(--text-muted); opacity: 0.6; }

.method {
  display: inline-flex;
  align-items: center;
  gap: var(--sp-2);
  font-size: var(--fs-sm);
  color: var(--text-muted);
}

.amount { font-weight: var(--fw-semibold); }

.footer {
  display: flex;
  align-items: baseline;
  justify-content: flex-end;
  gap: var(--sp-3);
}

.footer__label { font-size: var(--fs-sm); color: var(--text-muted); }
.footer__total { font-size: var(--fs-md); font-weight: var(--fw-semibold); }

@media (max-width: 900px) {
  .filters { grid-template-columns: 1fr 1fr; }
  .filters__actions { grid-column: span 2; justify-content: flex-start; }
}

@media (max-width: 480px) {
  .filters { grid-template-columns: 1fr; }
  .filters__actions { grid-column: span 1; }
}
</style>
