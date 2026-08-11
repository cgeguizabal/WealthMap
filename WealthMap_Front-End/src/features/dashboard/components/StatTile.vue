<script setup>
import BaseIcon from '@/components/base/BaseIcon.vue'

defineProps({
  label: { type: String, required: true },
  value: { type: String, required: true },
  icon: { type: String, default: '' },
  tone: {
    type: String,
    default: 'neutral',
    validator: (v) => ['neutral', 'positive', 'negative', 'accent'].includes(v)
  }
})
</script>

<template>
  <article :class="['tile', `tile--${tone}`]">
    <header class="tile__head">
      <span class="tile__label">{{ label }}</span>
      <BaseIcon v-if="icon" :name="icon" :size="16" class="tile__icon" />
    </header>

    <p class="tile__value numeric">{{ value }}</p>

    <div v-if="$slots.default" class="tile__detail">
      <slot />
    </div>
  </article>
</template>

<style scoped lang="scss" src="./StatTile.scss"></style>
