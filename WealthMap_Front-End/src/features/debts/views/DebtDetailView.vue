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

const { t } = useI18n()

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
      All debts
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
          Due on day {{ debt.monthlyDueDay }} each month
          <template v-if="debt.nextDueDate"> · next {{ debt.nextDueDate }}</template>
        </template>

        <template #actions>
          <BaseButton
            variant="primary"
            :disabled="debt.status === 'PaidOff'"
            @click="payOpen = true"
          >
            <template #icon><BaseIcon name="receipt" :size="15" /></template>
            Register payment
          </BaseButton>

          <BaseButton variant="ghost" @click="editOpen = true">
            <template #icon><BaseIcon name="pencil" :size="15" /></template>
            Edit
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
            {{ debt.status === 'PaidOff' ? 'Paid off' : debt.status }}
          </BaseBadge>
        </div>

        <BaseProgress
          :value="paid"
          :max="debt.originalAmount"
          :variant="debt.status === 'PaidOff' ? 'positive' : 'accent'"
          :label="`${format(paid, { currency: debt.currency })} of ${format(debt.originalAmount, { currency: debt.currency })} repaid`"
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

<style scoped lang="scss">
.back {
  display: inline-flex;
  align-items: center;
  gap: var(--sp-1);
  margin-bottom: var(--sp-4);
  font-size: var(--fs-sm);
  color: var(--text-muted);

  &:hover { color: var(--text); text-decoration: none; }
}

.summary {
  display: flex;
  flex-direction: column;
  gap: var(--sp-5);

  padding: var(--sp-5);
  margin-bottom: var(--sp-5);

  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
}

.summary__top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--sp-4);
}

.summary__label {
  font-size: var(--fs-xs);
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--text-muted);
}

.summary__value {
  font-size: var(--fs-2xl);
  font-weight: var(--fw-semibold);
  letter-spacing: -0.02em;
}

.summary__meta {
  display: flex;
  flex-wrap: wrap;
  gap: var(--sp-6);
  padding-top: var(--sp-4);
  border-top: var(--border-subtle);

  dt { font-size: var(--fs-xs); color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.05em; }
  dd { font-size: var(--fs-base); font-weight: var(--fw-medium); }
}

.state { display: grid; place-items: center; padding: var(--sp-12); color: var(--text-muted); }
</style>
