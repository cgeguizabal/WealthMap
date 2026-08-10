<script setup>
defineProps({
  title: { type: String, required: true },
  subtitle: { type: String, default: '' }
})
</script>

<template>
  <header class="page-header">
    <div class="page-header__text">
      <h1 class="page-header__title">{{ title }}</h1>
      <p v-if="subtitle || $slots.subtitle" class="page-header__subtitle">
        <slot name="subtitle">{{ subtitle }}</slot>
      </p>
    </div>

    <div v-if="$slots.actions" class="page-header__actions">
      <slot name="actions" />
    </div>
  </header>
</template>

<style scoped lang="scss">
.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--sp-4);
  margin-bottom: var(--sp-6);
}

.page-header__title {
  font-size: var(--fs-xl);
  font-weight: var(--fw-semibold);
  letter-spacing: -0.02em;
}

.page-header__subtitle {
  margin-top: var(--sp-1);
  font-size: var(--fs-sm);
  color: var(--text-muted);
  max-width: 68ch;
}

.page-header__actions {
  display: flex;
  align-items: center;
  gap: var(--sp-2);
  flex: none;
}

@media (max-width: 767px) {
  .page-header {
    flex-direction: column;
    align-items: stretch;
    gap: var(--sp-3);
    margin-bottom: var(--sp-4);
  }

  .page-header__actions { flex-wrap: wrap; }
}
</style>
