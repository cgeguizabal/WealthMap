<script setup>
import { ref, computed, onMounted } from 'vue'
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { debtsApi } from '@/api/debts.api'
import { useAsync } from '@/composables/useAsync'
import { useMoney } from '@/composables/useMoney'
import { useToast } from '@/composables/useToast'
import { useUiStore } from '@/stores/ui.store'
import { useDashboardStore } from '@/stores/dashboard.store'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'
import CardGridSkeleton from '@/features/shared/components/CardGridSkeleton.vue'

import DebtCard from '../components/DebtCard.vue'
import DebtFormModal from '../components/DebtFormModal.vue'
import DebtPaymentModal from '../components/DebtPaymentModal.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const { format } = useMoney()
const toast = useToast()
const ui = useUiStore()
const dashboard = useDashboardStore()

const { data: debts, loading, error, run: loadDebts } = useAsync(debtsApi.list, { initialData: [] })

const formOpen = ref(false)
const payOpen = ref(false)
const editing = ref(null)
const active = ref(null)

const outstanding = computed(() => {
  const byCurrency = new Map()

  for (const debt of debts.value ?? []) {
    if (debt.status === 'PaidOff') continue
    byCurrency.set(debt.currency, (byCurrency.get(debt.currency) ?? 0) + debt.remainingAmount)
  }

  return [...byCurrency.entries()].map(([currency, total]) => ({ currency, total }))
})

function openCreate() {
  editing.value = null
  formOpen.value = true
}

function openEdit(debt) {
  editing.value = debt
  formOpen.value = true
}

function openPay(debt) {
  active.value = debt
  payOpen.value = true
}

async function markDefaulted(debt) {
  const confirmed = await ui.confirm({
    title: t('debts.markDefaultedTitle', { name: debt.name }),
    message: t('debts.markDefaultedMessage'),
    confirmLabel: t('debts.markDefaulted'),
    variant: 'danger'
  })

  if (!confirmed) return

  try {
    await debtsApi.markDefaulted(debt.id)
    toast.warning(`${debt.name} marked as defaulted.`)
    refresh()
  } catch (err) {
    toast.error(err.message)
  }
}

async function remove(debt) {
  const confirmed = await ui.confirm({
    title: t('debts.deleteTitle', { name: debt.name }),
    message: t('debts.deleteMessage'),
    confirmLabel: t('common.delete'),
    variant: 'danger'
  })

  if (!confirmed) return

  try {
    await debtsApi.remove(debt.id)
    toast.success(t('debts.deleted', { name: debt.name }))
    refresh()
  } catch (err) {
    toast.error(err.message)
  }
}

function refresh() {
  loadDebts()
  dashboard.invalidate()
}

onMounted(loadDebts)
</script>

<template>
  <div>
    <PageHeader :title="t('debts.title')" :subtitle="t('debts.subtitle')">
      <template #actions>
        <BaseButton variant="primary" @click="openCreate">
          <template #icon><BaseIcon name="plus" :size="15" /></template>
          {{ t('debts.newDebt') }}
        </BaseButton>
      </template>
    </PageHeader>

    <div v-if="outstanding.length" class="totals">
      <div v-for="entry in outstanding" :key="entry.currency" class="totals__item">
        <span class="totals__label">{{ t('debts.stillOwed') }}</span>
        <span class="totals__value numeric">{{ format(entry.total, { currency: entry.currency }) }}</span>
      </div>
    </div>

    <CardGridSkeleton v-if="loading && !debts?.length" />

    <BaseEmptyState
      v-else-if="error"
      icon="alert"
      :title="t('debts.loadFailed')"
      :message="error.message"
    >
      <template #action><BaseButton variant="primary" @click="loadDebts">{{ t('common.tryAgain') }}</BaseButton></template>
    </BaseEmptyState>

    <BaseEmptyState
      v-else-if="!debts?.length"
      icon="debt"
      :title="t('debts.emptyTitle')"
      :message="t('debts.emptyMessage')"
    >
      <template #action><BaseButton variant="primary" @click="openCreate">{{ t('debts.addFirst') }}</BaseButton></template>
    </BaseEmptyState>

    <motion.div
      v-else
      class="grid"
      v-bind="fadeUp()"
    >
      <DebtCard
        v-for="debt in debts"
        :key="debt.id"
        :debt="debt"
        @pay="openPay"
        @edit="openEdit"
        @default="markDefaulted"
        @delete="remove"
      />
    </motion.div>

    <DebtFormModal v-model="formOpen" :debt="editing" @saved="refresh" />
    <DebtPaymentModal v-model="payOpen" :debt="active" @saved="refresh" />
  </div>
</template>

<style scoped lang="scss" src="@/assets/styles/features/debts/DebtsView.scss"></style>
