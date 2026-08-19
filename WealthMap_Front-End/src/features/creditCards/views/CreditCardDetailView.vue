<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter, RouterLink } from 'vue-router'
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { creditCardsApi } from '@/api/creditCards.api'
import { cardIncidentsApi, CARD_KIND } from '@/api/cardIncidents.api'
import { purchasesApi } from '@/api/purchases.api'
import { installmentsApi } from '@/api/installments.api'
import { useAsync } from '@/composables/useAsync'
import { useMoney } from '@/composables/useMoney'
import { useDateTime } from '@/composables/useDateTime'
import { useDashboardStore } from '@/stores/dashboard.store'
import { useUiStore } from '@/stores/ui.store'
import { useToast } from '@/composables/useToast'

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
import CardBlockedBanner from '@/features/shared/components/CardBlockedBanner.vue'
import CardIncidentHistory from '@/features/shared/components/CardIncidentHistory.vue'
import ReportCardLostModal from '@/features/shared/components/ReportCardLostModal.vue'
import ReplaceCardModal from '@/features/shared/components/ReplaceCardModal.vue'
import CardPaymentModal from '../components/CardPaymentModal.vue'
import LimitModal from '../components/LimitModal.vue'
import PaymentsTable from '@/features/shared/components/PaymentsTable.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const route = useRoute()
const router = useRouter()
const { format, formatPercent } = useMoney()
const { formatDate, relativeDay } = useDateTime()
const dashboard = useDashboardStore()
const ui = useUiStore()
const toast = useToast()

const cardId = route.params.id

const { data: card, loading, error, run: loadCard } = useAsync(() => creditCardsApi.get(cardId))
const { data: payments, loading: loadingPayments, run: loadPayments } =
  useAsync(() => creditCardsApi.payments(cardId), { initialData: [] })

/** The API filters by card, so this is every purchase charged to it. */
const { data: purchasePage, loading: loadingCharges, run: loadPurchases } =
  useAsync(() => purchasesApi.list({ creditCardId: cardId, page: 1, pageSize: 100 }))
const { data: plans, run: loadPlans } = useAsync(installmentsApi.list, { initialData: [] })

/** Every time this card was reported lost, and how each report ended. */
const { data: incidents, run: loadIncidents } =
  useAsync(() => cardIncidentsApi.list(CARD_KIND.CREDIT, cardId), { initialData: [] })

/** What the report modals need: an id, a name, and the number being replaced. */
const reportTarget = computed(() => ({
  id: cardId,
  name: card.value?.cardName ?? '',
  lastFour: card.value?.lastFour ?? null
}))

const payOpen = ref(false)
const editOpen = ref(false)
const limitOpen = ref(false)
const reportOpen = ref(false)
const replaceOpen = ref(false)
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
      isPlan: false,
      kind: t('purchases.kindPurchase'),
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
      isPlan: true,
      kind: t('installments.planKind'),
      meta: t('composed.planMeta', { total: plan.monthsCount, remaining: plan.remainingMonths }),
      name: plan.productName,
      // The full price hits the card on day one, which is what created the debt.
      amount: plan.totalPrice,
      currency: plan.currency,
      occurredAt: plan.purchasedAt
    }))

  return [...fromPurchases, ...fromPlans]
    .sort((a, b) => new Date(b.occurredAt) - new Date(a.occurredAt))
})

/** Every plan bought on this card, newest first. Completed ones stay: the card
 *  paid for them, and hiding them would make the history look shorter than it was. */
const cardPlans = computed(() =>
  (plans.value ?? [])
    .filter((plan) => plan.creditCardId === cardId)
    .sort((a, b) => new Date(b.purchasedAt) - new Date(a.purchasedAt))
)

/** What the plans together add to the statement the card is about to bill. */
const plansDueThisStatement = computed(() =>
  cardPlans.value.reduce((sum, plan) => sum + (plan.dueThisStatement ?? 0), 0)
)

const PLAN_COLUMNS = computed(() => [
  { key: 'productName', label: t('purchases.item') },
  { key: 'progress', label: t('installments.progress'), width: '150px' },
  { key: 'remainingBalance', label: t('installments.remaining'), align: 'right', width: '130px' },
  { key: 'dueThisStatement', label: t('cards.addsToStatement'), align: 'right', width: '150px' }
])

