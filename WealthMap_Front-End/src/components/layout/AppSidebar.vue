<script setup>
import { RouterLink, useRoute } from 'vue-router'
import { NAV_GROUPS } from './navigation.js'
import BaseIcon from '@/components/base/BaseIcon.vue'

defineProps({
  /** Drives the mobile drawer; ignored at desktop widths. */
  open: { type: Boolean, default: false }
})

const emit = defineEmits(['navigate'])
const route = useRoute()

/** Exact for the dashboard, prefix elsewhere so detail routes keep the parent lit. */
function isActive(item) {
  if (item.exact) return route.path === item.path
  return route.path === item.path || route.path.startsWith(`${item.path}/`)
}
</script>

<template>
  <aside :class="['sidebar', { 'sidebar--open': open }]">
    <div class="sidebar__brand">
      <span class="sidebar__mark">WM</span>
      <span class="sidebar__wordmark">WealthMap</span>
    </div>

    <nav class="sidebar__nav" aria-label="Main">
      <div v-for="(group, index) in NAV_GROUPS" :key="index" class="sidebar__group">
        <p v-if="group.label" class="sidebar__group-label">{{ group.label }}</p>

        <RouterLink
          v-for="item in group.items"
          :key="item.path"
          :to="item.path"
          class="sidebar__link"
          :class="{ 'sidebar__link--active': isActive(item) }"
          :aria-current="isActive(item) ? 'page' : undefined"
          @click="emit('navigate')"
        >
          <BaseIcon :name="item.icon" :size="17" />
          <span>{{ item.label }}</span>
        </RouterLink>
      </div>
    </nav>
  </aside>
</template>

<style scoped lang="scss">
.sidebar {
  display: flex;
  flex-direction: column;
  width: 232px;
  flex: none;
  height: 100dvh;

  background: var(--canvas-alt);
  border-right: var(--border);
  overflow-y: auto;
}

.sidebar__brand {
  display: flex;
  align-items: center;
  gap: var(--sp-3);
  padding: var(--sp-5);
  flex: none;
}

.sidebar__mark {
  display: grid;
  place-items: center;
  width: 28px;
  height: 28px;
  flex: none;

  background: var(--ink);
  color: var(--canvas);
  border-radius: var(--radius-sm);
  font-size: var(--fs-xs);
  font-weight: var(--fw-bold);
}

.sidebar__wordmark {
  font-size: var(--fs-md);
  font-weight: var(--fw-semibold);
  letter-spacing: -0.01em;
}

.sidebar__nav {
  display: flex;
  flex-direction: column;
  gap: var(--sp-5);
  padding: 0 var(--sp-3) var(--sp-6);
}

.sidebar__group { display: flex; flex-direction: column; gap: 2px; }

.sidebar__group-label {
  padding: 0 var(--sp-3);
  margin-bottom: var(--sp-2);

  font-size: var(--fs-xs);
  font-weight: var(--fw-semibold);
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--text-subtle);
}

.sidebar__link {
  display: flex;
  align-items: center;
  gap: var(--sp-3);

  padding: var(--sp-2) var(--sp-3);
  border-radius: var(--radius-sm);
  border: 1px solid transparent;

  color: var(--text-muted);
  font-size: var(--fs-base);
  font-weight: var(--fw-medium);
  text-decoration: none;

  @include focus-ring;

  &:hover {
    background: var(--surface);
    color: var(--text);
    text-decoration: none;
  }

  &--active {
    background: var(--surface);
    border-color: var(--border-color);
    color: var(--ink);
    font-weight: var(--fw-semibold);
    box-shadow: var(--shadow-sm);
  }
}

/* ── Mobile: off-canvas drawer ─────────────── */
@media (max-width: 1023px) {
  .sidebar {
    position: fixed;
    top: 0;
    left: 0;
    z-index: 60;
    width: 264px;

    transform: translateX(-100%);
    transition: transform var(--dur) var(--ease);
    box-shadow: var(--shadow-lg);
  }

  .sidebar--open { transform: translateX(0); }
}

@media (prefers-reduced-motion: reduce) {
  .sidebar { transition: none; }
}
</style>
