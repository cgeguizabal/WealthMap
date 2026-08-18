<script setup>
import { ref, watch, computed } from 'vue'
import { authApi } from '@/api/auth.api'
import { useForm } from '@/composables/useForm'
import { clearSession } from '@/api/session'

import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useI18n } from '@/composables/useI18n'

/**
 * The last thing a user does. Deliberately more work than any other action here.
 *
 * Two gates rather than one dialog: the password, which the API also demands,
 * and typing the word DELETE. The password proves who is asking; the typed word
 * proves they read the sentence above it. Neither alone stops the case this is
 * really for — a moment of frustration on a laptop that is already signed in.
 */
const { t } = useI18n()

const props = defineProps({
  modelValue: { type: Boolean, default: false }
})

const emit = defineEmits(['update:modelValue'])

/** Matched case-sensitively, and deliberately not translated — see the label. */
const CONFIRMATION_WORD = 'DELETE'

const typed = ref('')

const confirmed = computed(() => typed.value.trim() === CONFIRMATION_WORD)

const { values, submitting, formError, submit, reset, fieldError } = useForm(
  { password: '' },
  (payload) => authApi.deleteAccount(payload.password)
)

const open = ref(props.modelValue)

watch(() => props.modelValue, (value) => {
  open.value = value
  if (value) {
    reset({ password: '' })
    typed.value = ''
  }
})

watch(open, (value) => emit('update:modelValue', value))

async function onSubmit() {
  if (!confirmed.value) return

  const result = await submit()
  if (!result && formError.value) return

  // The account it named no longer exists, so there is nothing to sign out of.
  // Clearing the stored token is the whole of it — leaving it would reload into
  // a dashboard that could not fetch anything.
  clearSession()

  // A full reload rather than a route push, to leave no store holding data that
  // has just been deleted.
  window.location.href = '/login'
}
</script>

<template>
  <BaseModal v-model="open" :title="t('settings.deleteAccount')">
    <form id="delete-account-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <p class="danger-note">
        {{ t('settings.deleteAccountWarning') }}
      </p>

      <ul class="danger-list">
        <li>{{ t('settings.deleteAccountAccounts') }}</li>
        <li>{{ t('settings.deleteAccountHistory') }}</li>
        <li>{{ t('settings.deleteAccountImmediate') }}</li>
      </ul>

      <BaseInput
        v-model="values.password"
        :label="t('settings.deleteAccountPassword')"
        type="password"
        autocomplete="current-password"
        required
        :error="fieldError('password')"
      />

      <!--
        The word is not translated on purpose. It has to be typed exactly, and a
        translated target would be one more thing to get subtly wrong at the
        moment it matters least.
      -->
      <BaseInput
        v-model="typed"
        :label="t('settings.deleteAccountType', { word: CONFIRMATION_WORD })"
        :placeholder="CONFIRMATION_WORD"
        autocomplete="off"
      />
    </form>

    <template #footer>
      <BaseButton variant="ghost" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton
        type="submit"
        form="delete-account-form"
        variant="danger"
        :loading="submitting"
        :disabled="!confirmed || !values.password"
      >
        {{ t('settings.deleteAccountConfirm') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss" src="@/assets/styles/features/settings/DeleteAccountModal.scss"></style>