const tabs = computed(() => [
  { value: 'charges', label: t('cards.charges'), count: charges.value.length },
  { value: 'plans', label: t('cards.installments'), count: cardPlans.value.length },
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

/**
 * Purchases have no detail screen, so only plans are navigable.
 *
 * Keyed off a flag set when the row is built, not off the label. The label is
 * translated, so comparing against the English text would silently stop matching
 * the moment the language changed.
 */
function isPlan(row) {
  return row.isPlan === true
}

function openCharge(row) {
  if (isPlan(row)) router.push(`/installments/${row.id}`)
}

function refresh() {
  loadCard()
  loadPayments()
  loadPurchases()
  loadPlans()
  loadIncidents()
  dashboard.invalidate()
}

/**
 * Confirmed rather than done outright: it closes the open report, and a report
 * closed by accident cannot be reopened — only filed again on a new date.
 */
async function markRecovered() {
  const confirmed = await ui.confirm({
    title: t('cardLoss.foundTitle', { card: card.value.cardName }),
    message: t('cardLoss.foundMessage'),
    confirmLabel: t('cardLoss.foundIt')
  })

  if (!confirmed) return

  await cardIncidentsApi.recover(CARD_KIND.CREDIT, cardId, {
    recoveredOn: new Date().toISOString().slice(0, 10)
  })

  toast.success(t('cardLoss.recoveredToast', { card: card.value.cardName }))
  refresh()
}

onMounted(() => {
  loadCard()
  loadPayments()
  loadPurchases()
  loadPlans()
  loadIncidents()
})
</script>

<template>
  <div>
    <RouterLink to="/credit-cards" class="back">
      <BaseIcon name="chevron-left" :size="15" />
      {{ t('cards.allCards') }}
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

    <motion.div v-else-if="card" v-bind="fadeUp()">
      <PageHeader :title="card.cardName" :subtitle="card.bankName">
        <template #actions>
          <BaseButton variant="primary" :disabled="card.usedCredit <= 0" @click="payOpen = true">
            <template #icon><BaseIcon name="receipt" :size="15" /></template>
            {{ t('cards.registerPayment') }}
          </BaseButton>
          <BaseButton variant="secondary" @click="limitOpen = true">{{ t('cards.limit') }}</BaseButton>
          <BaseButton variant="ghost" @click="editOpen = true">
            <template #icon><BaseIcon name="pencil" :size="15" /></template>
            {{ t('common.edit') }}
          </BaseButton>
          <BaseButton v-if="!card.blockedOn" variant="ghost" @click="reportOpen = true">
            <template #icon><BaseIcon name="alert" :size="15" /></template>
            {{ t('cardLoss.reportAction') }}
          </BaseButton>
        </template>
      </PageHeader>

      <CardBlockedBanner
        v-if="card.blockedOn"
        :kind="CARD_KIND.CREDIT"
        :reason="card.blockReason"
        :blocked-on="card.blockedOn"
        @replace="replaceOpen = true"
        @recover="markRecovered"
      />

      <div class="summary">
        <div class="summary__figures">
          <div class="summary__figure">
            <span class="summary__label">{{ t('cards.available') }}</span>
            <p class="summary__value numeric is-positive">
              {{ format(card.availableCredit, { currency: card.currency }) }}
            </p>
          </div>

          <!-- Owed in full, then broken down below: the total answers "how deep am
               I in?", the split answers "what do I have to pay, and when?" — and
               only the second one is actionable. -->
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
          :label="t('cards.limitInUse', { percent: utilisation.toFixed(0) })"
        />

        <!-- The three parts of what is owed, each with the date that makes it
             actionable. They sum to `usedCredit` above, so the reader can check. -->
        <dl class="summary__split">
          <div class="summary__split-item summary__split-item--due">
            <dt>
              {{ t('cards.dueThisStatement') }}
              <span class="summary__split-note">
                {{ t('composed.closedOn', { date: formatDate(card.lastCutoffDate) }) }} ·
                {{ t('composed.payBy', { date: formatDate(card.nextDueDate) }) }}
              </span>
            </dt>
            <dd class="numeric">{{ format(card.statementBalance, { currency: card.currency }) }}</dd>
          </div>

          <div class="summary__split-item">
            <dt>
              {{ t('cards.nextStatement') }}
              <span class="summary__split-note">
                {{ t('composed.closesOn', { date: formatDate(card.nextCutoffDate) }) }}
              </span>
            </dt>
            <dd class="numeric">{{ format(card.currentCycleCharges, { currency: card.currency }) }}</dd>
          </div>

          <div v-if="card.futureInstallments > 0" class="summary__split-item">
            <dt>
              {{ t('cards.futureInstallments') }}
              <span class="summary__split-note">{{ t('composed.notYetBilled') }}</span>
            </dt>
            <dd class="numeric">{{ format(card.futureInstallments, { currency: card.currency }) }}</dd>
          </div>
        </dl>

        <!-- Dates rather than day numbers: "the 17th" leaves the reader to work
             out which 17th, and after the cutoff has passed it is not the next one. -->
        <dl class="summary__meta">
          <div>
            <dt>{{ t('cards.statementCloses') }}</dt>
            <dd>{{ formatDate(card.nextCutoffDate) }} · {{ relativeDay(card.daysUntilCutoff) }}</dd>
          </div>
          <div>
            <dt>{{ t('cards.paymentDue') }}</dt>
            <dd>{{ formatDate(card.nextDueDate) }} · {{ relativeDay(card.daysUntilDue) }}</dd>
          </div>
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

          <!-- Keyed off the row's flag, not its label: the label is translated, so
               comparing it against English would pick the wrong colour in Spanish. -->
          <template #cell-kind="{ row, value }">
            <BaseBadge :variant="row.isPlan ? 'accent' : 'neutral'" size="sm">
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

      <!-- What was bought on instalments, and what each one is about to bill.
           The charges tab shows the day a plan was created and its full price;
           this answers the different question of what it costs this month. -->
      <BaseCard v-else-if="tab === 'plans'" :padded="false">
        <BaseTable
          :columns="PLAN_COLUMNS"
          :rows="cardPlans"
          :clickable="() => true"
          :empty-title="t('cards.noPlansTitle')"
          :empty-message="t('cards.noPlansMessage')"
          @row-click="(row) => $router.push(`/installments/${row.id}`)"
        >
          <template #cell-productName="{ row }">
            <div class="cell-stack">
              <RouterLink :to="`/installments/${row.id}`" class="cell-stack__link" @click.stop>
                {{ row.productName }}
                <BaseIcon name="arrow-up-right" :size="13" />
              </RouterLink>
              <span class="cell-stack__sub">
                {{ t('composed.planMeta', { total: row.monthsCount, remaining: row.remainingMonths }) }}
                · {{ format(row.monthlyPayment, { currency: row.currency }) }}
              </span>
            </div>
          </template>

          <template #cell-progress="{ row }">
            <BaseBadge v-if="row.isCompleted" variant="positive" size="sm">
              {{ t('common.completed') }}
            </BaseBadge>
            <BaseProgress
              v-else
              :value="row.monthsCount - row.remainingMonths"
              :max="row.monthsCount"
              variant="accent"
              size="sm"
            />
          </template>

          <template #cell-remainingBalance="{ row }">
            <span class="numeric">{{ format(row.remainingBalance, { currency: row.currency }) }}</span>
          </template>

          <!-- Zero once this month's instalment is paid, which is the point: the
               column answers "what is still coming", not "what does it cost". -->
          <template #cell-dueThisStatement="{ row }">
            <span :class="['numeric', { 'is-muted': row.dueThisStatement === 0 }]">
              {{ format(row.dueThisStatement, { currency: row.currency }) }}
            </span>
          </template>
        </BaseTable>

        <template v-if="cardPlans.length" #footer>
          <div class="plans-total">
            <span>{{ t('cards.plansAddToStatement', { date: formatDate(card.nextDueDate) }) }}</span>
            <strong class="numeric">
              {{ format(plansDueThisStatement, { currency: card.currency }) }}
            </strong>
          </div>
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

      <!-- After the tables: the history is context for the card, not a task on it. -->
      <CardIncidentHistory :incidents="incidents ?? []" class="history-card" />

      <CardPaymentModal v-model="payOpen" :card="card" @saved="refresh" />
      <CardFormModal v-model="editOpen" :card="card" @saved="refresh" />
      <LimitModal v-model="limitOpen" :card="card" @saved="refresh" />

      <ReportCardLostModal
        v-model="reportOpen"
        :kind="CARD_KIND.CREDIT"
        :card="reportTarget"
        @saved="refresh"
      />
      <ReplaceCardModal
        v-model="replaceOpen"
        :kind="CARD_KIND.CREDIT"
        :card="reportTarget"
        @saved="refresh"
      />
    </motion.div>
  </div>
</template>

<style scoped lang="scss" src="@/assets/styles/features/creditCards/CreditCardDetailView.scss"></style>
