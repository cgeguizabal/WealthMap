<script setup>
import { computed } from 'vue'

/**
 * Stroke icons drawn from a local path map. No icon library and no emoji:
 * one consistent 24px grid, 1.5px stroke, currentColor.
 */
const props = defineProps({
  name: { type: String, required: true },
  size: { type: [Number, String], default: 18 },
  strokeWidth: { type: [Number, String], default: 1.5 }
})

const ICONS = {
  // ── Navigation ────────────────────────────────
  home: ['M3 10.5 12 3l9 7.5', 'M5 9.5V20a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V9.5', 'M9.5 21v-6h5v6'],
  wallet: [
    'M3 6a2 2 0 0 1 2-2h12v3',
    'M3 6v12a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7a2 2 0 0 0-2-2H5a2 2 0 0 1-2-2z',
    'M17.5 13.5h.01'
  ],
  card: [
    'M4 5h16a2 2 0 0 1 2 2v10a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V7a2 2 0 0 1 2-2z',
    'M2 10h20'
  ],
  bag: [
    'M6 2 3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z',
    'M3 6h18',
    'M16 10a4 4 0 0 1-8 0'
  ],
  layers: ['m12 2 10 5-10 5L2 7z', 'm2 12 10 5 10-5', 'm2 17 10 5 10-5'],
  debt: ['m22 17-8.5-8.5-5 5L2 7', 'M16 17h6v-6'],
  target: [
    'M22 12a10 10 0 1 1-20 0 10 10 0 0 1 20 0',
    'M18 12a6 6 0 1 1-12 0 6 6 0 0 1 12 0',
    'M14 12a2 2 0 1 1-4 0 2 2 0 0 1 4 0'
  ],
  briefcase: [
    'M4 7h16a2 2 0 0 1 2 2v10a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V9a2 2 0 0 1 2-2z',
    'M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16'
  ],
  store: ['M3 9h18v11a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1z', 'M3 9 5 4h14l2 5', 'M9.5 21v-6h5v6'],
  report: [
    'M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z',
    'M14 2v6h6', 'M9 13h6', 'M9 17h5'
  ],
  bell: ['M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9', 'M13.73 21a2 2 0 0 1-3.46 0'],
  logout: ['M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4', 'm16 17 5-5-5-5', 'M21 12H9'],
  receipt: [
    'M5 2v20l2.5-2 2.5 2 2-2 2 2 2.5-2 2.5 2V2l-2.5 2L14 2l-2 2-2-2-2.5 2z',
    'M9 8h6', 'M9 12h6'
  ],

  // ── Actions & controls ────────────────────────
  'chevron-down': ['m6 9 6 6 6-6'],
  'chevron-up': ['m18 15-6-6-6 6'],
  'chevron-left': ['m15 18-6-6 6-6'],
  'chevron-right': ['m9 18 6-6-6-6'],
  x: ['M18 6 6 18', 'm6 6 12 12'],
  plus: ['M12 5v14', 'M5 12h14'],
  minus: ['M5 12h14'],
  check: ['M20 6 9 17l-5-5'],
  pencil: ['M17 3a2.83 2.83 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5z'],
  trash: [
    'M3 6h18',
    'M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6',
    'M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2'
  ],
  search: ['M19 11a8 8 0 1 1-16 0 8 8 0 0 1 16 0', 'm21 21-4.3-4.3'],
  filter: ['M22 3H2l8 9.46V19l4 2v-8.54z'],
  calendar: [
    'M5 4h14a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2z',
    'M16 2v4', 'M8 2v4', 'M3 10h18'
  ],
  download: ['M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4', 'm7 10 5 5 5-5', 'M12 15V3'],
  menu: ['M4 6h16', 'M4 12h16', 'M4 18h16'],
  refresh: ['M3 12a9 9 0 0 1 15.5-6.2L21 8', 'M21 3v5h-5', 'M21 12a9 9 0 0 1-15.5 6.2L3 16', 'M3 21v-5h5'],
  transfer: ['m17 2 4 4-4 4', 'M21 6H3', 'm7 22-4-4 4-4', 'M3 18h18'],
  lock: [
    'M5 11h14a1 1 0 0 1 1 1v8a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1v-8a1 1 0 0 1 1-1z',
    'M7.5 11V7a4.5 4.5 0 0 1 9 0v4'
  ],
  unlock: [
    'M5 11h14a1 1 0 0 1 1 1v8a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1v-8a1 1 0 0 1 1-1z',
    'M7.5 11V7a4.5 4.5 0 0 1 8.6-1.8'
  ],

  // ── Status & direction ────────────────────────
  alert: ['m21.7 18-8-14a2 2 0 0 0-3.4 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.7-3', 'M12 9v4', 'M12 17h.01'],
  info: ['M22 12a10 10 0 1 1-20 0 10 10 0 0 1 20 0', 'M12 16v-4', 'M12 8h.01'],
  'check-circle': ['M22 12a10 10 0 1 1-20 0 10 10 0 0 1 20 0', 'm8.5 12 2.5 2.5 4.5-5'],
  'arrow-up-right': ['M7 7h10v10', 'M7 17 17 7'],
  'arrow-down-left': ['M17 17H7V7', 'M17 7 7 17'],
  'arrow-right': ['M5 12h14', 'm12 5 7 7-7 7'],
  clock: ['M22 12a10 10 0 1 1-20 0 10 10 0 0 1 20 0', 'M12 6.5V12l3.5 2']
}

const paths = computed(() => ICONS[props.name] ?? [])
</script>

<template>
  <svg
    class="icon"
    :width="size"
    :height="size"
    viewBox="0 0 24 24"
    fill="none"
    :stroke-width="strokeWidth"
    stroke="currentColor"
    stroke-linecap="round"
    stroke-linejoin="round"
    aria-hidden="true"
    focusable="false"
  >
    <path v-for="(d, i) in paths" :key="i" :d="d" />
  </svg>
</template>

<style scoped lang="scss">
.icon {
  display: block;
  flex: none;
}
</style>
