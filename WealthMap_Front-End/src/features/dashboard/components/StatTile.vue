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

<style scoped lang="scss">
.tile {
  display: flex;
  flex-direction: column;
  gap: var(--sp-2);

  padding: var(--sp-4) var(--sp-5);
  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
}

.tile__head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--sp-2);
}

.tile__label {
  font-size: var(--fs-xs);
  font-weight: var(--fw-semibold);
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--text-muted);
}

.tile__icon { color: var(--text-subtle); }

.tile__value {
  font-size: var(--fs-xl);
  font-weight: var(--fw-semibold);
  letter-spacing: -0.02em;
  line-height: 1.15;
}

.tile--positive .tile__value { color: var(--positive); }
.tile--negative .tile__value { color: var(--negative); }
.tile--accent .tile__value { color: var(--accent); }

.tile__detail {
  margin-top: auto;
  padding-top: var(--sp-2);
  border-top: var(--border-subtle);
  font-size: var(--fs-xs);
  color: var(--text-muted);
}

@media (max-width: 767px) {
  .tile { padding: var(--sp-3) var(--sp-4); }
  .tile__value { font-size: var(--fs-lg); }
}
</style>
