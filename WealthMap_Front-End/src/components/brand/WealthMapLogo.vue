<script setup>
import { computed } from 'vue'
import { useTheme } from '@/composables/useTheme'

import lightArtwork from '@/assets/Logo/Logo_LightMode.png'
import darkArtwork from '@/assets/Logo/Logo_DarkMode.png'

/**
 * The full wordmark: "WealthMap" over the tagline.
 *
 * Sized by width, because the artwork is 3.91:1 and the height follows. Callers
 * know how much horizontal room they have; none of them think in logo heights.
 *
 * A note on the tagline. It occupies the bottom fifth of the artwork, which puts
 * it at roughly 2% of the logo's width in cap height — legible at about 320px
 * wide, a grey smudge below about 200px. That is a property of the file rather
 * than of this component: the small placements here render the name clearly and
 * the tagline decoratively, which is the usual fate of a lockup used at two very
 * different scales. A wordmark-only asset would fix it if it ever matters.
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
