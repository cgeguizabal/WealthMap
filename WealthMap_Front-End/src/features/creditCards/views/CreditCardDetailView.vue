<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter, RouterLink } from 'vue-router'
import { creditCardsApi } from '@/api/creditCards.api'
import { purchasesApi } from '@/api/purchases.api'
import { installmentsApi } from '@/api/installments.api'
import { useAsync } from '@/composables/useAsync'
import { useMoney } from '@/composables/useMoney'
import { useDashboardStore } from '@/stores/dashboard.store'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseTable from '@/components/base/BaseTable.vue'
import BaseTabs from '@/components/base/BaseTabs.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseProgress from '@/components/base/BaseProgress.vue'
import BaseSpinner from '@/components/base/BaseSpinner.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'
import BaseTimestamp from '@/components/base/BaseTimestamp.vue'

import CardFormModal from '../components/CardFormModal.vue'
import CardPaymentModal from '../components/CardPaymentModal.vue'
import LimitModal from '../components/LimitModal.vue'
import PaymentsTable from '@/features/shared/components/PaymentsTable.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const route = useRoute()
const router = useRouter()
const { format, formatPercent } = useMoney()
const dashboard = useDashboardStore()

const cardId = route.params.id

const { data: card, loading, error, run: loadCard } = useAsync(() => creditCardsApi.get(cardId))
const { data: payments, loading: loadingPayments, run: loadPayments } =
  useAsync(() => creditCardsApi.payments(cardId), { initialData: [] })

/** The API filters by card, so this is every purchase charged to it. */
const { data: purchasePage, loading: loadingCharges, run: loadPurchases } =
  useAsync(() => purchasesApi.list({ creditCardId: cardId, page: 1, pageSize: 100 }))
const { data: plans, run: loadPlans } = useAsync(installmentsApi.list, { initialData: [] })

const payOpen = ref(false)
const editOpen = ref(false)
const limitOpen = ref(false)
const tab = ref('charges')

/** Computed so the headers follow the language selector rather than freezing. */
const CHARGE_COLUMNS = computed(() => [
  { key: 'occurredAt', label: t('common.date'), width: '130px' },
  { key: 'name', label: t('purchases.item') },
  { key: 'kind', label: t('common.type'), width: '160px' },
  { key: 'amount', label: t('cards.charged'), align: 'right', width: '130px' }
])

/** Purchases and installment plans both put debt on the card, so both belong here. */
const charges = computed(() => {
  const fromPurchases = (purchasePage.value?.items ?? [])
    .map((purchase) => ({
      id: purchase.id,
      kind: 'Purchase',
      meta: purchase.category,
      name: purchase.productName,
      amount: purchase.amount,
      currency: purchase.currency,
      occurredAt: purchase.occurredAt
    }))

  const fromPlans = (plans.value ?? [])
    .filter((plan) => plan.creditCardId === cardId)
    .map((plan) => ({
      id: plan.id,
      kind: 'Installment plan',
      meta: `${plan.monthsCount} months · ${plan.remainingMonths} left`,
      name: plan.productName,
      // The full price hits the card on day one, which is what created the debt.
      amount: plan.totalPrice,
      currency: plan.currency,
      occurredAt: plan.purchasedAt
    }))

  return [...fromPurchases, ...fromPlans]
    .sort((a, b) => new Date(b.occurredAt) - new Date(a.occurredAt))
})

const tabs = computed(() => [
  { value: 'charges', label: t('cards.charges'), count: charges.value.length },
  { value: 'payments', label: t('cards.payments'), count: payments.value?.length ?? 0 }
])

const utilisation = computed(() => {
  if (!card.value?.creditLimit) return 0
  return (card.value.usedCredit / card.value.creditLimit) * 100
})

const variant = computed(() => {
  if (utilisation.value >= 80) return 'negative'
  if (utilisation.value >= 50) return 'warning'
  return 'accent'
})

/** Purchases have no detail screen, so only plans are navigable. */
function isPlan(row) {
  return row.kind === 'Installment plan'
}

function openCharge(row) {
  if (isPlan(row)) router.push(`/installments/${row.id}`)
}

function refresh() {
  loadCard()
  loadPayments()
  loadPurchases()
  loadPlans()
  dashboard.invalidate()
}

onMounted(() => {
  loadCard()
  loadPayments()
  loadPurchases()
  loadPlans()
})
</script>

