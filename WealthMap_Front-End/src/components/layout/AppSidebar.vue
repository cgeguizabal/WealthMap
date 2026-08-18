<script setup>
import { RouterLink, useRoute } from 'vue-router'
import { NAV_GROUPS } from './navigation.js'
import { useI18n } from '@/composables/useI18n'
import BaseIcon from '@/components/base/BaseIcon.vue'
import { LEGAL_ROUTES } from '@/config/legal'
import { VERSION_LABEL } from '@/config/app'

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

    <!-- Hidden when the rail is collapsed: at that width there is no room for
         text, and an icon for "Privacy Policy" would be a guess. -->
    <footer v-if="!collapsed" class="sidebar__legal">
      <span class="sidebar__version">{{ VERSION_LABEL }}</span>

      <RouterLink :to="LEGAL_ROUTES.privacy" @click="emit('navigate')">
        {{ t('legal.privacy') }}
      </RouterLink>
      <RouterLink :to="LEGAL_ROUTES.terms" @click="emit('navigate')">
        {{ t('legal.terms') }}
      </RouterLink>
    </footer>
  </aside>
</template>

<style scoped lang="scss" src="@/assets/styles/layout/AppSidebar.scss"></style>
