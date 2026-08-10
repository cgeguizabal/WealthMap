<script setup>
defineProps({
  modelValue: { type: [String, Number], required: true },
  /** `[{ value, label, count? }]` */
  tabs: { type: Array, required: true }
})

defineEmits(['update:modelValue'])
</script>

<template>
  <div class="tabs" role="tablist">
    <button
      v-for="tab in tabs"
      :key="tab.value"
      class="tabs__tab"
      :class="{ 'tabs__tab--active': tab.value === modelValue }"
      type="button"
      role="tab"
      :aria-selected="tab.value === modelValue"
      @click="$emit('update:modelValue', tab.value)"
    >
      {{ tab.label }}
      <span v-if="tab.count !== undefined" class="tabs__count numeric">{{ tab.count }}</span>
    </button>
  </div>
</template>

<style scoped lang="scss">
.tabs {
  display: flex;
  gap: var(--sp-1);
  border-bottom: var(--border-subtle);
  overflow-x: auto;
  scrollbar-width: none;

  &::-webkit-scrollbar { display: none; }
}

.tabs__tab {
  display: inline-flex;
  align-items: center;
  gap: var(--sp-2);

  padding: var(--sp-3) var(--sp-4);
  border: none;
  border-bottom: 2px solid transparent;
  background: transparent;

  font-size: var(--fs-base);
  font-weight: var(--fw-medium);
  color: var(--text-muted);
  white-space: nowrap;
  cursor: pointer;

  @include focus-ring;

  &:hover { color: var(--text); }

  &--active {
    color: var(--ink);
    border-bottom-color: var(--ink);
  }
}

.tabs__count {
  padding: 1px var(--sp-2);
  border-radius: var(--radius-sm);
  background: var(--canvas-alt);
  font-size: var(--fs-xs);
}
</style>
