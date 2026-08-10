<script setup>
import { ref, computed, onMounted } from 'vue'
import { motion } from 'motion-v'
import { creditCardsApi } from '@/api/creditCards.api'
import { useAsync } from '@/composables/useAsync'
import { useMoney } from '@/composables/useMoney'
import { useDashboardStore } from '@/stores/dashboard.store'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseSpinner from '@/components/base/BaseSpinner.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'

import CreditCardTile from '../components/CreditCardTile.vue'
import CardFormModal from '../components/CardFormModal.vue'
import CardPaymentModal from '../components/CardPaymentModal.vue'
import LimitModal from '../components/LimitModal.vue'

const { format } = useMoney()
const dashboard = useDashboardStore()

const { data: cards, loading, error, run: loadCards } = useAsync(creditCardsApi.list, { initialData: [] })

const formOpen = ref(false)
const editing = ref(null)
const payOpen = ref(false)
const limitOpen = ref(false)
const active = ref(null)

const totals = computed(() => {
  const byCurrency = new Map()

  for (const card of cards.value ?? []) {
    const entry = byCurrency.get(card.currency) ?? { limit: 0, used: 0, available: 0 }
    entry.limit += card.creditLimit
    entry.used += card.usedCredit
    entry.available += card.availableCredit
    byCurrency.set(card.currency, entry)
  }

  return [...byCurrency.entries()].map(([currency, value]) => ({ currency, ...value }))
})

function openCreate() {
  editing.value = null
  formOpen.value = true
}

function openEdit(card) {
  editing.value = card
  formOpen.value = true
}

function openPay(card) {
  active.value = card
  payOpen.value = true
}

function openLimit(card) {
  active.value = card
  limitOpen.value = true
}

function refresh() {
  loadCards()
  dashboard.invalidate()
}

onMounted(loadCards)
</script>

<template>
  <div>
    <PageHeader title="Credit cards" subtitle="Available credit is limit minus what you owe — always computed.">
      <template #actions>
        <BaseButton variant="primary" @click="openCreate">
          <template #icon><BaseIcon name="plus" :size="15" /></template>
          New card
        </BaseButton>
      </template>
    </PageHeader>

    <div v-if="totals.length" class="totals">
      <div v-for="entry in totals" :key="entry.currency" class="totals__item">
        <div>
          <span class="totals__label">Available credit</span>
          <p class="totals__value numeric">{{ format(entry.available, { currency: entry.currency }) }}</p>
        </div>
        <p class="totals__sub numeric">
          {{ format(entry.used, { currency: entry.currency }) }} owed of
          {{ format(entry.limit, { currency: entry.currency }) }}
        </p>
      </div>
    </div>

    <div v-if="loading && !cards?.length" class="state"><BaseSpinner :size="22" /></div>

    <BaseEmptyState
      v-else-if="error"
      icon="alert"
      title="Could not load your cards"
      :message="error.message"
    >
      <template #action><BaseButton variant="primary" @click="loadCards">Try again</BaseButton></template>
    </BaseEmptyState>

    <BaseEmptyState
      v-else-if="!cards?.length"
      icon="card"
      title="No credit cards yet"
      message="Add a card to track its balance, available credit and due date."
    >
      <template #action><BaseButton variant="primary" @click="openCreate">Add a card</BaseButton></template>
    </BaseEmptyState>

    <motion.div
      v-else
      class="grid"
      :initial="{ opacity: 0, y: 8 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.28, ease: [0.2, 0, 0, 1] }"
    >
      <CreditCardTile
        v-for="card in cards"
        :key="card.id"
        :card="card"
        @pay="openPay"
        @edit="openEdit"
        @limit="openLimit"
      />
    </motion.div>

    <CardFormModal v-model="formOpen" :card="editing" @saved="refresh" />
    <CardPaymentModal v-model="payOpen" :card="active" @saved="refresh" />
    <LimitModal v-model="limitOpen" :card="active" @saved="refresh" />
  </div>
</template>

<style scoped lang="scss">
.totals {
  display: flex;
  flex-wrap: wrap;
  gap: var(--sp-3);
  margin-bottom: var(--sp-5);
}

.totals__item {
  display: flex;
  flex-direction: column;
  gap: var(--sp-2);
  padding: var(--sp-3) var(--sp-4);
  background: var(--canvas-alt);
  border: var(--border-subtle);
  border-radius: var(--radius);
}

.totals__label {
  font-size: var(--fs-xs);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-muted);
}

.totals__value { font-size: var(--fs-lg); font-weight: var(--fw-semibold); }
.totals__sub { font-size: var(--fs-xs); color: var(--text-muted); }

.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
  gap: var(--sp-4);
}

.state { display: grid; place-items: center; padding: var(--sp-12); color: var(--text-muted); }

@media (max-width: 640px) {
  .grid { grid-template-columns: 1fr; }
}
</style>
