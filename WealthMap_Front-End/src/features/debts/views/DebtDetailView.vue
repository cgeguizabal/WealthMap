<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import { debtsApi, DEBT_STATUS_VARIANT } from '@/api/debts.api'
import { useAsync } from '@/composables/useAsync'
import { useMoney } from '@/composables/useMoney'
import { useDashboardStore } from '@/stores/dashboard.store'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseProgress from '@/components/base/BaseProgress.vue'
import BaseSpinner from '@/components/base/BaseSpinner.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'

import DebtPaymentModal from '../components/DebtPaymentModal.vue'
import DebtFormModal from '../components/DebtFormModal.vue'
import PaymentsTable from '@/features/shared/components/PaymentsTable.vue'
import { useI18n } from '@/composables/useI18n'
import { useServerText } from '@/composables/useServerText'

const { t } = useI18n()
const { label: serverLabel } = useServerText()

const route = useRoute()
const { format } = useMoney()
const dashboard = useDashboardStore()

const debtId = route.params.id

const { data: debt, loading, error, run: loadDebt } = useAsync(() => debtsApi.get(debtId))
const { data: payments, loading: loadingPayments, run: loadPayments } =
  useAsync(() => debtsApi.payments(debtId), { initialData: [] })

const payOpen = ref(false)
const editOpen = ref(false)

const paid = computed(() =>
  debt.value ? debt.value.originalAmount - debt.value.remainingAmount : 0
)

function refresh() {
  loadDebt()
  loadPayments()
  dashboard.invalidate()
}

onMounted(() => {
  loadDebt()
  loadPayments()
})
</script>

<template>
  <div>
    <RouterLink to="/debts" class="back">
      <BaseIcon name="chevron-left" :size="15" />
      {{ t('debts.allDebts') }}
    </RouterLink>

    <div v-if="loading && !debt" class="state"><BaseSpinner :size="22" /></div>

    <BaseEmptyState
      v-else-if="error"
      icon="alert"
      :title="t('debts.notFound')"
      :message="t('common.notFoundHint')"
    >
      <template #action>
        <BaseButton variant="secondary" @click="$router.push('/debts')">{{ t('debts.backToDebts') }}</BaseButton>
      </template>
    </BaseEmptyState>

    <template v-else-if="debt">
      <PageHeader :title="debt.name">
        <template #subtitle>
          {{ t('composed.dueOnDay', { day: debt.monthlyDueDay }) }}
          <template v-if="debt.nextDueDate">{{ t('composed.nextDate', { date: debt.nextDueDate }) }}</template>
        </template>

        <template #actions>
          <BaseButton
            variant="primary"
            :disabled="debt.status === 'PaidOff'"
            @click="payOpen = true"
          >
            <template #icon><BaseIcon name="receipt" :size="15" /></template>
            {{ t('debts.registerPayment') }}
          </BaseButton>

          <BaseButton variant="ghost" @click="editOpen = true">
            <template #icon><BaseIcon name="pencil" :size="15" /></template>
            {{ t('common.edit') }}
          </BaseButton>
        </template>
      </PageHeader>

      <div class="summary">
        <div class="summary__top">
          <div>
            <span class="summary__label">{{ t('debts.remaining') }}</span>
            <p class="summary__value numeric">
              {{ format(debt.remainingAmount, { currency: debt.currency }) }}
            </p>
          </div>

          <BaseBadge :variant="DEBT_STATUS_VARIANT[debt.status] ?? 'neutral'">
            {{ serverLabel('debtStatus', debt.status) }}
          </BaseBadge>
        </div>

        <BaseProgress
          :value="paid"
          :max="debt.originalAmount"
          :variant="debt.status === 'PaidOff' ? 'positive' : 'accent'"
          :label="t('debts.repaidOf', {
            paid: format(paid, { currency: debt.currency }),
            total: format(debt.originalAmount, { currency: debt.currency })
          })"
        />

        <dl class="summary__meta">
          <div>
            <dt>{{ t('debts.original') }}</dt>
            <dd class="numeric">{{ format(debt.originalAmount, { currency: debt.currency }) }}</dd>
          </div>
          <div>
            <dt>{{ t('debts.monthlyPayment') }}</dt>
            <dd class="numeric">{{ format(debt.monthlyPayment, { currency: debt.currency }) }}</dd>
          </div>
          <div>
            <dt>{{ t('debts.dueDay') }}</dt>
            <dd class="numeric">{{ debt.monthlyDueDay }}</dd>
          </div>
        </dl>
      </div>

      <BaseCard :title="t('debts.payments')" :subtitle="t('debts.paymentsSubtitle')" :padded="false">
        <PaymentsTable
          :payments="payments ?? []"
          :loading="loadingPayments"
          :show-target="false"
          :empty-message="t('debts.paymentsEmpty')"
        />
      </BaseCard>

      <DebtPaymentModal v-model="payOpen" :debt="debt" @saved="refresh" />
      <DebtFormModal v-model="editOpen" :debt="debt" @saved="refresh" />
    </template>
  </div>
</template>

<style scoped lang="scss" src="./DebtDetailView.scss"></style>
