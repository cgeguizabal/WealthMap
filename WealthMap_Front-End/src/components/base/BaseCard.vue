<script setup>
defineProps({
  title: { type: String, default: '' },
  subtitle: { type: String, default: '' },
  padded: { type: Boolean, default: true },
  flush: { type: Boolean, default: false }
})
</script>

<template>
  <section :class="['card', { 'card--flush': flush }]">
    <header v-if="title || $slots.header || $slots.actions" class="card__header">
      <div class="card__heading">
        <slot name="header">
          <h3 class="card__title">{{ title }}</h3>
          <p v-if="subtitle" class="card__subtitle">{{ subtitle }}</p>
        </slot>
      </div>

      <div v-if="$slots.actions" class="card__actions">
        <slot name="actions" />
      </div>
    </header>

    <div :class="['card__body', { 'card__body--padded': padded }]">
      <slot />
    </div>

    <footer v-if="$slots.footer" class="card__footer">
      <slot name="footer" />
    </footer>
  </section>
</template>

<style scoped lang="scss" src="./BaseCard.scss"></style>
