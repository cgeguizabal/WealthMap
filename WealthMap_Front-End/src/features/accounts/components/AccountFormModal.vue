<script setup>
import { ref, watch, computed } from 'vue'
import { accountsApi, ACCOUNT_TYPE, ACCOUNT_TYPE_OPTIONS } from '@/api/accounts.api'
import { useServerText } from '@/composables/useServerText'
import { useForm } from '@/composables/useForm'
import { useToast } from '@/composables/useToast'
import { useAuthStore } from '@/stores/auth.store'
import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()
const { label: serverLabel } = useServerText()

/** Values stay the API's integers; only the wording follows the locale. */
const typeOptions = computed(() =>
  ACCOUNT_TYPE_OPTIONS.map((o) => ({
    ...o,
    label: serverLabel('accountType', o.value === ACCOUNT_TYPE.SAVINGS ? 'Savings' : 'Checking')
  }))
)

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  /** Present → edit mode; absent → create mode. */
  account: { type: Object, default: null }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const auth = useAuthStore()

const isEdit = computed(() => props.account !== null)

const CURRENCIES = ['USD', 'MXN', 'EUR', 'GBP', 'CAD', 'BRL', 'COP', 'ARS']
  .map((code) => ({ value: code, label: code }))

function blank() {
  return {
    name: '',
    bankName: '',
    type: 1,
    openingBalance: null,
    currency: auth.currency,
    notes: ''
  }
}

const { values, submitting, formError, submit, reset, fieldError } = useForm(blank(), async (payload) => {
  if (isEdit.value) {
    // Balance, type and currency are immutable after creation.
    return accountsApi.update(props.account.id, {
      name: payload.name,
      bankName: payload.bankName,
      notes: payload.notes || null
    })
  }

  return accountsApi.create({
    name: payload.name,
    bankName: payload.bankName,
    type: payload.type,
    openingBalance: payload.openingBalance ?? 0,
    currency: payload.currency
  })
})

const open = ref(props.modelValue)
watch(() => props.modelValue, (value) => {
  open.value = value

  if (value) {
    reset(props.account
      ? {
          ...blank(),
          name: props.account.name,
          bankName: props.account.bankName,
          notes: props.account.notes ?? ''
        }
      : blank())
  }
})
watch(open, (value) => emit('update:modelValue', value))

async function onSubmit() {
  const result = await submit()
  if (!result) return

  toast.success(isEdit.value ? 'Account updated.' : `${result.name} is ready.`)
  emit('saved', result)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" :title="isEdit ? t('accounts.editAccount') : t('accounts.newAccount')">
    <form id="account-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <BaseInput
        v-model="values.name"
        :label="t('accounts.accountName')"
        :placeholder="t('accounts.accountNamePlaceholder')"
        required
        :error="fieldError('name')"
      />

      <BaseInput
        v-model="values.bankName"
        :label="t('accounts.bankName')"
        :placeholder="t('accounts.bankPlaceholder')"
        required
        :error="fieldError('bankName')"
      />

      <template v-if="!isEdit">
        <div class="form__row">
          <BaseSelect
            v-model="values.type"
            :label="t('common.type')"
            :options="typeOptions"
            required
            :hint="t('accounts.typeHint')"
            :error="fieldError('type')"
          />

          <BaseSelect
            v-model="values.currency"
            :label="t('common.currency')"
            :options="CURRENCIES"
            required
            :error="fieldError('currency')"
          />
        </div>

        <BaseInput
          v-model="values.openingBalance"
          :label="t('accounts.openingBalance')"
          type="number"
          step="0.01"
          min="0"
          placeholder="0.00"
          :hint="t('accounts.openingBalanceHint')"
          :error="fieldError('openingBalance')"
        />
      </template>

      <BaseInput
        v-if="isEdit"
        v-model="values.notes"
        :label="t('common.notes')"
        :placeholder="t('common.optional')"
        :error="fieldError('notes')"
      />
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="account-form" variant="primary" :loading="submitting">
        {{ isEdit ? t('common.saveChanges') : t('accounts.createAccount') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss" src="./AccountFormModal.scss"></style>
