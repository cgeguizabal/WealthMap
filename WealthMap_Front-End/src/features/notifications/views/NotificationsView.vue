<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { motion } from 'motion-v'
import { useNotificationsStore } from '@/stores/notifications.store'
import { useToast } from '@/composables/useToast'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseTabs from '@/components/base/BaseTabs.vue'
import BaseSpinner from '@/components/base/BaseSpinner.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'
import BasePagination from '@/components/base/BasePagination.vue'

const notifications = useNotificationsStore()
const { items, unreadCount, loading, pagination } = storeToRefs(notifications)
const toast = useToast()

const tab = ref('all')
const page = ref(1)
const syncing = ref(false)

const ICON_BY_SEVERITY = { Critical: 'alert', Warning: 'alert', Info: 'info' }
const VARIANT_BY_SEVERITY = { Critical: 'negative', Warning: 'warning', Info: 'neutral' }

const tabs = computed(() => [
  { value: 'all', label: 'All' },
  { value: 'unread', label: 'Unread', count: unreadCount.value }
])

function load() {
  return notifications.load({ unreadOnly: tab.value === 'unread', page: page.value })
}

/** Switching filter makes the current page number meaningless. */
watch(tab, () => {
  page.value = 1
  load()
})

watch(page, load)

async function markRead(notification) {
  if (notification.isRead) return

  const ok = await notifications.markRead(notification.id)
  if (!ok) {
    toast.error('Could not mark that as read.')
    return
  }

  // An unread-filtered list should drop the row it no longer matches.
  if (tab.value === 'unread') load()
}

async function sync() {
  syncing.value = true

  try {
    const created = await notifications.sync()

    if (created === null) {
      toast.error('Could not check for new alerts.')
    } else if (created.length === 0) {
      toast.info('Nothing new — everything current is already here.')
    } else {
      toast.success(`${created.length} new notification${created.length === 1 ? '' : 's'}.`)
    }

    load()
  } finally {
    syncing.value = false
  }
}

function relativeTime(iso) {
  const then = new Date(iso)
  const minutes = Math.round((Date.now() - then.getTime()) / 60000)

  if (minutes < 1) return 'just now'
  if (minutes < 60) return `${minutes}m ago`
  if (minutes < 1440) return `${Math.round(minutes / 60)}h ago`
  if (minutes < 10080) return `${Math.round(minutes / 1440)}d ago`

  return then.toLocaleDateString(undefined, { month: 'short', day: 'numeric' })
}

onMounted(() => {
  load()
  notifications.refreshUnreadCount()
})
</script>

<template>
  <div>
    <PageHeader
      title="Notifications"
      subtitle="Alerts you have been shown. Checking again re-raises anything still true."
    >
      <template #actions>
        <BaseButton variant="primary" :loading="syncing" @click="sync">
          <template #icon><BaseIcon name="refresh" :size="15" /></template>
          Check now
        </BaseButton>
      </template>
    </PageHeader>

    <BaseTabs v-model="tab" :tabs="tabs" class="tabs" />

    <BaseCard :padded="false">
      <div v-if="loading && !items.length" class="state"><BaseSpinner :size="20" /></div>

      <BaseEmptyState
        v-else-if="!items.length"
        :icon="tab === 'unread' ? 'check-circle' : 'bell'"
        :title="tab === 'unread' ? 'Nothing unread' : 'No notifications yet'"
        :message="tab === 'unread'
          ? 'You are up to date.'
          : 'Use Check now to turn your current alerts into notifications you can work through.'"
        compact
      >
        <template v-if="tab === 'all'" #action>
          <BaseButton variant="secondary" :loading="syncing" @click="sync">Check now</BaseButton>
        </template>
      </BaseEmptyState>

      <ul v-else class="list">
        <motion.li
          v-for="(item, index) in items"
          :key="item.id"
          :class="['note', `note--${item.severity.toLowerCase()}`, { 'note--read': item.isRead }]"
          :initial="{ opacity: 0, y: 4 }"
          :animate="{ opacity: 1, y: 0 }"
          :transition="{ duration: 0.2, delay: Math.min(index * 0.03, 0.2), ease: [0.2, 0, 0, 1] }"
        >
          <BaseIcon
            :name="ICON_BY_SEVERITY[item.severity] ?? 'info'"
            :size="17"
            class="note__icon"
          />

          <div class="note__body">
            <div class="note__head">
              <span class="note__title">{{ item.title }}</span>
              <BaseBadge :variant="VARIANT_BY_SEVERITY[item.severity]" size="sm">
                {{ item.severity }}
              </BaseBadge>
            </div>

            <p class="note__message">{{ item.message }}</p>

            <span class="note__time">
              {{ relativeTime(item.createdAt) }}
              <template v-if="item.isRead"> · read</template>
            </span>
          </div>

          <BaseButton
            v-if="!item.isRead"
            size="sm"
            variant="ghost"
            title="Mark as read"
            @click="markRead(item)"
          >
            <template #icon><BaseIcon name="check" :size="14" /></template>
            <span class="note__action-label">Read</span>
          </BaseButton>
        </motion.li>
      </ul>

      <BasePagination
        v-if="pagination"
        :page="pagination.page"
        :page-size="pagination.pageSize"
        :total-count="pagination.totalCount"
        :total-pages="pagination.totalPages"
        :has-next-page="pagination.hasNextPage"
        :has-previous-page="pagination.hasPreviousPage"
        @update:page="page = $event"
      />
    </BaseCard>

    <p class="footnote">
      Marking something read is an acknowledgement, not a mute — if the condition still holds the
      next check raises it again.
    </p>
  </div>
</template>

<style scoped lang="scss">
.tabs { margin-bottom: var(--sp-4); }

.list { display: flex; flex-direction: column; }

.note {
  display: flex;
  align-items: flex-start;
  gap: var(--sp-3);

  padding: var(--sp-4) var(--sp-5);
  border-bottom: var(--border-subtle);

  &:last-child { border-bottom: none; }
}

.note--critical { border-left: 3px solid var(--negative); }
.note--warning { border-left: 3px solid var(--warning); }
.note--info { border-left: 3px solid var(--line); }

.note--critical .note__icon { color: var(--negative); }
.note--warning .note__icon { color: var(--warning); }
.note--info .note__icon { color: var(--text-muted); }

/* Read items stay legible but visibly recede */
.note--read {
  background: var(--canvas-alt);

  .note__title, .note__message { color: var(--text-muted); }
}

.note__icon { margin-top: 2px; flex: none; }
.note__body { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: var(--sp-1); }

.note__head {
  display: flex;
  align-items: center;
  gap: var(--sp-2);
  flex-wrap: wrap;
}

.note__title { font-size: var(--fs-base); font-weight: var(--fw-semibold); }
.note__message { font-size: var(--fs-sm); color: var(--text-muted); line-height: 1.5; }
.note__time { font-size: var(--fs-xs); color: var(--text-subtle); }

.state { display: grid; place-items: center; padding: var(--sp-10); color: var(--text-muted); }

.footnote {
  margin-top: var(--sp-4);
  font-size: var(--fs-xs);
  color: var(--text-subtle);
  text-align: center;
}

@media (max-width: 767px) {
  .note { padding: var(--sp-3) var(--sp-4); }
  .note__action-label { display: none; }
}
</style>
