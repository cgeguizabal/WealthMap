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

<style scoped lang="scss">
.lang {
  display: flex;
  flex-direction: column;
  gap: var(--sp-2);
  padding: var(--sp-3) var(--sp-4);
}

.lang__label {
  font-size: var(--fs-xs);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-muted);
}

.lang__options {
  display: flex;
  gap: var(--sp-1);
  padding: 2px;
  border: var(--border);
  border-radius: var(--radius-sm);
  background: var(--canvas-alt);
}

.lang__option {
  flex: 1;
  padding: var(--sp-1) var(--sp-2);

  border: 1px solid transparent;
  border-radius: calc(var(--radius-sm) - 1px);
  background: transparent;
  color: var(--text-muted);
  font-size: var(--fs-xs);
  font-weight: var(--fw-medium);
  cursor: pointer;

  @include focus-ring;

  &:hover { color: var(--text); }

  &.is-active {
    background: var(--surface);
    border-color: var(--border-color);
    color: var(--text);
  }
}
</style>
