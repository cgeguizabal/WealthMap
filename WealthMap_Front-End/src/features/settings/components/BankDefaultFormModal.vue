<script setup>
import { ref, watch, computed } from 'vue'
import { bankDefaultsApi } from '@/api/bankDefaults.api'
import { TRANSFER_DIRECTION, directionValue } from '@/api/tracking'
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
  /** Present → edit mode; absent → create mode. */
  bankDefault: { type: Object, default: null },
  /** Non-archived accounts only — an archived one could never be honoured. */
  accounts: { type: Array, default: () => [] }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const isEdit = computed(() => props.bankDefault !== null)

const directionOptions = computed(() => [
  { value: TRANSFER_DIRECTION.INBOUND, label: t('bankDefaults.inbound') },
  { value: TRANSFER_DIRECTION.OUTBOUND, label: t('bankDefaults.outbound') }
])

const accountOptions = computed(() =>
  props.accounts.map((a) => ({ value: a.id, label: `${a.name} · ${a.bankName}` }))
)

function blank() {
  return {
    bankName: '',
    direction: TRANSFER_DIRECTION.INBOUND,
    defaultAccountId: ''
  }
}

const { values, submitting, formError, submit, reset, fieldError } = useForm(blank(), (payload) =>
  // Always the same call: the endpoint upserts on (bank, direction), so editing
  // and creating are the same request and there is no id to send.
  bankDefaultsApi.save({
    bankName: payload.bankName,
    direction: payload.direction,
    defaultAccountId: payload.defaultAccountId
  })
)

const open = ref(props.modelValue)

watch(() => props.modelValue, (value) => {
  open.value = value

  if (value) {
    reset(props.bankDefault
      ? {
          bankName: props.bankDefault.bankName,
          direction: directionValue(props.bankDefault.direction),
          defaultAccountId: props.bankDefault.defaultAccountId
        }
      : blank())
  }
})

watch(open, (value) => emit('update:modelValue', value))

async function onSubmit() {
  const result = await submit()
  if (!result) return

  toast.success(t('bankDefaults.saved'))
  emit('saved', result)
  open.value = false
}
</script>

<template>
  <BaseModal
    v-model="open"
    :title="isEdit ? t('bankDefaults.editTitle') : t('bankDefaults.newTitle')"
  >
    <form id="bank-default-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <BaseInput
        v-model="values.bankName"
        :label="t('bankDefaults.bankName')"
        :placeholder="t('bankDefaults.bankNamePlaceholder')"
        required
        :error="fieldError('bankName')"
      />

      <BaseSelect
        v-model="values.direction"
        :label="t('bankDefaults.direction')"
        :options="directionOptions"
        required
        :hint="t('bankDefaults.directionHint')"
        :error="fieldError('direction')"
      />

      <BaseSelect
        v-model="values.defaultAccountId"
        :label="t('bankDefaults.account')"
        :options="accountOptions"
        :placeholder="t('bankDefaults.chooseAccount')"
        required
        :error="fieldError('defaultAccountId')"
      />
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="bank-default-form" variant="primary" :loading="submitting">
        {{ t('common.save') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>
