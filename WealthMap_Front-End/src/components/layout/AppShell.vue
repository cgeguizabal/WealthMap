<script setup>
import { ref, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useMediaQuery, DESKTOP_QUERY } from '@/composables/useMediaQuery'
import AppSidebar from './AppSidebar.vue'
import AppHeader from './AppHeader.vue'
import OfflineBanner from './OfflineBanner.vue'

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

      <OfflineBanner />

      <main id="main-content" class="shell__content" tabindex="-1">
        <slot />
      </main>
    </div>
  </div>
</template>

<style scoped lang="scss" src="@/assets/styles/layout/AppShell.scss"></style>