<template>
  <div>
    <RouterLink to="/credit-cards" class="back">
      <BaseIcon name="chevron-left" :size="15" />
      All cards
    </RouterLink>

    <div v-if="loading && !card" class="state"><BaseSpinner :size="22" /></div>

    <BaseEmptyState
      v-else-if="error"
      icon="alert"
      :title="t('cards.notFound')"
      :message="t('common.notFoundHint')"
    >
      <template #action>
        <BaseButton variant="secondary" @click="$router.push('/credit-cards')">{{ t('cards.backToCards') }}</BaseButton>
      </template>
    </BaseEmptyState>

    <template v-else-if="card">
      <PageHeader :title="card.cardName" :subtitle="card.bankName">
        <template #actions>
          <BaseButton variant="primary" :disabled="card.usedCredit <= 0" @click="payOpen = true">
            <template #icon><BaseIcon name="receipt" :size="15" /></template>
            Register payment
          </BaseButton>
          <BaseButton variant="secondary" @click="limitOpen = true">{{ t('cards.limit') }}</BaseButton>
          <BaseButton variant="ghost" @click="editOpen = true">
            <template #icon><BaseIcon name="pencil" :size="15" /></template>
            Edit
          </BaseButton>
        </template>
      </PageHeader>

      <div class="summary">
        <div class="summary__figures">
          <div class="summary__figure">
            <span class="summary__label">{{ t('cards.available') }}</span>
            <p class="summary__value numeric is-positive">
              {{ format(card.availableCredit, { currency: card.currency }) }}
            </p>
          </div>

          <div class="summary__figure">
            <span class="summary__label">{{ t('cards.owed') }}</span>
            <p class="summary__value numeric">{{ format(card.usedCredit, { currency: card.currency }) }}</p>
          </div>

          <div class="summary__figure">
            <span class="summary__label">{{ t('cards.limit') }}</span>
            <p class="summary__value numeric">{{ format(card.creditLimit, { currency: card.currency }) }}</p>
          </div>
        </div>

        <BaseProgress
          :value="card.usedCredit"
          :max="card.creditLimit"
          :variant="variant"
          :label="`${utilisation.toFixed(0)}% of your limit in use`"
        />

        <dl class="summary__meta">
          <div><dt>{{ t('cards.dueDay') }}</dt><dd class="numeric">{{ card.paymentDueDay }}</dd></div>
          <div><dt>{{ t('cards.statementCutoff') }}</dt><dd class="numeric">{{ card.statementCutoffDay }}</dd></div>
          <div><dt>{{ t('cards.interest') }}</dt><dd class="numeric">{{ formatPercent(card.annualInterestRate, 2) }}</dd></div>
        </dl>

        <p v-if="card.notes" class="summary__notes">{{ card.notes }}</p>
      </div>

      <BaseTabs v-model="tab" :tabs="tabs" class="tabs" />

      <BaseCard v-if="tab === 'charges'" :padded="false">
        <BaseTable
          :columns="CHARGE_COLUMNS"
          :rows="charges"
          :loading="loadingCharges"
          :clickable="isPlan"
          :empty-title="t('cards.noChargesTitle')"
          :empty-message="t('cards.noChargesMessage')"
          @row-click="openCharge"
        >
          <template #cell-occurredAt="{ value }">
            <BaseTimestamp :value="value" />
          </template>

          <!--
            Only plans have somewhere to go: a purchase has no detail screen, so
            making the whole row clickable would promise navigation that half the
            rows cannot deliver.
          -->
          <template #cell-name="{ row }">
            <div class="cell-stack">
              <!-- The row handles the click; the link keeps it reachable by keyboard.
                   `.stop` prevents both firing for the same activation. -->
              <RouterLink
                v-if="isPlan(row)"
                :to="`/installments/${row.id}`"
                class="cell-stack__link"
                @click.stop
              >
                {{ row.name }}
                <BaseIcon name="arrow-up-right" :size="13" />
              </RouterLink>

              <span v-else class="cell-stack__title">{{ row.name }}</span>

              <span class="cell-stack__sub">{{ row.meta }}</span>
            </div>
          </template>

          <template #cell-kind="{ value }">
            <BaseBadge :variant="value === 'Purchase' ? 'neutral' : 'accent'" size="sm">
              {{ value }}
            </BaseBadge>
          </template>

          <template #cell-amount="{ row }">
            <span class="numeric charge">{{ format(row.amount, { currency: row.currency }) }}</span>
          </template>
        </BaseTable>

        <template v-if="purchasePage && purchasePage.totalCount > purchasePage.items.length" #footer>
          <span class="footnote">
            {{ t('composed.showingRecent', {
              shown: purchasePage.items.length,
              total: purchasePage.totalCount
            }) }}
          </span>
        </template>
      </BaseCard>

      <BaseCard v-else :padded="false">
        <PaymentsTable
          :payments="payments ?? []"
          :loading="loadingPayments"
          :show-target="false"
          :empty-message="t('cards.noPaymentsMessage')"
        />
      </BaseCard>

      <CardPaymentModal v-model="payOpen" :card="card" @saved="refresh" />
      <CardFormModal v-model="editOpen" :card="card" @saved="refresh" />
      <LimitModal v-model="limitOpen" :card="card" @saved="refresh" />
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

.summary__figures {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--sp-4);
}

.summary__label {
  font-size: var(--fs-xs);
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--text-muted);
}

.summary__value {
  font-size: var(--fs-xl);
  font-weight: var(--fw-semibold);
  letter-spacing: -0.02em;

  &.is-positive { color: var(--positive); }
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

.summary__notes { font-size: var(--fs-sm); color: var(--text-muted); }

.tabs { margin-bottom: var(--sp-4); }

.muted { color: var(--text-muted); font-size: var(--fs-sm); }
.charge { font-weight: var(--fw-semibold); color: var(--negative); }

.cell-stack { display: flex; flex-direction: column; align-items: flex-start; }
.cell-stack__title { font-weight: var(--fw-medium); }
.cell-stack__sub { font-size: var(--fs-xs); color: var(--text-muted); }

.cell-stack__link {
  display: inline-flex;
  align-items: center;
  gap: var(--sp-1);

  font-weight: var(--fw-medium);
  color: var(--accent);

  @include focus-ring;

  &:hover { text-decoration: underline; }
}

.footnote { font-size: var(--fs-xs); color: var(--text-muted); }

.state { display: grid; place-items: center; padding: var(--sp-12); color: var(--text-muted); }

@media (max-width: 640px) {
  .summary__figures { grid-template-columns: 1fr; gap: var(--sp-3); }
  .summary__value { font-size: var(--fs-lg); }
}
</style>
