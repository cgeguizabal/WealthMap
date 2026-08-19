<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { accountsApi } from '@/api/accounts.api'
import { cardIncidentsApi, CARD_KIND } from '@/api/cardIncidents.api'
import { useAsync } from '@/composables/useAsync'
import { usePagination } from '@/composables/usePagination'
import { useMoney } from '@/composables/useMoney'
import { useToast } from '@/composables/useToast'
import { useDashboardStore } from '@/stores/dashboard.store'
import { useUiStore } from '@/stores/ui.store'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseSpinner from '@/components/base/BaseSpinner.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'

import MovementsTable from '../components/MovementsTable.vue'
import MovementFormModal from '../components/MovementFormModal.vue'
import AccountFormModal from '../components/AccountFormModal.vue'
import CardBlockedBanner from '@/features/shared/components/CardBlockedBanner.vue'
import CardIncidentHistory from '@/features/shared/components/CardIncidentHistory.vue'
import ReportCardLostModal from '@/features/shared/components/ReportCardLostModal.vue'
import ReplaceCardModal from '@/features/shared/components/ReplaceCardModal.vue'
import { useI18n } from '@/composables/useI18n'
import { useServerText } from '@/composables/useServerText'

const { t } = useI18n()
const { label: serverLabel } = useServerText()

const route = useRoute()
const toast = useToast()
const { format } = useMoney()
const dashboard = useDashboardStore()
const ui = useUiStore()
const pagination = usePagination({ pageSize: 20 })

const accountId = route.params.id

const { data: account, loading: loadingAccount, error, run: loadAccount } =
  useAsync(() => accountsApi.get(accountId))

const movements = ref([])
const loadingMovements = ref(false)

const movementOpen = ref(false)
const movementMode = ref('deposit')
const editOpen = ref(false)
const reportOpen = ref(false)
const replaceOpen = ref(false)

/** Every time this account's debit card was reported lost. */
const { data: incidents, run: loadIncidents } =
  useAsync(() => cardIncidentsApi.list(CARD_KIND.DEBIT, accountId), { initialData: [] })

/** Only an account that has a debit card can lose one. */
const hasDebitCard = computed(() => account.value?.debitCardType && account.value.debitCardType !== 'None')

/** The report modals speak about a card, so the account is described as one. */
const reportTarget = computed(() => ({
  id: accountId,
  name: account.value?.name ?? '',
  lastFour: account.value?.debitCardLastFour ?? null
}))

async function loadMovements() {
  loadingMovements.value = true

  try {
    const response = await accountsApi.movements(accountId, {
      page: pagination.page.value,
      pageSize: pagination.size.value
    })
    movements.value = pagination.apply(response)
  } catch (err) {
    toast.error(err.message)
  } finally {
    loadingMovements.value = false
  }
}

watch(pagination.page, loadMovements)

function refresh() {
  loadAccount()
  pagination.reset()
  loadMovements()
  loadIncidents()
  dashboard.invalidate()
}

/** Confirmed, because closing a report cannot be undone — only filed again. */
async function markRecovered() {
  const confirmed = await ui.confirm({
    title: t('cardLoss.foundTitle', { card: account.value.name }),
    message: t('cardLoss.foundMessage'),
    confirmLabel: t('cardLoss.foundIt')
  })

  if (!confirmed) return

  await cardIncidentsApi.recover(CARD_KIND.DEBIT, accountId, {
    recoveredOn: new Date().toISOString().slice(0, 10)
  })

  toast.success(t('cardLoss.recoveredToast', { card: account.value.name }))
  refresh()
}

async function toggleBlock() {
  try {
    if (account.value.isBlockedForSaving) {
      await accountsApi.unblock(accountId)
      toast.success(t('accounts.accountUnblocked'))
    } else {
      await accountsApi.block(accountId)
      toast.success(t('accounts.accountBlocked'))
    }
    loadAccount()
  } catch (err) {
    toast.error(err.message)
  }
}

onMounted(() => {
  loadAccount()
  loadMovements()
  loadIncidents()
})
</script>

