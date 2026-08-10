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

<style scoped lang="scss">
.card {
  @include flat-card;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.card--flush { box-shadow: none; }

.card__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--sp-4);

  padding: var(--sp-4) var(--sp-5);
  border-bottom: var(--border-subtle);
}

.card__title {
  font-size: var(--fs-md);
  font-weight: var(--fw-semibold);
  letter-spacing: -0.01em;
}

.card__subtitle {
  margin-top: 2px;
  font-size: var(--fs-sm);
  color: var(--text-muted);
}

.card__actions {
  display: flex;
  align-items: center;
  gap: var(--sp-2);
  flex: none;
}

.card__body--padded { padding: var(--sp-5); }

.card__footer {
  padding: var(--sp-3) var(--sp-5);
  border-top: var(--border-subtle);
  background: var(--canvas-alt);
}

@media (max-width: 767px) {
  .card__header { padding: var(--sp-3) var(--sp-4); flex-wrap: wrap; }
  .card__body--padded { padding: var(--sp-4); }
  .card__footer { padding: var(--sp-3) var(--sp-4); }
}
</style>
