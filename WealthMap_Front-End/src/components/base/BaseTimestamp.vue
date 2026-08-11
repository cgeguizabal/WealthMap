<script setup>
import { computed } from 'vue'
import { useDateTime } from '@/composables/useDateTime'

/**
 * A timestamp in a table cell: date on top, time beneath it.
 *
 * Stacked rather than inline so the hour can be added without widening every
 * date column, and so the date stays the thing you scan down the page.
 */
const props = defineProps({
  value: { type: [String, Date], default: null },
  withYear: { type: Boolean, default: true }
})

const { formatDate, formatTime } = useDateTime()

const date = computed(() => formatDate(props.value, { withYear: props.withYear }))
const time = computed(() => formatTime(props.value))
</script>

<template>
  <span class="ts numeric">
    <span class="ts__date">{{ date }}</span>
    <span v-if="time" class="ts__time">{{ time }}</span>
  </span>
</template>

<style scoped lang="scss" src="@/assets/styles/components/BaseTimestamp.scss"></style>
