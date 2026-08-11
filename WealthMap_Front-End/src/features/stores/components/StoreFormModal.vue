<script setup>
import { ref, watch, computed } from 'vue'
import { storesApi, STORE_CATEGORIES } from '@/api/stores.api'
import { useForm } from '@/composables/useForm'
import { useToast } from '@/composables/useToast'
import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  store: { type: Object, default: null }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const isEdit = computed(() => props.store !== null)

const categoryOptions = STORE_CATEGORIES.map((name) => ({ value: name, label: name }))

function blank() {
  return { name: '', category: '', logoUrl: '', description: '' }
}

const { values, submitting, formError, submit, reset, fieldError } = useForm(blank(), (payload) => {
  const body = {
    name: payload.name,
    category: payload.category,
    logoUrl: payload.logoUrl || null,
    description: payload.description || null
  }

  return isEdit.value ? storesApi.update(props.store.id, body) : storesApi.create(body)
})

const open = ref(props.modelValue)
watch(() => props.modelValue, (value) => {
  open.value = value

  if (value) {
    reset(props.store
      ? {
          name: props.store.name,
          category: props.store.category,
          logoUrl: props.store.logoUrl ?? '',
          description: props.store.description ?? ''
        }
      : blank())
  }
})
watch(open, (value) => emit('update:modelValue', value))

async function onSubmit() {
  const result = await submit()
  if (!result) return

  toast.success(isEdit.value ? 'Store updated.' : `${result.name} added to the catalogue.`)
  emit('saved', result)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" :title="isEdit ? t('stores.editStore') : t('stores.newStore')" size="sm">
    <form id="store-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <BaseInput
        v-model="values.name"
        :label="t('common.name')"
        :placeholder="t('stores.namePlaceholder')"
        required
        :error="fieldError('name')"
      />

      <BaseSelect
        v-model="values.category"
        :label="t('common.category')"
        :options="categoryOptions"
        :placeholder="t('common.chooseCategory')"
        required
        :error="fieldError('category')"
      />

      <BaseInput
        v-model="values.logoUrl"
        :label="t('stores.logoUrl')"
        :placeholder="t('stores.logoPlaceholder')"
        :hint="t('stores.logoHint')"
        :error="fieldError('logoUrl')"
      />

      <BaseInput
        v-model="values.description"
        :label="t('common.description')"
        :placeholder="t('common.optional')"
        :error="fieldError('description')"
      />
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="store-form" variant="primary" :loading="submitting">
        {{ isEdit ? 'Save changes' : 'Add store' }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss">
.form { display: flex; flex-direction: column; gap: var(--sp-4); }

.form__error {
  padding: var(--sp-3);
  border: 1px solid var(--negative);
  border-radius: var(--radius);
  background: var(--negative-soft);
  color: var(--negative);
  font-size: var(--fs-sm);
}
</style>
