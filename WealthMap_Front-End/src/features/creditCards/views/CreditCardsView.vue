<script setup>
import { ref, computed, onMounted } from 'vue'
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { creditCardsApi } from '@/api/creditCards.api'
import { useAsync } from '@/composables/useAsync'
import { useMoney } from '@/composables/useMoney'
import { useToast } from '@/composables/useToast'
import { useDoubleConfirm } from '@/composables/useDoubleConfirm'
import { useI18n } from '@/composables/useI18n'
import { useDashboardStore } from '@/stores/dashboard.store'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'
import CardGridSkeleton from '@/features/shared/components/CardGridSkeleton.vue'

import CreditCardTile from '../components/CreditCardTile.vue'
import CardFormModal from '../components/CardFormModal.vue'
import CardPaymentModal from '../components/CardPaymentModal.vue'
import LimitModal from '../components/LimitModal.vue'

const { t } = useI18n()
const { format } = useMoney()
const toast = useToast()
const confirmTwice = useDoubleConfirm()
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

/**
 * Deleting archives: the card leaves every list and total, but its purchases,
 * installment plans and payments stay on record. An outstanding balance is
 * called out by name — archiving it does not settle it.
 */
async function remove(card) {
  const owedNote = card.usedCredit > 0
    ? ` It still has ${format(card.usedCredit, { currency: card.currency })} owed, ` +
      'and deleting it will not pay that off.'
    : ''

  const confirmed = await confirmTwice({
    title: t('cards.deleteTitle', { name: card.cardName }),
    message:
      `${card.cardName} will be removed from your cards and your available ` +
      `credit.${owedNote} Its purchases, installment plans and payments are kept.`,
    secondMessage:
      `This removes ${card.cardName} from WealthMap. You will not be able to ` +
      'charge purchases to it or record payments against it again.'
  })

  if (!confirmed) return

  try {
    await creditCardsApi.remove(card.id)
    toast.success(t('cards.deleted', { name: card.cardName }))
    refresh()
  } catch (err) {
    toast.error(err.message)
  }
}

function refresh() {
  loadCards()
  dashboard.invalidate()
}

onMounted(loadCards)
</script>

<template>
  <div>
    <PageHeader :title="t('cards.title')" :subtitle="t('cards.subtitle')">
      <template #actions>
        <BaseButton variant="primary" @click="openCreate">
          <template #icon><BaseIcon name="plus" :size="15" /></template>
          {{ t('cards.newCard') }}
        </BaseButton>
      </template>
    </PageHeader>

    <div v-if="totals.length" class="totals">
      <div v-for="entry in totals" :key="entry.currency" class="totals__item">
        <div>
          <span class="totals__label">{{ t('cards.availableCredit') }}</span>
          <p class="totals__value numeric">{{ format(entry.available, { currency: entry.currency }) }}</p>
        </div>
        <p class="totals__sub numeric">
          {{ format(entry.used, { currency: entry.currency }) }} owed of
          {{ format(entry.limit, { currency: entry.currency }) }}
        </p>
      </div>
    </div>

    <CardGridSkeleton v-if="loading && !cards?.length" />

    <BaseEmptyState
      v-else-if="error"
      icon="alert"
      :title="t('cards.loadFailed')"
      :message="error.message"
    >
      <template #action><BaseButton variant="primary" @click="loadCards">{{ t('common.tryAgain') }}</BaseButton></template>
    </BaseEmptyState>

    <BaseEmptyState
      v-else-if="!cards?.length"
      icon="card"
      :title="t('cards.noCardsTitle')"
      :message="t('cards.noCardsMessage')"
    >
      <template #action><BaseButton variant="primary" @click="openCreate">{{ t('cards.addFirst') }}</BaseButton></template>
    </BaseEmptyState>

    <motion.div
      v-else
      class="grid"
      v-bind="fadeUp()"
    >
      <CreditCardTile
        v-for="card in cards"
        :key="card.id"
        :card="card"
        @pay="openPay"
        @edit="openEdit"
        @limit="openLimit"
        @delete="remove"
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
  grid-template-columns: repeat(auto-fill, minmax(360px, 1fr));
  gap: var(--sp-4);
}


@media (max-width: 640px) {
  .grid { grid-template-columns: 1fr; }
}
</style>
