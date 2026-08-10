import { reactive, ref } from 'vue'

/**
 * Form state plus the API's two error shapes, already separated: field errors go
 * to the inputs, everything else to a form-level banner. `client.js` normalizes
 * the backend's PascalCase keys to camelCase, so `errors.pageSize` lines up with
 * a `pageSize` field without any mapping here.
 */
export function useForm(initialValues, submitFn) {
  const values = reactive({ ...initialValues })
  const errors = ref({})
  const formError = ref(null)
  const submitting = ref(false)

  function fieldError(name) {
    return errors.value[name] ?? null
  }

  function clearFieldError(name) {
    if (errors.value[name]) {
      const { [name]: _removed, ...rest } = errors.value
      errors.value = rest
    }
  }

  function reset(nextValues = initialValues) {
    Object.assign(values, { ...nextValues })
    errors.value = {}
    formError.value = null
  }

  /** Resolves to the API result on success, or `null` on failure. */
  async function submit() {
    submitting.value = true
    errors.value = {}
    formError.value = null

    try {
      return await submitFn({ ...values })
    } catch (err) {
      if (err?.fields) {
        errors.value = err.fields
      } else {
        formError.value = err?.message ?? 'Something went wrong.'
      }
      return null
    } finally {
      submitting.value = false
    }
  }

  return { values, errors, formError, submitting, submit, reset, fieldError, clearFieldError }
}
