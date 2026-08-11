<script setup>
import { reactive, ref, computed } from 'vue'
import { useRouter, useRoute, RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import AuthShell from '../components/AuthShell.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const router = useRouter()
const route = useRoute()
const auth = useAuthStore()

const CURRENCIES = [
  { value: 'USD', label: 'USD — US Dollar' },
  { value: 'MXN', label: 'MXN — Mexican Peso' },
  { value: 'EUR', label: 'EUR — Euro' },
  { value: 'GBP', label: 'GBP — British Pound' },
  { value: 'CAD', label: 'CAD — Canadian Dollar' },
  { value: 'BRL', label: 'BRL — Brazilian Real' },
  { value: 'COP', label: 'COP — Colombian Peso' },
  { value: 'ARS', label: 'ARS — Argentine Peso' }
]

const values = reactive({
  fullName: '',
  email: '',
  password: '',
  country: '',
  currency: 'USD'
})

const fieldErrors = ref({})
const formError = ref(null)

const submitting = computed(() => auth.loading)

async function onSubmit() {
  fieldErrors.value = {}
  formError.value = null

  const ok = await auth.register({ ...values })

  if (!ok) {
    const error = auth.error
    if (error?.fields) fieldErrors.value = error.fields
    else formError.value = error?.message ?? 'Could not create your account.'
    return
  }

  // The API does not echo the currency back, so keep the one just chosen.
  auth.setCurrency(values.currency)

  const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/'
  router.replace(redirect)
}
</script>

<template>
  <AuthShell :title="t('auth.registerTitle')" :subtitle="t('auth.registerSubtitle')">
    <form class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <BaseInput
        v-model="values.fullName"
        :label="t('auth.fullName')"
        :placeholder="t('auth.fullNamePlaceholder')"
        autocomplete="name"
        required
        :error="fieldErrors.fullName"
      />

      <BaseInput
        v-model="values.email"
        :label="t('auth.email')"
        type="email"
        placeholder="you@example.com"
        autocomplete="email"
        required
        :error="fieldErrors.email"
      />

      <BaseInput
        v-model="values.password"
        :label="t('auth.password')"
        type="password"
        :placeholder="t('auth.passwordPlaceholder')"
        autocomplete="new-password"
        :hint="t('auth.passwordHint')"
        required
        :error="fieldErrors.password"
      />

      <BaseInput
        v-model="values.country"
        :label="t('auth.country')"
        :placeholder="t('auth.countryPlaceholder')"
        autocomplete="country-name"
        required
        :error="fieldErrors.country"
      />

      <!-- Full width: the descriptive option labels do not fit a half column
           in a 420px panel, and this is the one choice that cannot be changed
           later, so it is worth the room. -->
      <BaseSelect
        v-model="values.currency"
        :label="t('common.currency')"
        :options="CURRENCIES"
        required
        :hint="t('auth.currencyHint')"
        :error="fieldErrors.currency"
      />

      <BaseButton type="submit" variant="primary" size="lg" block :loading="submitting">
        {{ t('auth.signUp') }}
      </BaseButton>
    </form>

    <template #footer>
      {{ t('auth.haveAccount') }}
      <RouterLink :to="{ name: 'login', query: route.query }">{{ t('auth.signIn') }}</RouterLink>
    </template>
  </AuthShell>
</template>

<style scoped lang="scss">
.form {
  display: flex;
  flex-direction: column;
  gap: var(--sp-4);
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
