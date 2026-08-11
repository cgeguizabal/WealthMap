<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import { accountsApi } from '@/api/accounts.api'
import { useAsync } from '@/composables/useAsync'
import { usePagination } from '@/composables/usePagination'
import { useMoney } from '@/composables/useMoney'
import { useToast } from '@/composables/useToast'
import { useDashboardStore } from '@/stores/dashboard.store'

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
import { useI18n } from '@/composables/useI18n'
import { useServerText } from '@/composables/useServerText'

const { t } = useI18n()
const { label: serverLabel } = useServerText()

const route = useRoute()
const toast = useToast()
const { format } = useMoney()
const dashboard = useDashboardStore()
const pagination = usePagination({ pageSize: 20 })

const accountId = route.params.id

const { data: account, loading: loadingAccount, error, run: loadAccount } =
  useAsync(() => accountsApi.get(accountId))

const movements = ref([])
const loadingMovements = ref(false)

const movementOpen = ref(false)
const movementMode = ref('deposit')
const editOpen = ref(false)

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
  dashboard.invalidate()
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
})
</script>

<template>
  <div>
    <RouterLink to="/accounts" class="back">
      <BaseIcon name="chevron-left" :size="15" />
      All accounts
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

    <template v-else-if="account">
      <PageHeader :title="account.name" :subtitle="account.bankName">
        <template #actions>
          <BaseButton variant="secondary" @click="movementMode = 'deposit'; movementOpen = true">
            <template #icon><BaseIcon name="plus" :size="15" /></template>
            Deposit
          </BaseButton>

          <BaseButton
            variant="secondary"
            :disabled="account.isBlockedForSaving"
            @click="movementMode = 'withdraw'; movementOpen = true"
          >
            <template #icon><BaseIcon name="minus" :size="15" /></template>
            Withdraw
          </BaseButton>

          <BaseButton variant="ghost" @click="editOpen = true">
            <template #icon><BaseIcon name="pencil" :size="15" /></template>
            Edit
          </BaseButton>
        </template>
      </PageHeader>

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
            Blocked for saving
          </BaseBadge>

          <BaseButton size="sm" variant="ghost" @click="toggleBlock">
            <template #icon>
              <BaseIcon :name="account.isBlockedForSaving ? 'unlock' : 'lock'" :size="14" />
            </template>
            {{ account.isBlockedForSaving ? 'Unblock' : 'Block for saving' }}
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

      <AccountFormModal v-model="editOpen" :account="account" @saved="refresh" />
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
  gap: var(--sp-3);

  padding: var(--sp-5);
  margin-bottom: var(--sp-5);

  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
}

.summary__balance { display: flex; flex-direction: column; gap: var(--sp-1); }

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
  align-items: center;
  flex-wrap: wrap;
  gap: var(--sp-2);
}

.summary__notes {
  padding-top: var(--sp-3);
  border-top: var(--border-subtle);
  font-size: var(--fs-sm);
  color: var(--text-muted);
}

.state { display: grid; place-items: center; padding: var(--sp-12); color: var(--text-muted); }
</style>
