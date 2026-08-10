<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter, RouterLink } from 'vue-router'
import { storeToRefs } from 'pinia'
import { useAuthStore } from '@/stores/auth.store'
import { useNotificationsStore } from '@/stores/notifications.store'
import BaseIcon from '@/components/base/BaseIcon.vue'

defineProps({
  drawerOpen: { type: Boolean, default: false }
})

const emit = defineEmits(['toggle-drawer'])

const router = useRouter()
const auth = useAuthStore()
const notifications = useNotificationsStore()
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

function logout() {
  menuOpen.value = false
  notifications.reset()
  auth.logout()
  router.replace({ name: 'login' })
}
</script>

<template>
  <header class="header">
    <button
      class="header__burger"
      type="button"
      :aria-expanded="drawerOpen"
      aria-label="Toggle navigation"
      @click="emit('toggle-drawer')"
    >
      <BaseIcon :name="drawerOpen ? 'x' : 'menu'" :size="20" />
    </button>

    <RouterLink to="/" class="header__brand">
      <span class="header__mark">WM</span>
      <span class="header__wordmark">WealthMap</span>
    </RouterLink>

    <div class="header__spacer" />

    <RouterLink to="/notifications" class="header__icon-btn" aria-label="Notifications">
      <BaseIcon name="bell" :size="18" />
      <span v-if="unreadCount > 0" class="header__badge numeric" aria-hidden="true">
        {{ unreadCount > 99 ? '99+' : unreadCount }}
      </span>
      <span v-if="unreadCount > 0" class="sr-only">{{ unreadCount }} unread</span>
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
            <p class="menu__currency">Totals shown in {{ auth.currency }}</p>
          </div>

          <button class="menu__item" type="button" role="menuitem" @click="logout">
            <BaseIcon name="logout" :size="16" />
            Sign out
          </button>
        </div>
      </Transition>
    </div>
  </header>
</template>

<style scoped lang="scss">
.header {
  display: flex;
  align-items: center;
  gap: var(--sp-2);
  flex: none;

  height: 56px;
  padding: 0 var(--sp-5);
  background: var(--surface);
  border-bottom: var(--border);
}

.header__spacer { flex: 1; }

.header__burger {
  display: none;
  place-items: center;
  width: 34px;
  height: 34px;

  border: var(--border);
  border-radius: var(--radius-sm);
  background: var(--surface);
  cursor: pointer;

  @include focus-ring;
  &:hover { background: var(--canvas-alt); }
}

/* The sidebar carries the brand at desktop widths */
.header__brand { display: none; align-items: center; gap: var(--sp-2); text-decoration: none; color: inherit; }

.header__mark {
  display: grid;
  place-items: center;
  width: 26px;
  height: 26px;
  background: var(--ink);
  color: var(--canvas);
  border-radius: var(--radius-sm);
  font-size: var(--fs-xs);
  font-weight: var(--fw-bold);
}

.header__wordmark { font-size: var(--fs-base); font-weight: var(--fw-semibold); }

.header__icon-btn {
  position: relative;
  display: grid;
  place-items: center;
  width: 34px;
  height: 34px;

  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  color: var(--text-muted);

  @include focus-ring;

  &:hover { background: var(--canvas-alt); color: var(--ink); }
}

.header__badge {
  position: absolute;
  top: -3px;
  right: -3px;

  min-width: 17px;
  height: 17px;
  padding: 0 4px;

  display: grid;
  place-items: center;
  border-radius: 9px;

  background: var(--negative);
  color: #fff;
  font-size: 10px;
  font-weight: var(--fw-semibold);
  line-height: 1;
}

.header__user { position: relative; }

.header__avatar {
  display: flex;
  align-items: center;
  gap: var(--sp-1);

  padding: 3px var(--sp-2) 3px 3px;
  border: var(--border);
  border-radius: 999px;
  background: var(--surface);
  cursor: pointer;

  @include focus-ring;
  &:hover { background: var(--canvas-alt); }
}

.header__initials {
  display: grid;
  place-items: center;
  width: 26px;
  height: 26px;

  background: var(--accent);
  color: #fff;
  border-radius: 50%;
  font-size: var(--fs-xs);
  font-weight: var(--fw-semibold);
}

.header__caret { color: var(--text-muted); }

/* ── Dropdown ─────────────────────────────── */
.menu {
  position: absolute;
  top: calc(100% + var(--sp-2));
  right: 0;
  z-index: 50;
  min-width: 232px;

  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
  overflow: hidden;
}

.menu__identity {
  padding: var(--sp-3) var(--sp-4);
  border-bottom: var(--border-subtle);
}

.menu__name { font-size: var(--fs-sm); font-weight: var(--fw-semibold); }
.menu__email { font-size: var(--fs-xs); color: var(--text-muted); @include truncate; }
.menu__currency { margin-top: var(--sp-1); font-size: var(--fs-xs); color: var(--text-subtle); }

.menu__item {
  display: flex;
  align-items: center;
  gap: var(--sp-2);
  width: 100%;

  padding: var(--sp-3) var(--sp-4);
  border: none;
  background: transparent;
  color: var(--text);
  font-size: var(--fs-sm);
  text-align: left;
  cursor: pointer;

  &:hover { background: var(--canvas-alt); }
}

.menu-enter-active, .menu-leave-active { transition: opacity var(--dur-fast) var(--ease), transform var(--dur-fast) var(--ease); }
.menu-enter-from, .menu-leave-to { opacity: 0; transform: translateY(-4px); }

.sr-only {
  position: absolute;
  width: 1px; height: 1px;
  overflow: hidden;
  clip: rect(0 0 0 0);
}

@media (max-width: 1023px) {
  .header { padding: 0 var(--sp-4); }
  .header__burger { display: grid; }
  .header__brand { display: flex; }
}

@media (prefers-reduced-motion: reduce) {
  .menu-enter-active, .menu-leave-active { transition: none; }
}
</style>
