<script setup>
import { ref, watch, computed } from 'vue'
import { accountsApi } from '@/api/accounts.api'
import { useForm } from '@/composables/useForm'
import { useToast } from '@/composables/useToast'
import { useMoney } from '@/composables/useMoney'
import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  accounts: { type: Array, default: () => [] }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const { format } = useMoney()

function blank() {
  return { fromAccountId: null, toAccountId: null, amount: null, description: '' }
}

const { values, submitting, formError, submit, reset, fieldError } = useForm(blank(), (payload) =>
  accountsApi.transfer({
    fromAccountId: payload.fromAccountId,
    toAccountId: payload.toAccountId,
    amount: payload.amount,
    description: payload.description || null
  })
)

const from = computed(() => props.accounts.find((a) => a.id === values.fromAccountId) ?? null)

const fromOptions = computed(() =>
  props.accounts.map((account) => ({
    value: account.id,
    // Blocked accounts reject withdrawals, so they cannot be a source.
    label: `${account.name} — ${format(account.balance, { currency: account.currency })}${account.isBlockedForSaving ? ' (blocked)' : ''}`,
    disabled: account.isBlockedForSaving
  }))
)

/**
 * Same-account transfers are rejected by the API, and cross-currency ones fail
 * on the amount's currency, so neither is offered as a destination.
 */
const toOptions = computed(() =>
  props.accounts
    .filter((account) => account.id !== values.fromAccountId)
    .filter((account) => !from.value || account.currency === from.value.currency)
    .map((account) => ({
      value: account.id,
      label: `${account.name} — ${format(account.balance, { currency: account.currency })}`
    }))
)

watch(() => values.fromAccountId, () => {
  if (values.toAccountId && !toOptions.value.some((o) => o.value === values.toAccountId)) {
    values.toAccountId = null
  }
})

const open = ref(props.modelValue)
watch(() => props.modelValue, (value) => {
  open.value = value
  if (value) reset(blank())
})
watch(open, (value) => emit('update:modelValue', value))

async function onSubmit() {
  const result = await submit()
  if (!result) return

  toast.success(
    `${format(result.amount, { currency: result.currency })} moved to ${result.toAccount.name}.`
  )
  emit('saved', result)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" :title="t('accounts.transferTitle')">
    <form id="transfer-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <BaseSelect
        v-model="values.fromAccountId"
        :label="t('accounts.from')"
        :options="fromOptions"
        :placeholder="t('accounts.chooseAccount')"
        required
        :error="fieldError('fromAccountId')"
      />

      <div class="form__arrow" aria-hidden="true">
        <BaseIcon name="arrow-right" :size="16" />
      </div>

      <BaseSelect
        v-model="values.toAccountId"
        :label="t('accounts.to')"
        :options="toOptions"
        :placeholder="values.fromAccountId ? t('accounts.chooseAccount') : t('accounts.pickSourceFirst')"
        :disabled="!values.fromAccountId"
        required
        :hint="from ? t('accounts.sameCurrencyHint', { currency: from.currency }) : ''"
        :error="fieldError('toAccountId')"
      />

      <BaseInput
        v-model="values.amount"
        :label="t('common.amount')"
        type="number"
        step="0.01"
        min="0"
        placeholder="0.00"
        required
        :error="fieldError('amount')"
      >
        <template #prefix>{{ from?.currency ?? '' }}</template>
      </BaseInput>

      <BaseInput
        v-model="values.description"
        :label="t('common.description')"
        :placeholder="t('common.optional')"
        :error="fieldError('description')"
      />
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="transfer-form" variant="primary" :loading="submitting">
        {{ t('accounts.transfer') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss">
.form { display: flex; flex-direction: column; gap: var(--sp-4); }

.form__arrow {
  display: flex;
  justify-content: center;
  color: var(--text-subtle);
  margin: calc(var(--sp-2) * -1) 0;
}

.form__error {
  padding: var(--sp-3);
  border: 1px solid var(--negative);
  border-radius: var(--radius);
  background: var(--negative-soft);
  color: var(--negative);
  font-size: var(--fs-sm);
}
</style>
