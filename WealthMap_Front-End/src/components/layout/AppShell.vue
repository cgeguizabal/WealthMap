<script setup>
import { ref, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useMediaQuery, DESKTOP_QUERY } from '@/composables/useMediaQuery'
import AppSidebar from './AppSidebar.vue'
import AppHeader from './AppHeader.vue'

const STORAGE_KEY = 'wm_sidebar_collapsed'

const route = useRoute()
const isDesktop = useMediaQuery(DESKTOP_QUERY)

/** Mobile: an overlay drawer. Desktop: a column that can be collapsed away. */
const drawerOpen = ref(false)
const collapsed = ref(localStorage.getItem(STORAGE_KEY) === 'true')

/**
 * Only ever off-screen on mobile. Collapsing at desktop widths narrows the
 * sidebar to an icon rail, so it stays visible and reachable.
 */
const sidebarHidden = computed(() => !isDesktop.value && !drawerOpen.value)

/** One button, two meanings — whichever the current viewport actually has. */
function toggleSidebar() {
  if (isDesktop.value) {
    collapsed.value = !collapsed.value
    return
  }

  drawerOpen.value = !drawerOpen.value
}

watch(collapsed, (value) => localStorage.setItem(STORAGE_KEY, String(value)))

// A drawer left open across a route change would cover the page just navigated to.
watch(() => route.path, () => { drawerOpen.value = false })

// Only the mobile drawer locks scroll; the desktop column never covers content.
watch(drawerOpen, (open) => {
  document.body.style.overflow = open ? 'hidden' : ''
})
</script>

<template>
  <div class="shell">
    <AppSidebar
      :open="drawerOpen"
      :collapsed="collapsed"
      :hidden="sidebarHidden"
      @navigate="drawerOpen = false"
      @toggle="toggleSidebar"
    />

    <Transition name="scrim">
      <div v-if="drawerOpen" class="shell__scrim" @click="drawerOpen = false" />
    </Transition>

    <div class="shell__main">
      <!-- Only needed on mobile, where the drawer really does leave the screen. -->
      <AppHeader :show-toggle="sidebarHidden" @toggle-drawer="toggleSidebar" />

      <main class="shell__content">
        <slot />
      </main>
    </div>
  </div>
</template>

<style scoped lang="scss">
.shell {
  display: flex;
  height: 100dvh;
  overflow: hidden;
}

.shell__main {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-width: 0;
}

.shell__content {
  flex: 1;
  overflow-y: auto;
  padding: var(--sp-6);
}

.shell__scrim {
  position: fixed;
  inset: 0;
  z-index: 55;
  background: rgba(32, 31, 29, 0.4);
}

.scrim-enter-active, .scrim-leave-active { transition: opacity var(--dur) var(--ease); }
.scrim-enter-from, .scrim-leave-to { opacity: 0; }

@media (min-width: 1024px) {
  .shell__scrim { display: none; }
}

@media (max-width: 1023px) {
  .shell__content { padding: var(--sp-4); }
}

@media (max-width: 767px) {
  .shell__content { padding: var(--sp-3); }
}

@media (prefers-reduced-motion: reduce) {
  .scrim-enter-active, .scrim-leave-active { transition: none; }
}
</style>
