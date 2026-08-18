<script setup>
import { ref, watch, computed, onMounted } from 'vue'
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { paymentsApi, PAYMENT_TARGET_OPTIONS } from '@/api/payments.api'
import { usePagination } from '@/composables/usePagination'
import { useServerText } from '@/composables/useServerText'
import { useToast } from '@/composables/useToast'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BasePagination from '@/components/base/BasePagination.vue'
import PaymentsTable from '@/features/shared/components/PaymentsTable.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()
const { label: serverLabel } = useServerText()

/** Values stay the API's target names; only the wording follows the locale. */
const targetOptions = computed(() =>
  PAYMENT_TARGET_OPTIONS.map((o) => ({ ...o, label: serverLabel('paymentTarget', o.value) }))
)

const toast = useToast()
const pagination = usePagination({ pageSize: 20 })

const payments = ref([])
const loading = ref(false)

const filters = ref({ from: '', to: '', targetType: null })

async function load() {
  loading.value = true

  try {
    const response = await paymentsApi.list({
      page: pagination.page.value,
      pageSize: pagination.size.value,
      from: filters.value.from || undefined,
      to: filters.value.to || undefined,
      targetType: filters.value.targetType || undefined
    })
    payments.value = pagination.apply(response)
  } catch (err) {
    toast.error(err.message)
    payments.value = []
  } finally {
    loading.value = false
  }
}

/** A filter change makes the current page number meaningless. */
function applyFilters() {
  pagination.reset()
  load()
}

function clearFilters() {
  filters.value = { from: '', to: '', targetType: null }
  applyFilters()
}

watch(pagination.page, load)
onMounted(load)
</script>

<template>
  <motion.div v-bind="fadeUp()">
    <PageHeader
      :title="t('payments.title')"
      :subtitle="t('payments.subtitle')"
    />

    <BaseCard :padded="false">
      <template #header>
        <div class="filters">
          <BaseSelect
            v-model="filters.targetType"
            :options="targetOptions"
            :placeholder="t('payments.allTypes')"
            :label="t('common.type')"
          />

          <BaseInput v-model="filters.from" :label="t('payments.from')" type="date" />
          <BaseInput v-model="filters.to" label="To" type="date" />

          <div class="filters__actions">
            <BaseButton variant="secondary" @click="applyFilters">{{ t('common.apply') }}</BaseButton>
            <BaseButton variant="ghost" @click="clearFilters">{{ t('common.clear') }}</BaseButton>
          </div>
        </div>
      </template>

      <PaymentsTable :payments="payments" :loading="loading" />

      <BasePagination
        :page="pagination.page.value"
        :page-size="pagination.size.value"
        :total-count="pagination.totalCount.value"
        :total-pages="pagination.totalPages.value"
        :has-next-page="pagination.hasNextPage.value"
        :has-previous-page="pagination.hasPreviousPage.value"
        @update:page="pagination.goTo($event)"
      />
    </BaseCard>
  </motion.div>
</template>

<style scoped lang="scss" src="@/assets/styles/features/payments/PaymentsView.scss"></style>