<template>
  <div>
    <RouterLink to="/accounts" class="back">
      <BaseIcon name="chevron-left" :size="15" />
      {{ t('accounts.allAccounts') }}
    </RouterLink>

    <div v-if="loadingAccount && !account" class="state"><BaseSpinner :size="22" /></div>

    <BaseEmptyState
      v-else-if="error"
      icon="alert"
      :title="t('accounts.notFound')"
      :message="t('common.notFoundHint')"
    >
      <template #action>
        <BaseButton variant="secondary" @click="$router.push('/accounts')">{{ t('accounts.backToAccounts') }}</BaseButton>
      </template>
    </BaseEmptyState>

    <motion.div v-else-if="account" v-bind="fadeUp()">
      <PageHeader :title="account.name" :subtitle="account.bankName">
        <template #actions>
          <BaseButton variant="secondary" @click="movementMode = 'deposit'; movementOpen = true">
            <template #icon><BaseIcon name="plus" :size="15" /></template>
            {{ t('accounts.deposit') }}
          </BaseButton>

          <BaseButton
            variant="secondary"
            :disabled="account.isBlockedForSaving"
            @click="movementMode = 'withdraw'; movementOpen = true"
          >
            <template #icon><BaseIcon name="minus" :size="15" /></template>
            {{ t('accounts.withdraw') }}
          </BaseButton>

          <BaseButton variant="ghost" @click="editOpen = true">
            <template #icon><BaseIcon name="pencil" :size="15" /></template>
            {{ t('common.edit') }}
          </BaseButton>

          <BaseButton
            v-if="hasDebitCard && !account.debitCardBlockedOn"
            variant="ghost"
            @click="reportOpen = true"
          >
            <template #icon><BaseIcon name="alert" :size="15" /></template>
            {{ t('cardLoss.reportDebitAction') }}
          </BaseButton>
        </template>
      </PageHeader>

      <CardBlockedBanner
        v-if="account.debitCardBlockedOn"
        :kind="CARD_KIND.DEBIT"
        :reason="account.debitCardBlockReason"
        :blocked-on="account.debitCardBlockedOn"
        @replace="replaceOpen = true"
        @recover="markRecovered"
      />

      <div class="summary">
        <div class="summary__balance">
          <span class="summary__label">{{ t('accounts.currentBalance') }}</span>
          <span class="summary__value numeric">
            {{ format(account.balance, { currency: account.currency }) }}
          </span>
        </div>

        <div class="summary__meta">
          <BaseBadge :variant="account.type === 'Savings' ? 'accent' : 'neutral'">
            {{ serverLabel('accountType', account.type) }}
          </BaseBadge>

          <BaseBadge v-if="account.isBlockedForSaving" variant="warning">
            {{ t('accounts.blocked') }}
          </BaseBadge>

          <BaseButton size="sm" variant="ghost" @click="toggleBlock">
            <template #icon>
              <BaseIcon :name="account.isBlockedForSaving ? 'unlock' : 'lock'" :size="14" />
            </template>
            {{ account.isBlockedForSaving ? t('accounts.unblock') : t('accounts.block') }}
          </BaseButton>
        </div>

        <p v-if="account.notes" class="summary__notes">{{ account.notes }}</p>
      </div>

      <BaseCard :title="t('accounts.movements')" :subtitle="t('accounts.movementsSubtitle')" :padded="false">
        <MovementsTable
          :movements="movements"
          :loading="loadingMovements"
          :pagination="{
            page: pagination.page.value,
            pageSize: pagination.size.value,
            totalCount: pagination.totalCount.value,
            totalPages: pagination.totalPages.value,
            hasNextPage: pagination.hasNextPage.value,
            hasPreviousPage: pagination.hasPreviousPage.value
          }"
          @update:page="pagination.goTo($event)"
        />
      </BaseCard>

      <MovementFormModal
        v-model="movementOpen"
        :account="account"
        :mode="movementMode"
        @saved="refresh"
      />

      <CardIncidentHistory :incidents="incidents ?? []" class="history-card" />

      <AccountFormModal v-model="editOpen" :account="account" @saved="refresh" />

      <ReportCardLostModal
        v-model="reportOpen"
        :kind="CARD_KIND.DEBIT"
        :card="reportTarget"
        @saved="refresh"
      />
      <ReplaceCardModal
        v-model="replaceOpen"
        :kind="CARD_KIND.DEBIT"
        :card="reportTarget"
        @saved="refresh"
      />
    </motion.div>
  </div>
</template>

<style scoped lang="scss" src="@/assets/styles/features/accounts/AccountDetailView.scss"></style>
