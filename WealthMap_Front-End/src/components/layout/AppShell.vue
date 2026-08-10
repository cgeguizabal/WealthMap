<script setup>
import { ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import AppSidebar from './AppSidebar.vue'
import AppHeader from './AppHeader.vue'

const route = useRoute()
const drawerOpen = ref(false)

// A drawer left open across a route change would cover the page just navigated to.
watch(() => route.path, () => { drawerOpen.value = false })

watch(drawerOpen, (open) => {
  document.body.style.overflow = open ? 'hidden' : ''
})
</script>

<template>
  <div class="shell">
    <AppSidebar :open="drawerOpen" @navigate="drawerOpen = false" />

    <Transition name="scrim">
      <div v-if="drawerOpen" class="shell__scrim" @click="drawerOpen = false" />
    </Transition>

    <div class="shell__main">
      <AppHeader :drawer-open="drawerOpen" @toggle-drawer="drawerOpen = !drawerOpen" />

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
