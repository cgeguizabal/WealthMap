<script setup>
import { ref, watch, computed } from 'vue'
import { jobsApi, DEDUCTION_TYPE, DEDUCTION_TYPE_OPTIONS, computeNet } from '@/api/jobs.api'
import { useForm } from '@/composables/useForm'
import { useToast } from '@/composables/useToast'
import { useMoney } from '@/composables/useMoney'

import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

/** Values stay the API's integers; only the wording follows the locale. */
const typeOptions = computed(() =>
  DEDUCTION_TYPE_OPTIONS.map((o) => ({
    ...o,
    label: t(o.value === DEDUCTION_TYPE.PERCENTAGE ? 'job.percentageOfGross' : 'job.fixedAmount')
  }))
)

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  job: { type: Object, required: true },
  deduction: { type: Object, default: null }
})

const emit = defineEmits(['update:modelValue', 'saved'])

const toast = useToast()
const { format } = useMoney()

const isEdit = computed(() => props.deduction !== null)

const { values, submitting, formError, submit, reset, fieldError } = useForm(
  { name: '', type: DEDUCTION_TYPE.FIXED, value: null },
  (payload) => {
    const body = { name: payload.name, type: payload.type, value: payload.value }

    return isEdit.value
      ? jobsApi.updateDeduction(props.job.id, props.deduction.id, body)
      : jobsApi.addDeduction(props.job.id, body)
  }
)

const isPercentage = computed(() => values.type === DEDUCTION_TYPE.PERCENTAGE)

/**
 * What the job would look like with this deduction applied — computed locally so
 * the consequence is visible before saving.
 */
const preview = computed(() => {
  if (!values.value || !props.job) return null

  const others = props.job.deductions.filter((d) => d.id !== props.deduction?.id)
  const draft = [...others, { type: values.type, value: values.value }]

  const net = computeNet(props.job.grossMonthlySalary, draft)
  const current = computeNet(props.job.grossMonthlySalary, others)

  return { net, reduction: current - net, isNegative: net < 0 }
})

const open = ref(props.modelValue)
watch(() => props.modelValue, (value) => {
  open.value = value

  if (value) {
    reset(props.deduction
      ? {
          name: props.deduction.name,
          type: props.deduction.type === 'Percentage' ? DEDUCTION_TYPE.PERCENTAGE : DEDUCTION_TYPE.FIXED,
          value: props.deduction.value
        }
      : { name: '', type: DEDUCTION_TYPE.FIXED, value: null })
  }
})
watch(open, (value) => emit('update:modelValue', value))

async function onSubmit() {
  const job = await submit()
  if (!job) return

  toast.success(isEdit.value ? 'Deduction updated.' : `${values.name} added.`)
  emit('saved', job)
  open.value = false
}
</script>

<template>
  <BaseModal v-model="open" :title="isEdit ? t('job.editDeduction') : t('job.addDeduction')" size="sm">
    <form id="deduction-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <BaseInput
        v-model="values.name"
        :label="t('common.name')"
        :placeholder="t('job.deductionNamePlaceholder')"
        required
        :hint="t('job.deductionNameHint')"
        :error="fieldError('name')"
      />

      <BaseSelect
        v-model="values.type"
        :label="t('common.type')"
        :options="typeOptions"
        required
        :error="fieldError('type')"
      />

      <BaseInput
        v-model="values.value"
        :label="isPercentage ? t('job.percentage') : t('common.amount')"
        type="number"
        step="0.01"
        min="0"
        :max="isPercentage ? 100 : undefined"
        required
        :error="fieldError('value')"
      >
        <template #prefix>{{ isPercentage ? '' : job?.currency }}</template>
        <template #suffix>{{ isPercentage ? '%' : '' }}</template>
      </BaseInput>

      <div v-if="preview" class="preview" :class="{ 'preview--invalid': preview.isNegative }">
        <div class="preview__row">
          <span>{{ t('job.takesOff') }}</span>
          <span class="numeric">−{{ format(preview.reduction, { currency: job.currency }) }}</span>
        </div>
        <div class="preview__row preview__row--total">
          <span>{{ t('job.netBecomes') }}</span>
          <span class="numeric">{{ format(preview.net, { currency: job.currency }) }}</span>
        </div>
        <p v-if="preview.isNegative" class="preview__warning">
          {{ t('job.deductionsExceed') }}
        </p>
      </div>
    </form>

    <template #footer>
      <BaseButton variant="secondary" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton type="submit" form="deduction-form" variant="primary" :loading="submitting">
        {{ isEdit ? t('common.save') : t('job.addDeduction') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss">
.form { display: flex; flex-direction: column; gap: var(--sp-4); }

.preview {
  display: flex;
  flex-direction: column;
  gap: var(--sp-2);

  padding: var(--sp-3) var(--sp-4);
  background: var(--canvas-alt);
  border: var(--border-subtle);
  border-radius: var(--radius);

  &--invalid {
    background: var(--negative-soft);
    border-color: var(--negative);
  }
}

.preview__row {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: var(--sp-3);
  font-size: var(--fs-sm);
  color: var(--text-muted);
}

.preview__row--total {
  padding-top: var(--sp-2);
  border-top: var(--border-subtle);
  color: var(--text);
  font-weight: var(--fw-semibold);
}

.preview__warning { font-size: var(--fs-xs); color: var(--negative); }

.form__error {
  padding: var(--sp-3);
  border: 1px solid var(--negative);
  border-radius: var(--radius);
  background: var(--negative-soft);
  color: var(--negative);
  font-size: var(--fs-sm);
}
</style>
