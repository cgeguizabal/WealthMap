<script setup>
import { RouterLink, useRoute } from 'vue-router'
import { NAV_GROUPS } from './navigation.js'
import { useI18n } from '@/composables/useI18n'
import BaseIcon from '@/components/base/BaseIcon.vue'

defineProps({
  /** Drives the mobile drawer; ignored at desktop widths. */
  open: { type: Boolean, default: false },
  /** Drives the desktop collapse; ignored at mobile widths. */
  collapsed: { type: Boolean, default: false },
  /** True when the sidebar is off-screen at the current width. */
  hidden: { type: Boolean, default: false }
})

const emit = defineEmits(['navigate', 'toggle'])
const route = useRoute()
const { t } = useI18n()

/** Exact for the dashboard, prefix elsewhere so detail routes keep the parent lit. */
function isActive(item) {
  if (item.exact) return route.path === item.path
  return route.path === item.path || route.path.startsWith(`${item.path}/`)
}
</script>

<template>
  <!-- `hidden` is decided by the shell because it depends on the viewport:
       collapsed at desktop widths, closed drawer at mobile ones. Without inert,
       links inside an off-screen sidebar stay in the tab order. -->
  <aside
    :class="['sidebar', { 'sidebar--open': open, 'sidebar--collapsed': collapsed }]"
    :inert="hidden || undefined"
  >
    <div class="sidebar__brand">
      <span class="sidebar__mark">WM</span>
      <span class="sidebar__wordmark">WealthMap</span>

      <button
        class="sidebar__toggle"
        type="button"
        :aria-label="collapsed ? t('nav.expand') : t('nav.collapse')"
        :title="collapsed ? t('nav.expand') : t('nav.collapse')"
        :aria-expanded="!collapsed"
        @click="emit('toggle')"
      >
        <BaseIcon name="menu" :size="18" />
      </button>
    </div>

    <nav class="sidebar__nav" :aria-label="t('nav.main')">
      <div v-for="(group, index) in NAV_GROUPS" :key="index" class="sidebar__group">
        <p v-if="group.labelKey" class="sidebar__group-label">{{ t(group.labelKey) }}</p>

        <RouterLink
          v-for="item in group.items"
          :key="item.path"
          :to="item.path"
          class="sidebar__link"
          :class="{ 'sidebar__link--active': isActive(item) }"
          :aria-current="isActive(item) ? 'page' : undefined"
          :title="collapsed ? t(item.labelKey) : undefined"
          @click="emit('navigate')"
        >
          <BaseIcon :name="item.icon" :size="17" />
          <!-- Hidden rather than removed when collapsed, so the accessible name survives -->
          <span class="sidebar__link-label">{{ t(item.labelKey) }}</span>
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
  flex: 1;
}

.sidebar__toggle {
  display: grid;
  place-items: center;
  width: 30px;
  height: 30px;
  flex: none;

  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  background: transparent;
  color: var(--text-muted);
  cursor: pointer;

  @include focus-ring;

  &:hover {
    background: var(--surface);
    border-color: var(--border-color);
    color: var(--ink);
  }
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

/* ── Desktop: collapses to an icon rail ─────
   Narrowed rather than hidden: navigation stays one click away, and the icons
   keep their position so muscle memory survives the collapse. */
@media (min-width: 1024px) {
  .sidebar { transition: width var(--dur) var(--ease); }

  .sidebar--collapsed {
    width: 64px;

    .sidebar__brand {
      flex-direction: column;
      gap: var(--sp-2);
      padding: var(--sp-4) var(--sp-2);
    }

    .sidebar__wordmark,
    .sidebar__group-label,
    .sidebar__link-label { display: none; }

    .sidebar__nav { padding: 0 var(--sp-2) var(--sp-6); }

    .sidebar__group { gap: var(--sp-1); }

    .sidebar__link {
      justify-content: center;
      padding: var(--sp-3) 0;
    }
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
