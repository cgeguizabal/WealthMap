<script setup>
import { ref, computed, onMounted } from 'vue'
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { bankDefaultsApi } from '@/api/bankDefaults.api'
import { accountsApi } from '@/api/accounts.api'
import { useAsync } from '@/composables/useAsync'
import { useToast } from '@/composables/useToast'
import { useDoubleConfirm } from '@/composables/useDoubleConfirm'
import { useI18n } from '@/composables/useI18n'
import { useTourStore } from '@/stores/tour.store'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseTable from '@/components/base/BaseTable.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'
import BankDefaultFormModal from '../components/BankDefaultFormModal.vue'
import DeleteAccountModal from '../components/DeleteAccountModal.vue'

const { t } = useI18n()
const toast = useToast()
const confirmTwice = useDoubleConfirm()
const tour = useTourStore()
const deleteAccountOpen = ref(false)

const { data: defaults, loading, run: loadDefaults } = useAsync(
  bankDefaultsApi.list, { initialData: [] })

// The account list is only ever used to fill the dropdown, so archived accounts
// are excluded exactly as the server requires them to be.
const { data: accounts, run: loadAccounts } = useAsync(accountsApi.list, { initialData: [] })

const formOpen = ref(false)
const editing = ref(null)

const COLUMNS = computed(() => [
  { key: 'bankName', label: t('bankDefaults.bankName') },
  { key: 'direction', label: t('bankDefaults.direction') },
  { key: 'defaultAccountName', label: t('bankDefaults.account') },
  { key: 'actions', label: '', align: 'right' }
])

const rows = computed(() => defaults.value ?? [])

onMounted(() => {
  loadDefaults()
  loadAccounts()
})

function openCreate() {
  editing.value = null
  formOpen.value = true
}

function openEdit(row) {
  editing.value = row
  formOpen.value = true
}

async function onSaved() {
  await loadDefaults()
}

async function remove(row) {
  const confirmed = await confirmTwice({
    title: t('bankDefaults.deleteTitle'),
    message: t('bankDefaults.deleteMessage', { bank: row.bankName }),
    secondMessage: t('bankDefaults.deleteSecond')
  })

  if (!confirmed) return

  try {
    await bankDefaultsApi.remove(row.id)
    toast.success(t('bankDefaults.deleted'))
    await loadDefaults()
  } catch (err) {
    toast.error(err.message)
  }
}

/**
 * Clears the record of which tours have played, so each one runs again the next
 * time its screen is opened. Not a confirm-twice action: the worst case is
 * seeing a tour you did not need, which is a click to dismiss.
 */
function replayTours() {
  tour.resetAll()
  toast.success(t('tour.replayed'))
}
</script>

<template>
  <motion.div v-bind="fadeUp()">
    <PageHeader :title="t('settings.title')" :subtitle="t('settings.subtitle')" />

    <!-- Last on the page, and visually separated: it is the only action here
         that cannot be undone. -->
    <BaseCard
      class="settings__danger"
      :title="t('settings.dangerZone')"
      :subtitle="t('settings.dangerZoneSubtitle')"
    >
      <BaseButton variant="danger" @click="deleteAccountOpen = true">
        <template #icon><BaseIcon name="trash" :size="15" /></template>
        {{ t('settings.deleteAccount') }}
      </BaseButton>
    </BaseCard>

    <BaseCard :title="t('tour.replay')" :subtitle="t('tour.replayHint')">
      <BaseButton variant="secondary" @click="replayTours">
        <template #icon><BaseIcon name="info" :size="15" /></template>
        {{ t('tour.replay') }}
      </BaseButton>
    </BaseCard>

    <BaseCard data-tour="settings-defaults" :title="t('bankDefaults.title')" :subtitle="t('bankDefaults.explain')" :padded="false">
      <template #actions>
        <BaseButton variant="primary" size="sm" @click="openCreate">
          <template #icon><BaseIcon name="plus" :size="15" /></template>
          {{ t('bankDefaults.add') }}
        </BaseButton>
      </template>

      <!-- Informational, not an error: having no defaults is the normal starting
           state and nothing is broken without them. -->
      <BaseEmptyState
        v-if="!loading && rows.length === 0"
        icon="info"
        :title="t('bankDefaults.emptyTitle')"
        :message="t('bankDefaults.emptyMessage')"
      >
        <template #action>
          <BaseButton variant="secondary" @click="openCreate">{{ t('bankDefaults.add') }}</BaseButton>
        </template>
      </BaseEmptyState>

      <BaseTable
        v-else
        :columns="COLUMNS"
        :rows="rows"
        :loading="loading"
        :empty-title="t('bankDefaults.emptyTitle')"
        :empty-message="t('bankDefaults.emptyMessage')"
      >
        <template #cell-direction="{ value }">
          <BaseBadge size="sm">
            {{ value === 'Outbound' ? t('bankDefaults.outbound') : t('bankDefaults.inbound') }}
          </BaseBadge>
        </template>

        <template #cell-actions="{ row }">
          <div class="settings__row-actions">
            <BaseButton
              size="sm"
              variant="ghost"
              :title="t('common.edit')"
              :aria-label="t('common.edit')"
              @click="openEdit(row)"
            >
              <template #icon><BaseIcon name="pencil" :size="14" /></template>
            </BaseButton>

            <BaseButton
              class="settings__delete"
              size="sm"
              variant="ghost"
              :title="t('common.delete')"
              :aria-label="t('common.delete')"
              @click="remove(row)"
            >
              <template #icon><BaseIcon name="trash" :size="14" /></template>
            </BaseButton>
          </div>
        </template>
      </BaseTable>
    </BaseCard>

    <DeleteAccountModal v-model="deleteAccountOpen" />

    <BankDefaultFormModal
      v-model="formOpen"
      :bank-default="editing"
      :accounts="accounts ?? []"
      @saved="onSaved"
    />
  </motion.div>
</template>

<style scoped lang="scss" src="@/assets/styles/features/settings/SettingsView.scss"></style>
