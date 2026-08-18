<script setup>
import { useTheme } from '@/composables/useTheme'
import { useI18n } from '@/composables/useI18n'
import BaseIcon from '@/components/base/BaseIcon.vue'

/**
 * Three options as a segmented control, matching LanguageSelector.
 *
 * Icons rather than words: "Light / Dark / System" in two languages would be the
 * widest thing in the header, and a sun, a moon and a screen are understood
 * without reading. The label stays available as the accessible name and the
 * tooltip, so nothing is lost to someone who does not recognise them.
 */
const { theme, setTheme, themes } = useTheme()
const { t } = useI18n()
</script>

<template>
  <div class="theme">
    <span id="theme-label" class="theme__label">{{ t('theme.label') }}</span>

    <div class="theme__options" role="radiogroup" aria-labelledby="theme-label">
      <button
        v-for="option in themes"
        :key="option.value"
        class="theme__option"
        :class="{ 'is-active': theme === option.value }"
        type="button"
        role="radio"
        :aria-checked="theme === option.value"
        :aria-label="t(option.labelKey)"
        :title="t(option.labelKey)"
        @click="setTheme(option.value)"
      >
        <BaseIcon :name="option.icon" :size="15" />
      </button>
    </div>
  </div>
</template>

<style scoped lang="scss" src="@/assets/styles/layout/ThemeSelector.scss"></style>
