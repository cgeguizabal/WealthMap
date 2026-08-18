<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { RouterLink } from 'vue-router'
import { storeToRefs } from 'pinia'
import { useAuthStore } from '@/stores/auth.store'
import { useNotificationsStore } from '@/stores/notifications.store'
import { useDashboardStore } from '@/stores/dashboard.store'
import { useI18n } from '@/composables/useI18n'
import BaseIcon from '@/components/base/BaseIcon.vue'
import LanguageSelector from './LanguageSelector.vue'
import WealthMapIcon from '@/components/brand/WealthMapIcon.vue'
import WealthMapLogo from '@/components/brand/WealthMapLogo.vue'
import ThemeSelector from './ThemeSelector.vue'

defineProps({
  /** Only shown when the sidebar is off-screen — otherwise its own toggle is visible. */
  showToggle: { type: Boolean, default: false }
})

const emit = defineEmits(['toggle-drawer'])

const { t } = useI18n()
const auth = useAuthStore()
const notifications = useNotificationsStore()
const dashboard = useDashboardStore()
const { unreadCount } = storeToRefs(notifications)

const menuOpen = ref(false)
const menuRoot = ref(null)


function onDocumentClick(event) {
  if (menuOpen.value && menuRoot.value && !menuRoot.value.contains(event.target)) {
    menuOpen.value = false
  }
}

function onEscape(event) {
  if (event.key === 'Escape') menuOpen.value = false
}

// Dismissing on outside click/escape is what makes a dropdown feel native.
onMounted(() => {
  document.addEventListener('click', onDocumentClick)
  document.addEventListener('keydown', onEscape)
  notifications.refreshUnreadCount()
})

onUnmounted(() => {
  document.removeEventListener('click', onDocumentClick)
  document.removeEventListener('keydown', onEscape)
})

/**
 * Clears credentials, then leaves via a full page load rather than a router
 * navigation. A router navigation keeps the JavaScript heap alive, so every
 * Pinia store survives — and the next person to sign in on this browser would
 * be shown the previous user's cached figures. Reloading discards all in-memory
 * state by construction, and cannot be forgotten when a new store is added.
 * The 401 interceptor in api/client.js exits the same way.
 */
async function logout() {
  menuOpen.value = false
  notifications.reset()
  dashboard.reset()

  // Awaited: navigating cancels requests still in flight, and this one revokes
  // the refresh token. Skipping it would leave a working two-week credential
  // alive for a user who just asked to be signed out.
  await auth.logout()

  window.location.assign('/login')
}
</script>

<template>
  <header class="header">
    <button
      v-if="showToggle"
      class="header__burger"
      type="button"
      :aria-expanded="false"
      :aria-label="t('nav.openMenu')"
      :title="t('nav.openMenu')"
      @click="emit('toggle-drawer')"
    >
      <BaseIcon name="menu" :size="20" />
    </button>

    <RouterLink to="/" class="header__brand">
      <WealthMapIcon :size="26" />
      <WealthMapLogo class="header__wordmark" :width="112" />
    </RouterLink>

    <div class="header__spacer" />

    <RouterLink to="/notifications" class="header__icon-btn" :aria-label="t('nav.notifications')">
      <BaseIcon name="bell" :size="18" />
      <span v-if="unreadCount > 0" class="header__badge numeric" aria-hidden="true">
        {{ unreadCount > 99 ? '99+' : unreadCount }}
      </span>
      <span v-if="unreadCount > 0" class="sr-only">{{ t('notifications.unread', { count: unreadCount }) }}</span>
    </RouterLink>

    <div ref="menuRoot" class="header__user">
      <button
        class="header__avatar"
        type="button"
        :aria-expanded="menuOpen"
        aria-haspopup="menu"
        @click="menuOpen = !menuOpen"
      >
        <span class="header__initials">{{ auth.initials }}</span>
        <BaseIcon name="chevron-down" :size="14" class="header__caret" />
      </button>

      <Transition name="menu">
        <div v-if="menuOpen" class="menu" role="menu">
          <div class="menu__identity">
            <p class="menu__name">{{ auth.user?.fullName }}</p>
            <p class="menu__email">{{ auth.user?.email }}</p>
            <p class="menu__currency">{{ t('common.totalsShownIn', { currency: auth.currency }) }}</p>
          </div>

          <ThemeSelector />

          <LanguageSelector />

          <button class="menu__item" type="button" role="menuitem" @click="logout">
            <BaseIcon name="logout" :size="16" />
            {{ t('common.logout') }}
          </button>
        </div>
      </Transition>
    </div>
  </header>
</template>

<style scoped lang="scss" src="@/assets/styles/layout/AppHeader.scss"></style>
