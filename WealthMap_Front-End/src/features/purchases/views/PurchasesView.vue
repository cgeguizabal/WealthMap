<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { purchasesApi, PURCHASE_CATEGORIES } from '@/api/purchases.api'
import { usePagination } from '@/composables/usePagination'
import { useMoney } from '@/composables/useMoney'
import { useToast } from '@/composables/useToast'
import { useDashboardStore } from '@/stores/dashboard.store'
import { useI18n } from '@/composables/useI18n'
import { useServerText } from '@/composables/useServerText'
import { useDoubleConfirm } from '@/composables/useDoubleConfirm'

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

const { t, locale } = useI18n()
const { format } = useMoney()
const toast = useToast()
const dashboard = useDashboardStore()
const confirmTwice = useDoubleConfirm()
const pagination = usePagination({ pageSize: 20 })

const purchases = ref([])
const loading = ref(false)
const formOpen = ref(false)
const editing = ref(null)

const now = new Date()
const filters = ref({ year: now.getFullYear(), month: now.getMonth() + 1, category: null })

const YEARS = Array.from({ length: 6 }, (_, i) => {
  const year = now.getFullYear() - i
  return { value: year, label: String(year) }
})

/**
 * Month names come from the browser's own calendar data for the active locale,
 * rather than a hand-kept list in every language.
 */
const MONTHS = computed(() =>
  Array.from({ length: 12 }, (_, index) => ({
    value: index + 1,
    label: new Date(2000, index, 1).toLocaleDateString(locale.value, { month: 'long' })
  }))
)

/**
 * The value stays the English name the API filters on; only the label is
 * translated, so switching language cannot change the query.
 */
const categoryOptions = computed(() =>
  PURCHASE_CATEGORIES.map((name) => ({ value: name, label: serverLabel('category', name) }))
)

/**
 * Computed, not a plain const: a const would call t() once during setup and
 * freeze the headers in whichever language was active at that moment, so
 * switching would leave the table in the old one.
 */
const COLUMNS = computed(() => [
  { key: 'occurredAt', label: t('common.date'), width: '130px' },
  { key: 'productName', label: t('purchases.item') },
  { key: 'storeName', label: t('purchases.store'), width: '160px' },
  { key: 'category', label: t('common.category'), width: '150px' },
  { key: 'paymentMethod', label: t('purchases.method'), width: '150px' },
  { key: 'amount', label: t('common.amount'), align: 'right', width: '130px' },
  { key: 'actions', label: '', align: 'right', width: '96px' }
])

const { label: serverLabel } = useServerText()

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

function openCreate() {
  editing.value = null
  formOpen.value = true
}

function openEdit(purchase) {
  editing.value = purchase
  formOpen.value = true
}

function onSaved() {
  applyFilters()
  dashboard.invalidate()
}

/**
 * Correcting a purchase moves money back, so the confirmation says which way and
 * how much rather than asking a generic "are you sure".
 */
async function remove(purchase) {
  const confirmed = await confirmTwice({
    title: t('purchases.deleteTitle'),
    message: t('purchases.deleteMessage', {
      name: purchase.productName,
      amount: format(purchase.amount, { currency: purchase.currency })
    }),
    secondMessage: t('purchases.deleteSecond')
  })

  if (!confirmed) return

  try {
    await purchasesApi.remove(purchase.id)
    toast.success(t('purchases.deletedToast', { name: purchase.productName }))
    onSaved()
  } catch (err) {
    // The server refuses when a card has been paid below the charge — that
    // message explains why, so it is shown rather than a generic failure.
    toast.error(err.message)
  }
}

watch(pagination.page, load)
onMounted(load)
</script>

<template>
  <motion.div v-bind="fadeUp()">
    <PageHeader :title="t('purchases.title')" :subtitle="t('purchases.subtitle')">
      <template #actions>
        <BaseButton variant="primary" @click="openCreate">
          <template #icon><BaseIcon name="plus" :size="15" /></template>
          {{ t('purchases.newPurchase') }}
        </BaseButton>
      </template>
    </PageHeader>

    <BaseCard :padded="false">
      <template #header>
        <div class="filters">
          <BaseSelect
            v-model="filters.year"
            :label="t('purchases.year')"
            :options="YEARS"
            :placeholder="t('purchases.allYears')"
          />

          <BaseSelect
            v-model="filters.month"
            :label="t('purchases.month')"
            :options="MONTHS"
            :placeholder="t('purchases.allMonths')"
            :disabled="!filters.year"
            :hint="!filters.year ? t('purchases.pickYearFirst') : ''"
          />

          <BaseSelect
            v-model="filters.category"
            :label="t('common.category')"
            :options="categoryOptions"
            :placeholder="t('purchases.allCategories')"
          />

          <div class="filters__actions">
            <BaseButton variant="secondary" @click="applyFilters">{{ t('purchases.apply') }}</BaseButton>
            <BaseButton variant="ghost" @click="clearFilters">{{ t('purchases.clear') }}</BaseButton>
          </div>
        </div>
      </template>

      <BaseTable
        :columns="COLUMNS"
        :rows="purchases"
        :loading="loading"
        :empty-title="t('purchases.emptyTitle')"
        :empty-message="t('purchases.emptyMessage')"
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
          <BaseBadge size="sm">{{ serverLabel('category', value) }}</BaseBadge>
        </template>

        <template #cell-paymentMethod="{ value }">
          <span class="method">
            <BaseIcon :name="METHOD_ICON[value] ?? 'receipt'" :size="14" />
            {{ serverLabel('paymentMethod', value) }}
          </span>
        </template>

        <template #cell-amount="{ row }">
          <span class="numeric amount">{{ format(row.amount, { currency: row.currency }) }}</span>
        </template>

        <!-- Icon-only, since the row is already dense. Both carry a title and an
             aria-label so the action is named for a pointer and a screen reader. -->
        <template #cell-actions="{ row }">
          <div class="row-actions">
            <BaseButton
              size="sm"
              variant="ghost"
              :title="t('common.edit')"
              :aria-label="t('purchases.editAria', { name: row.productName })"
              @click="openEdit(row)"
            >
              <template #icon><BaseIcon name="pencil" :size="14" /></template>
            </BaseButton>

            <BaseButton
              class="row-actions__delete"
              size="sm"
              variant="ghost"
              :title="t('common.delete')"
              :aria-label="t('purchases.deleteAria', { name: row.productName })"
              @click="remove(row)"
            >
              <template #icon><BaseIcon name="trash" :size="14" /></template>
            </BaseButton>
          </div>
        </template>
      </BaseTable>

      <template v-if="purchases.length" #footer>
        <div class="footer">
          <span class="footer__label">{{ t('purchases.thisPage') }}</span>
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

    <PurchaseFormModal v-model="formOpen" :purchase="editing" @saved="onSaved" />
  </motion.div>
</template>

<style scoped lang="scss" src="@/assets/styles/features/purchases/PurchasesView.scss"></style>
