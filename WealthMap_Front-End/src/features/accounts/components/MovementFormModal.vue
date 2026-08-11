<script setup>
import { ref, watch, computed } from 'vue'
import { accountsApi, DEPOSIT_TYPE_OPTIONS } from '@/api/accounts.api'
import { useForm } from '@/composables/useForm'
import { useToast } from '@/composables/useToast'
import { useMoney } from '@/composables/useMoney'
import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

/**
 * Only Deposit and Bonus can be created by hand; the values are the API's
 * integers and stay as they are, so only the wording follows the locale.
 */
const kindOptions = computed(() =>
  DEPOSIT_TYPE_OPTIONS.map((o) => ({
    ...o,
    label: t(o.value === 3 ? 'accounts.bonus' : 'accounts.deposit')
  }))
)

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  account: { type: Object, default: null },
  mode: { type: String, default: 'deposit', validator: (v) => ['deposit', 'withdraw'].includes(v) }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const { format } = useMoney()

const isDeposit = computed(() => props.mode === 'deposit')

function blank() {
  return { amount: null, description: '', type: 2, location: '' }
}

const { values, submitting, formError, submit, reset, fieldError } = useForm(blank(), async (payload) => {
  if (isDeposit.value) {
    return accountsApi.deposit(props.account.id, {
      amount: payload.amount,
      description: payload.description,
      type: payload.type
    })
  }

  return accountsApi.withdraw(props.account.id, {
    amount: payload.amount,
    description: payload.description,
    location: payload.location || null
  })
})

const open = ref(props.modelValue)
watch(() => props.modelValue, (value) => {
  open.value = value
  if (value) reset(blank())
})
watch(open, (value) => emit('update:modelValue', value))

async function onSubmit() {
  const movement = await submit()
  if (!movement) return

  toast.success(
    `${format(movement.amount, { currency: movement.currency })} ${isDeposit.value ? 'added to' : 'taken from'} ${props.account.name}.`
  )
  emit('saved', movement)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" :title="isDeposit ? t('accounts.deposit') : t('accounts.withdraw')" size="sm">
    <form id="movement-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <p v-if="account" class="form__context">
        {{ account.name }} · balance
        <strong class="numeric">{{ format(account.balance, { currency: account.currency }) }}</strong>
      </p>

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
        <template #prefix>{{ account?.currency }}</template>
      </BaseInput>

      <BaseSelect
        v-if="isDeposit"
        v-model="values.type"
        :label="t('common.kind')"
        :options="kindOptions"
        required
        :hint="t('accounts.depositKindHint')"
        :error="fieldError('type')"
      />

      <BaseInput
        v-model="values.description"
        :label="t('common.description')"
        :placeholder="isDeposit ? t('accounts.depositPlaceholder') : t('accounts.withdrawPlaceholder')"
        required
        :error="fieldError('description')"
      />

      <BaseInput
        v-if="!isDeposit"
        v-model="values.location"
        :label="t('common.location')"
        :placeholder="t('accounts.locationPlaceholder')"
        :hint="t('accounts.withdrawHint')"
        :error="fieldError('location')"
      />
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="movement-form" variant="primary" :loading="submitting">
        {{ isDeposit ? 'Deposit' : 'Withdraw' }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss">
.form { display: flex; flex-direction: column; gap: var(--sp-4); }

.form__context {
  padding: var(--sp-3);
  background: var(--canvas-alt);
  border-radius: var(--radius-sm);
  font-size: var(--fs-sm);
  color: var(--text-muted);

  strong { color: var(--text); font-weight: var(--fw-semibold); }
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
