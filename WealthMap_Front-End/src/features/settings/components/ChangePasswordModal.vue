<script setup>
import { ref, watch, computed } from 'vue'
import { usersApi } from '@/api/users.api'
import { useForm } from '@/composables/useForm'
import { clearSession } from '@/api/session'

import BaseModal from '@/components/base/BaseModal.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useI18n } from '@/composables/useI18n'

/**
 * Changing a password ends every session, so this modal signs the user out on
 * success rather than pretending nothing happened.
 *
 * That is deliberate and worth the friction. Someone changes their password
 * because they think a session has been taken; leaving the other sessions alive
 * would change the lock and leave the intruder a key that still works, since a
 * refresh token does not care what the password is.
 */
const { t } = useI18n()

const props = defineProps({
  modelValue: { type: Boolean, default: false }
})

const emit = defineEmits(['update:modelValue'])

const confirmation = ref('')

const mismatch = computed(() =>
  confirmation.value.length > 0 && confirmation.value !== values.newPassword
)

const { values, submitting, formError, submit, reset, fieldError } = useForm(
  { currentPassword: '', newPassword: '' },
  (payload) => usersApi.changePassword(payload.currentPassword, payload.newPassword)
)

const open = ref(props.modelValue)

watch(() => props.modelValue, (value) => {
  open.value = value
  if (value) {
    reset({ currentPassword: '', newPassword: '' })
    confirmation.value = ''
  }
})

watch(open, (value) => emit('update:modelValue', value))

async function onSubmit() {
  if (mismatch.value || !confirmation.value) return

  const result = await submit()
  if (!result && formError.value) return

  // Every session just ended, this one included. Clearing here rather than
  // leaving it to the next failed request: the token in localStorage is already
  // dead, and a reload that still looked signed in would land on the dashboard
  // and only bounce to login once something 401'd.
  clearSession()

  // A full reload rather than a route push, so no store is left holding data
  // fetched with a session that no longer exists.
  window.location.href = '/login'
}
</script>

<template>
  <BaseModal v-model="open" :title="t('settings.changePassword')">
    <form id="change-password-form" class="form" novalidate @submit.prevent="onSubmit">
      <p v-if="formError" class="form__error" role="alert">{{ formError }}</p>

      <p class="form__note">{{ t('settings.changePasswordNote') }}</p>

      <BaseInput
        v-model="values.currentPassword"
        :label="t('settings.currentPassword')"
        type="password"
        autocomplete="current-password"
        required
        :error="fieldError('currentPassword')"
      />

      <BaseInput
        v-model="values.newPassword"
        :label="t('settings.newPassword')"
        type="password"
        autocomplete="new-password"
        required
        :hint="t('auth.passwordHint')"
        :error="fieldError('newPassword')"
      />

      <BaseInput
        v-model="confirmation"
        :label="t('settings.confirmPassword')"
        type="password"
        autocomplete="new-password"
        required
        :error="mismatch ? t('settings.passwordsDoNotMatch') : undefined"
      />
    </form>

    <template #footer>
      <BaseButton variant="ghost" @click="open = false">{{ t('common.cancel') }}</BaseButton>
      <BaseButton
        type="submit"
        form="change-password-form"
        variant="primary"
        :loading="submitting"
        :disabled="mismatch || !confirmation || !values.currentPassword || !values.newPassword"
      >
        {{ t('settings.changePassword') }}
      </BaseButton>
    </template>
  </BaseModal>
</template>

<style scoped lang="scss" src="@/assets/styles/features/settings/ProfileCard.scss"></style>
