<script setup>
import { reactive, ref, computed } from 'vue'
import { useRouter, useRoute, RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import AuthShell from '../components/AuthShell.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'

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
  <AuthShell title="Create your account" subtitle="Takes a minute. Your data stays yours.">
    <form class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <BaseInput
        v-model="values.fullName"
        label="Full name"
        placeholder="Ada Lovelace"
        autocomplete="name"
        required
        :error="fieldErrors.fullName"
      />

      <BaseInput
        v-model="values.email"
        label="Email"
        type="email"
        placeholder="you@example.com"
        autocomplete="email"
        required
        :error="fieldErrors.email"
      />

      <BaseInput
        v-model="values.password"
        label="Password"
        type="password"
        placeholder="At least 8 characters"
        autocomplete="new-password"
        hint="Minimum 8 characters."
        required
        :error="fieldErrors.password"
      />

      <BaseInput
        v-model="values.country"
        label="Country"
        placeholder="Mexico"
        autocomplete="country-name"
        required
        :error="fieldErrors.country"
      />

      <!-- Full width: the descriptive option labels do not fit a half column
           in a 420px panel, and this is the one choice that cannot be changed
           later, so it is worth the room. -->
      <BaseSelect
        v-model="values.currency"
        label="Currency"
        :options="CURRENCIES"
        required
        hint="Every total in WealthMap is shown in this currency."
        :error="fieldErrors.currency"
      />

      <BaseButton type="submit" variant="primary" size="lg" block :loading="submitting">
        Create account
      </BaseButton>
    </form>

    <template #footer>
      Already have an account?
      <RouterLink :to="{ name: 'login', query: route.query }">Sign in</RouterLink>
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
