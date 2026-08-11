<script setup>
import { ref, computed, onMounted } from 'vue'
import { storesApi } from '@/api/stores.api'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import StoreFormModal from '@/features/stores/components/StoreFormModal.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

/**
 * The catalogue is shared, so the list is everyone's stores. Creating one inline
 * keeps a purchase from being abandoned just because the shop is missing.
 */
defineProps({
  modelValue: { type: String, default: null },
  label: { type: String, default: 'Store' },
  error: { type: [Array, String], default: null }
})

const emit = defineEmits(['update:modelValue'])

const stores = ref([])
const createOpen = ref(false)

const options = computed(() =>
  stores.value.map((store) => ({
    value: store.id,
    label: store.category ? `${store.name} — ${store.category}` : store.name
  }))
)

async function load() {
  try {
    stores.value = await storesApi.list()
  } catch {
    stores.value = []
  }
}

/** Select the store just created, so the inline flow finishes the job. */
function onCreated(store) {
  stores.value = [...stores.value, store].sort((a, b) => a.name.localeCompare(b.name))
  emit('update:modelValue', store.id)
}

onMounted(load)
</script>

<template>
  <div class="picker">
    <div class="picker__field">
      <BaseSelect
        :model-value="modelValue"
        :label="label"
        :options="options"
        :placeholder="t('stores.noStore')"
        :error="error"
        @update:model-value="emit('update:modelValue', $event)"
      />
    </div>

    <BaseButton size="sm" variant="secondary" class="picker__add" @click="createOpen = true">
      <template #icon><BaseIcon name="plus" :size="14" /></template>
      {{ t('common.new') }}
    </BaseButton>

    <StoreFormModal v-model="createOpen" @saved="onCreated" />
  </div>
</template>

<style scoped lang="scss" src="./StorePicker.scss"></style>
