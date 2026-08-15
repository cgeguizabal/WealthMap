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

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseTable from '@/components/base/BaseTable.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'
import BankDefaultFormModal from '../components/BankDefaultFormModal.vue'

const { t } = useI18n()
const toast = useToast()
const confirmTwice = useDoubleConfirm()

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
</script>

<template>
  <motion.div v-bind="fadeUp()">
    <PageHeader :title="t('settings.title')" :subtitle="t('settings.subtitle')" />

    <BaseCard :title="t('bankDefaults.title')" :subtitle="t('bankDefaults.explain')" :padded="false">
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

    <BankDefaultFormModal
      v-model="formOpen"
      :bank-default="editing"
      :accounts="accounts ?? []"
      @saved="onSaved"
    />
  </motion.div>
</template>

<style scoped lang="scss" src="@/assets/styles/features/settings/SettingsView.scss"></style>
