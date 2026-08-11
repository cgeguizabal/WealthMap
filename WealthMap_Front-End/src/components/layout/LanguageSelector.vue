<script setup>
import { useI18n } from '@/composables/useI18n'

/**
 * Two languages, so a segmented control rather than a dropdown: both options
 * stay visible and switching is one click instead of open-then-pick.
 *
 * Each option is labelled in its own language — "Español" reads as Spanish to
 * someone who cannot yet read the interface, which is exactly who needs it.
 */
const { t, locale, setLocale, locales } = useI18n()
</script>

<template>
  <div class="lang">
    <span :id="'lang-label'" class="lang__label">{{ t('language.label') }}</span>

    <div class="lang__options" role="radiogroup" aria-labelledby="lang-label">
      <button
        v-for="option in locales"
        :key="option.value"
        class="lang__option"
        :class="{ 'is-active': locale === option.value }"
        type="button"
        role="radio"
        :aria-checked="locale === option.value"
        @click="setLocale(option.value)"
      >
        {{ t(option.labelKey) }}
      </button>
    </div>
  </div>
</template>

<style scoped lang="scss" src="@/assets/styles/layout/LanguageSelector.scss"></style>
