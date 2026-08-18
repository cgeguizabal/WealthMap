<script setup>
import { ref, onMounted } from 'vue'
import { usersApi } from '@/api/users.api'
import { useForm } from '@/composables/useForm'
import { useToast } from '@/composables/useToast'
import { useAuthStore } from '@/stores/auth.store'
import { useDashboardStore } from '@/stores/dashboard.store'

import BaseCard from '@/components/base/BaseCard.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseSpinner from '@/components/base/BaseSpinner.vue'
import { useI18n } from '@/composables/useI18n'
import { CURRENCY_OPTIONS as CURRENCIES } from '@/config/currencies'

/**
 * Name, country and reporting currency — the details a person can correct about
 * themselves.
 *
 * Email is shown but not editable. It identifies the account and carries a
 * unique index derived from it, so changing it is a different operation with its
 * own failure mode rather than a field on this form.
 */
const { t } = useI18n()

const toast = useToast()
const auth = useAuthStore()
const dashboard = useDashboardStore()

const loading = ref(true)
const email = ref('')

const { values, submitting, formError, submit, reset, fieldError } = useForm(
  { fullName: '', country: '', currency: 'USD' },
  (payload) => usersApi.updateMe(payload)
)

onMounted(async () => {
  try {
    const profile = await usersApi.me()
    email.value = profile.email
    reset({
      fullName: profile.fullName,
      country: profile.country,
      currency: profile.currency
    })
  } catch (err) {
    toast.error(err.message)
  } finally {
    loading.value = false
  }
})

async function onSubmit() {
  const result = await submit()
  if (!result) return

  // The header shows the name and initials, and every total is labelled in the
  // reporting currency — both are stale the moment this succeeds.
  auth.setProfile({ fullName: result.fullName, currency: result.currency })
  dashboard.load()

  toast.success(t('settings.profileSaved'))
}
</script>

<template>
  <BaseCard :title="t('settings.profile')" :subtitle="t('settings.profileSubtitle')">
    <div v-if="loading" class="profile__loading"><BaseSpinner :size="20" /></div>

    <form v-else id="profile-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <BaseInput
        :model-value="email"
        :label="t('auth.email')"
        disabled
        :hint="t('settings.emailNotEditable')"
      />

      <BaseInput
        v-model="values.fullName"
        :label="t('auth.fullName')"
        required
        :error="fieldError('fullName')"
      />

      <BaseInput
        v-model="values.country"
        :label="t('auth.country')"
        required
        :error="fieldError('country')"
      />

      <!-- Changing this converts nothing. It decides which accounts are summed
           into the totals, since other currencies are excluded rather than
           converted — worth saying on the form, not just in the guide. -->
      <BaseSelect
        v-model="values.currency"
        :label="t('common.currency')"
        :options="CURRENCIES"
        required
        :hint="t('settings.currencyHint')"
        :error="fieldError('currency')"
      />

      <div class="form__actions">
        <BaseButton type="submit" variant="primary" :loading="submitting">
          {{ t('common.save') }}
        </BaseButton>
      </div>
    </form>
  </BaseCard>
</template>

<style scoped lang="scss" src="@/assets/styles/features/settings/ProfileCard.scss"></style>
