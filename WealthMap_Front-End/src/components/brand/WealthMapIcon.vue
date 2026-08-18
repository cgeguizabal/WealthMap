<script setup>
import { computed } from 'vue'
import { useTheme } from '@/composables/useTheme'

import lightArtwork from '@/assets/Icon/Icon_LightMode.png'
import darkArtwork from '@/assets/Icon/Icon_DarkMode.png'

/**
 * The WealthMap mark on its own tile.
 *
 * The artwork is picked in JavaScript rather than by a CSS media query, because
 * the theme has three states and only two of them are visible to CSS. A user who
 * has explicitly chosen light on a machine set to dark is following
 * `data-theme`, not `prefers-color-scheme`; `isDark` from the theme store has
 * already resolved all three, including reacting when the OS flips while
 * "system" is selected.
 *
 * The artwork is not square, so it is contained inside the square tile rather
 * than filling it. Nothing here scales or crops the mark, and nothing records
 * its dimensions — the logo component learned that lesson the hard way.
 */
const props = defineProps({
  /**
   * Any CSS length. A number is treated as pixels; a string is used verbatim, so
   * `'100%'` or `'6rem'` work and let a parent decide.
   *
   * Left off, the tile sizes itself against the viewport, which is what makes it
   * usable from a phone up to a desktop without a breakpoint.
   */
  size: { type: [String, Number], default: null },

  /**
   * Empty by default: the mark almost always sits beside the wordmark, where
   * announcing it repeats what the text already says. Pass a description on the
   * rare occasion it stands alone.
   */
  alt: { type: String, default: '' }
})

const { isDark } = useTheme()

const artwork = computed(() => (isDark.value ? darkArtwork : lightArtwork))

const sizing = computed(() => {
  if (props.size === null) return undefined

  return {
    '--tile-size': typeof props.size === 'number' ? `${props.size}px` : props.size
  }
})
</script>

<template>
  <span class="tile" :style="sizing">
    <img class="tile__art" :src="artwork" :alt="alt" draggable="false" />
  </span>
</template>

<style scoped lang="scss" src="@/assets/styles/components/WealthMapIcon.scss"></style>
