<script setup>
import { ref, watch, computed } from 'vue'
import { freelanceJobsApi } from '@/api/freelanceJobs.api'
import { useForm } from '@/composables/useForm'
import { useToast } from '@/composables/useToast'
import { useAuthStore } from '@/stores/auth.store'

import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useI18n } from '@/composables/useI18n'
import { CURRENCY_OPTIONS as CURRENCIES } from '@/config/currencies'

const { t } = useI18n()

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  job: { type: Object, default: null }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const auth = useAuthStore()

const isEdit = computed(() => props.job !== null)

function blank() {
  return {
    title: '',
    client: '',
    agreedAmount: null,
    currency: auth.currency,
    dueOn: '',
    notes: ''
  }
}

const { values, submitting, formError, submit, reset, fieldError } = useForm(blank(), (payload) => {
  const body = {
    title: payload.title,
    client: payload.client || null,
    agreedAmount: payload.agreedAmount,
    currency: payload.currency,
    // An empty date input is '', which the API would reject as a malformed date.
    dueOn: payload.dueOn || null,
    notes: payload.notes || null
  }

  return isEdit.value
    ? freelanceJobsApi.update(props.job.id, body)
    : freelanceJobsApi.create(body)
})

const open = ref(props.modelValue)

watch(() => props.modelValue, (value) => {
  open.value = value

  if (value) {
    reset(props.job
      ? {
          title: props.job.title,
          client: props.job.client ?? '',
          agreedAmount: props.job.agreedAmount,
          currency: props.job.currency,
          dueOn: props.job.dueOn ?? '',
          notes: props.job.notes ?? ''
        }
      : blank())
  }
})

watch(open, (value) => emit('update:modelValue', value))

async function onSubmit() {
  const result = await submit()
  if (!result) return

  toast.success(isEdit.value ? t('freelance.updated') : t('freelance.added', { title: result.title }))
  emit('saved', result)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" :title="isEdit ? t('freelance.editWork') : t('freelance.newWork')">
    <form id="freelance-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <BaseInput
        v-model="values.title"
        :label="t('freelance.workTitle')"
        :placeholder="t('freelance.workTitlePlaceholder')"
        required
        :error="fieldError('title')"
      />

      <BaseInput
        v-model="values.client"
        :label="t('freelance.client')"
        :placeholder="t('freelance.clientPlaceholder')"
        :hint="t('freelance.clientHint')"
        :error="fieldError('client')"
      />

      <div class="form__row">
        <BaseInput
          v-model="values.agreedAmount"
          :label="t('freelance.agreedAmount')"
          type="number"
          step="0.01"
          min="0"
          required
          :hint="t('freelance.agreedAmountHint')"
          :error="fieldError('agreedAmount')"
        />

        <!-- Currency is fixed after creation: the API refuses to change it, since
             an amount already agreed in one currency cannot be reinterpreted. -->
        <BaseSelect
          v-model="values.currency"
          :label="t('common.currency')"
          :options="CURRENCIES"
          required
          :disabled="isEdit"
          :error="fieldError('currency')"
        />
      </div>

      <BaseInput
        v-model="values.dueOn"
        :label="t('freelance.dueOn')"
        type="date"
        :hint="t('freelance.dueOnHint')"
        :error="fieldError('dueOn')"
      />

      <BaseInput
        v-model="values.notes"
        :label="t('common.notes')"
        :placeholder="t('freelance.notesPlaceholder')"
        :error="fieldError('notes')"
      />
    </form>

    <template #footer>
      <BaseButton variant="ghost" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="freelance-form" variant="primary" :loading="submitting">
        {{ isEdit ? t('common.save') : t('freelance.addWork') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss" src="@/assets/styles/features/job/JobFormModal.scss"></style>
