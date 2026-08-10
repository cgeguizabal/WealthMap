import { defineStore } from 'pinia'
import { ref } from 'vue'
import { notificationsApi } from '@/api/notifications.api'

/**
 * Shared because the header badge and the notifications screen must agree: mark
 * one read on the list and the badge has to drop immediately.
 */
export const useNotificationsStore = defineStore('notifications', () => {
  const items = ref([])
  const unreadCount = ref(0)
  const loading = ref(false)
  const pagination = ref(null)

  /** Cheapest possible unread probe: one row, read the envelope's totalCount. */
  async function refreshUnreadCount() {
    try {
      const response = await notificationsApi.list({ unreadOnly: true, page: 1, pageSize: 1 })
      unreadCount.value = response.totalCount ?? 0
    } catch {
      // A failed badge refresh must never interrupt the page the user is on.
    }
  }

  async function load({ unreadOnly = false, page = 1, pageSize = 20 } = {}) {
    loading.value = true

    try {
      const response = await notificationsApi.list({ unreadOnly, page, pageSize })
      items.value = response.items ?? []
      pagination.value = response
      return response
    } catch {
      return null
    } finally {
      loading.value = false
    }
  }

  async function markRead(id) {
    try {
      const updated = await notificationsApi.markRead(id)

      const index = items.value.findIndex((item) => item.id === id)
      if (index !== -1) items.value[index] = updated

      unreadCount.value = Math.max(0, unreadCount.value - 1)
      return true
    } catch {
      return false
    }
  }

  async function sync() {
    try {
      const created = await notificationsApi.sync()
      if (created?.length) unreadCount.value += created.length
      return created ?? []
    } catch {
      return null
    }
  }

  function reset() {
    items.value = []
    unreadCount.value = 0
    pagination.value = null
  }

  return { items, unreadCount, loading, pagination, refreshUnreadCount, load, markRead, sync, reset }
})
