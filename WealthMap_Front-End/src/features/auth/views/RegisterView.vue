<script setup>
import { reactive, ref, computed } from 'vue'
import { useRouter, useRoute, RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import AuthShell from '../components/AuthShell.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useI18n } from '@/composables/useI18n'
import { POLICY_VERSION, LEGAL_ROUTES } from '@/config/legal'

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

const acceptedTerms = ref(false)

const fieldErrors = ref({})
const formError = ref(null)

const submitting = computed(() => auth.loading)

/**
 * The consent sentence, split around its two link placeholders.
 *
 * Built this way rather than concatenated in the template because word order
 * differs between languages — "I accept the {terms} and the {privacy}" against
 * "Acepto los {terms} y la {privacy}" — and hard-coding the order in markup
 * would quietly mistranslate one of them.
 */
const consentParts = computed(() =>
  t('auth.acceptTerms')
    .split(/(\{terms\}|\{privacy\})/)
    .filter((part) => part !== '')
)

/**
 * The same sentence with the placeholders resolved, as the checkbox's
 * accessible name. The visible text cannot serve that purpose here: wrapping it
 * in a label would swallow clicks meant for the two links.
 */
const consentLabel = computed(() =>
  t('auth.acceptTerms')
    .replace('{terms}', t('legal.terms'))
    .replace('{privacy}', t('legal.privacy'))
)

async function onSubmit() {
  fieldErrors.value = {}
  formError.value = null

  // Checked here as well as by the API. The server is the authority — a client
  // that skipped this would still be refused — but a round trip to be told
  // about a checkbox on screen is a poor way to find out.
  if (!acceptedTerms.value) {
    fieldErrors.value = { acceptedTerms: t('auth.acceptTermsRequired') }
    return
  }

  const ok = await auth.register({
    ...values,
    acceptedTerms: acceptedTerms.value,
    policyVersion: POLICY_VERSION
  })

  if (!ok) {
    const error = auth.error
    if (error?.fields) fieldErrors.value = error.fields
    else formError.value = error?.message ?? 'Could not create your account.'
    return
  }

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

      <div class="consent">
        <!--
          Not a <label> wrapping the sentence, which is the obvious markup and is
          wrong here: a label forwards clicks to its control, so clicking "Terms
          of Service" toggled the checkbox instead of opening the document. The
          checkbox gets its accessible name from aria-label instead, and the
          links are left to behave like links.
        -->
        <div class="consent__row">
          <input
            v-model="acceptedTerms"
            type="checkbox"
            class="consent__box"
            :aria-label="consentLabel"
          />

          <span class="consent__text">
            <template v-for="(part, index) in consentParts" :key="index">
              <RouterLink v-if="part === '{terms}'" :to="LEGAL_ROUTES.terms" target="_blank">
                {{ t('legal.terms') }}
              </RouterLink>
              <RouterLink
                v-else-if="part === '{privacy}'"
                :to="LEGAL_ROUTES.privacy"
                target="_blank"
              >
                {{ t('legal.privacy') }}
              </RouterLink>
              <template v-else>{{ part }}</template>
            </template>
          </span>
        </div>

        <p v-if="fieldErrors.acceptedTerms" class="consent__error" role="alert">
          {{ fieldErrors.acceptedTerms }}
        </p>
      </div>

      <!-- Disabled until the box is ticked, so the requirement is visible before
           the click rather than after it. -->
      <BaseButton
        type="submit"
        variant="primary"
        size="lg"
        block
        :loading="submitting"
        :disabled="!acceptedTerms"
      >
        {{ t('auth.signUp') }}
      </BaseButton>
    </form>

    <template #footer>
      {{ t('auth.haveAccount') }}
      <RouterLink :to="{ name: 'login', query: route.query }">{{ t('auth.signIn') }}</RouterLink>
    </template>
  </AuthShell>
</template>

<style scoped lang="scss" src="@/assets/styles/features/auth/RegisterView.scss"></style>
