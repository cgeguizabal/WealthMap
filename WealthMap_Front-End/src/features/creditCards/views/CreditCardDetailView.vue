<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import { creditCardsApi } from '@/api/creditCards.api'
import { useAsync } from '@/composables/useAsync'
import { useMoney } from '@/composables/useMoney'
import { useDashboardStore } from '@/stores/dashboard.store'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseProgress from '@/components/base/BaseProgress.vue'
import BaseSpinner from '@/components/base/BaseSpinner.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'

import CardFormModal from '../components/CardFormModal.vue'
import CardPaymentModal from '../components/CardPaymentModal.vue'
import LimitModal from '../components/LimitModal.vue'
import PaymentsTable from '@/features/shared/components/PaymentsTable.vue'

const route = useRoute()
const { format, formatPercent } = useMoney()
const dashboard = useDashboardStore()

const cardId = route.params.id

const { data: card, loading, error, run: loadCard } = useAsync(() => creditCardsApi.get(cardId))
const { data: payments, loading: loadingPayments, run: loadPayments } =
  useAsync(() => creditCardsApi.payments(cardId), { initialData: [] })

const payOpen = ref(false)
const editOpen = ref(false)
const limitOpen = ref(false)

const utilisation = computed(() => {
  if (!card.value?.creditLimit) return 0
  return (card.value.usedCredit / card.value.creditLimit) * 100
})

const variant = computed(() => {
  if (utilisation.value >= 80) return 'negative'
  if (utilisation.value >= 50) return 'warning'
  return 'accent'
})

function refresh() {
  loadCard()
  loadPayments()
  dashboard.invalidate()
}

onMounted(() => {
  loadCard()
  loadPayments()
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
      title="Card not found"
      message="It may have been removed, or it is not yours."
    >
      <template #action>
        <BaseButton variant="secondary" @click="$router.push('/credit-cards')">Back to cards</BaseButton>
      </template>
    </BaseEmptyState>

    <template v-else-if="card">
      <PageHeader :title="card.cardName" :subtitle="card.bankName">
        <template #actions>
          <BaseButton variant="primary" :disabled="card.usedCredit <= 0" @click="payOpen = true">
            <template #icon><BaseIcon name="receipt" :size="15" /></template>
            Register payment
          </BaseButton>
          <BaseButton variant="secondary" @click="limitOpen = true">Limit</BaseButton>
          <BaseButton variant="ghost" @click="editOpen = true">
            <template #icon><BaseIcon name="pencil" :size="15" /></template>
            Edit
          </BaseButton>
        </template>
      </PageHeader>

      <div class="summary">
        <div class="summary__figures">
          <div class="summary__figure">
            <span class="summary__label">Available</span>
            <p class="summary__value numeric is-positive">
              {{ format(card.availableCredit, { currency: card.currency }) }}
            </p>
          </div>

          <div class="summary__figure">
            <span class="summary__label">Owed</span>
            <p class="summary__value numeric">{{ format(card.usedCredit, { currency: card.currency }) }}</p>
          </div>

          <div class="summary__figure">
            <span class="summary__label">Limit</span>
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
          <div><dt>Due day</dt><dd class="numeric">{{ card.paymentDueDay }}</dd></div>
          <div><dt>Statement cutoff</dt><dd class="numeric">{{ card.statementCutoffDay }}</dd></div>
          <div><dt>Interest</dt><dd class="numeric">{{ formatPercent(card.annualInterestRate, 2) }}</dd></div>
        </dl>

        <p v-if="card.notes" class="summary__notes">{{ card.notes }}</p>
      </div>

      <BaseCard
        title="Payments"
        subtitle="Every payment against this card, from any source"
        :padded="false"
      >
        <PaymentsTable
          :payments="payments ?? []"
          :loading="loadingPayments"
          :show-target="false"
          empty-message="Once you register a payment it appears here — including cash payments, which touch no account."
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

.state { display: grid; place-items: center; padding: var(--sp-12); color: var(--text-muted); }

@media (max-width: 640px) {
  .summary__figures { grid-template-columns: 1fr; gap: var(--sp-3); }
  .summary__value { font-size: var(--fs-lg); }
}
</style>
