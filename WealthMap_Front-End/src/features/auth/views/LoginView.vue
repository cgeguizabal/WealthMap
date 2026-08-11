<script setup>
import { reactive, ref, computed } from 'vue'
import { useRouter, useRoute, RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import { useI18n } from '@/composables/useI18n'
import AuthShell from '../components/AuthShell.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseButton from '@/components/base/BaseButton.vue'

const { t } = useI18n()
const router = useRouter()
const route = useRoute()
const auth = useAuthStore()

const values = reactive({ email: '', password: '' })
const fieldErrors = ref({})
const formError = ref(null)

const submitting = computed(() => auth.loading)

async function onSubmit() {
  fieldErrors.value = {}
  formError.value = null

  const ok = await auth.login({ ...values })

  if (!ok) {
    // The store keeps the normalized error; field errors go to the inputs and
    // anything else (including wrong credentials) to the banner.
    const error = auth.error
    if (error?.fields) fieldErrors.value = error.fields
    else formError.value = error?.message ?? 'Could not sign you in.'
    return
  }

  // Honour where the guard wanted to send them before the redirect to login.
  const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/'
  router.replace(redirect)
}
</script>

<template>
  <AuthShell :title="t('auth.loginTitle')" :subtitle="t('auth.loginSubtitle')">
    <form class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

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
        placeholder="••••••••"
        autocomplete="current-password"
        required
        :error="fieldErrors.password"
      />

      <BaseButton type="submit" variant="primary" size="lg" block :loading="submitting">
        {{ t('auth.signIn') }}
      </BaseButton>
    </form>

    <template #footer>
      {{ t('auth.noAccount') }}
      <RouterLink :to="{ name: 'register', query: route.query }">{{ t('auth.createOne') }}</RouterLink>
    </template>
  </AuthShell>
</template>

<style scoped lang="scss" src="@/assets/styles/features/auth/LoginView.scss"></style>
