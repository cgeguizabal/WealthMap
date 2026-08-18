<script setup>
import { computed } from 'vue'
import { useTheme } from '@/composables/useTheme'

import lightArtwork from '@/assets/Logo/Logo_LightMode.png'
import darkArtwork from '@/assets/Logo/Logo_DarkMode.png'

/**
 * The WealthMap wordmark.
 *
 * Sized by width, and only by width: callers know how much horizontal room they
 * have, and none of them think in logo heights. The ratio is deliberately not
 * written down anywhere — not here either — because the artwork has already
 * changed shape once and a number in a comment goes stale as quietly as one in
 * a stylesheet.
 *
 * The height is deliberately not declared anywhere — see the stylesheet. It
 * comes from the file, so changing the artwork changes the shape and nothing
 * needs updating to match.
 *
 * Theme picked in JavaScript for the same reason as WealthMapIcon: the app has
 * three theme states and CSS can only see two of them.
 */
const props = defineProps({
  /**
   * Any CSS length. A number is pixels; a string is used verbatim, so `'100%'`
   * lets the parent decide. Height is always derived from the aspect ratio.
   */
  width: { type: [String, Number], default: null },

  /**
   * The logo carries the brand name, so unlike the icon it is content rather
   * than decoration and gets a real accessible name by default.
   */
  alt: { type: String, default: 'WealthMap' }
})

const { isDark } = useTheme()

const artwork = computed(() => (isDark.value ? darkArtwork : lightArtwork))

const sizing = computed(() => {
  if (props.width === null) return undefined

  return {
    '--logo-width': typeof props.width === 'number' ? `${props.width}px` : props.width
  }
})
</script>

<template>
  <img class="logo" :src="artwork" :alt="alt" :style="sizing" draggable="false" />
</template>

<style scoped lang="scss" src="@/assets/styles/components/WealthMapLogo.scss"></style>
