<script setup>
import { ref, computed, onMounted } from 'vue'
import { accountsApi } from '@/api/accounts.api'
import { creditCardsApi } from '@/api/creditCards.api'
import { useToast } from '@/composables/useToast'
import { useMoney } from '@/composables/useMoney'
import { useDashboardStore } from '@/stores/dashboard.store'

import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseSpinner from '@/components/base/BaseSpinner.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'
import { useI18n } from '@/composables/useI18n'

/**
 * Archived accounts and cards, and the way back.
 *
 * Archiving is offered everywhere as the safe alternative to deleting — the row
 * survives, and every movement and payment that references it survives with it.
 * That was only half true while nothing could un-archive: a mis-click removed an
 * account from every list and total permanently, which is not what "safe" means.
 */
const { t } = useI18n()

const toast = useToast()
const { format } = useMoney()
const dashboard = useDashboardStore()

const loading = ref(true)
const accounts = ref([])
const cards = ref([])

const archived = computed(() => [
  ...accounts.value
    .filter((a) => a.isArchived)
    .map((a) => ({ ...a, kind: 'account', label: a.name, sub: a.bankName })),
  ...cards.value
    .filter((c) => c.isArchived)
    .map((c) => ({ ...c, kind: 'card', label: c.cardName, sub: c.bankName }))
])

async function load() {
  loading.value = true

  try {
    // Archived rows are excluded by default, so both lists have to ask for them.
    const [accountResult, cardResult] = await Promise.all([
      accountsApi.list({ includeArchived: true }),
      creditCardsApi.list({ includeArchived: true })
    ])

    accounts.value = accountResult
    cards.value = cardResult
  } catch (err) {
    toast.error(err.message)
  } finally {
    loading.value = false
  }
}

onMounted(load)

async function restore(item) {
  try {
    if (item.kind === 'account') await accountsApi.restore(item.id)
    else await creditCardsApi.restore(item.id)

    toast.success(t('settings.restored', { name: item.label }))

    // Back in the totals, so every figure on the dashboard is now stale.
    await load()
    dashboard.load()
  } catch (err) {
    toast.error(err.message)
  }
}
</script>

<template>
  <BaseCard :title="t('settings.archived')" :subtitle="t('settings.archivedSubtitle')" :padded="false">
    <div v-if="loading" class="archived__loading"><BaseSpinner :size="20" /></div>

    <BaseEmptyState
      v-else-if="!archived.length"
      icon="info"
      :title="t('settings.nothingArchivedTitle')"
      :message="t('settings.nothingArchivedMessage')"
      compact
    />

    <ul v-else class="archived">
      <li v-for="item in archived" :key="`${item.kind}-${item.id}`" class="archived__item">
        <BaseIcon :name="item.kind === 'account' ? 'wallet' : 'card'" :size="16" />

        <div class="archived__text">
          <span class="archived__name">{{ item.label }}</span>
          <span class="archived__meta">
            {{ item.sub }}
            <template v-if="item.kind === 'account'">
              · {{ format(item.balance, { currency: item.currency }) }}
            </template>
          </span>
        </div>

        <BaseButton size="sm" variant="secondary" @click="restore(item)">
          {{ t('settings.restore') }}
        </BaseButton>
      </li>
    </ul>
  </BaseCard>
</template>

<style scoped lang="scss" src="@/assets/styles/features/settings/ArchivedCard.scss"></style>
