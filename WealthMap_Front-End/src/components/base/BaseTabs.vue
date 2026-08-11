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

<style scoped lang="scss" src="@/assets/styles/components/BaseTabs.scss"></style>
