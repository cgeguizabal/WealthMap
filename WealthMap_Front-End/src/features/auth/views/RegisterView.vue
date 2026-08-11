<script setup>
import { reactive, ref, computed } from 'vue'
import { useRouter, useRoute, RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import AuthShell from '../components/AuthShell.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useI18n } from '@/composables/useI18n'

const { t, locale } = useI18n()

const router = useRouter()
const route = useRoute()
const auth = useAuthStore()

const CURRENCY_CODES = ['USD', 'MXN', 'EUR', 'GBP', 'CAD', 'BRL', 'COP', 'ARS']

/**
 * Currency names come from the browser's own locale data rather than a list
 * kept by hand in each language — "US Dollar" against "dólar estadounidense".
 * The code is kept in front of the name because that is what the rest of the
 * app displays, and Intl is skipped entirely if the runtime lacks DisplayNames.
 */
const CURRENCIES = computed(() => {
  let names = null

  try {
    names = new Intl.DisplayNames([locale.value], { type: 'currency' })
  } catch {
    names = null
  }

  return CURRENCY_CODES.map((code) => {
    const name = names?.of(code)
    return { value: code, label: name && name !== code ? `${code} — ${name}` : code }
  })
})

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

<style scoped lang="scss" src="./RegisterView.scss"></style>
