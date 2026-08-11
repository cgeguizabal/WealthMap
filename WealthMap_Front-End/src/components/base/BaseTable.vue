<script setup>
import BaseSpinner from './BaseSpinner.vue'
import BaseEmptyState from './BaseEmptyState.vue'

const props = defineProps({
  /** `[{ key, label, align?: 'left'|'right', width?, hideOnMobile? }]` */
  columns: { type: Array, required: true },
  rows: { type: Array, default: () => [] },
  rowKey: { type: String, default: 'id' },
  loading: { type: Boolean, default: false },
  /**
   * `true` makes every row clickable. A predicate `(row) => boolean` makes only
   * some clickable — for tables that mix rows which have a destination with
   * rows that do not.
   */
  clickable: { type: [Boolean, Function], default: false },
  emptyTitle: { type: String, default: 'Nothing here yet' },
  emptyMessage: { type: String, default: '' }
})

defineEmits(['row-click'])

function valueOf(row, key) {
  return key.split('.').reduce((acc, part) => acc?.[part], row)
}

function isRowClickable(row) {
  return typeof props.clickable === 'function' ? props.clickable(row) : props.clickable
}
</script>

<template>
  <div class="table-wrap">
    <div v-if="loading" class="table-state">
      <BaseSpinner :size="20" />
    </div>

    <BaseEmptyState
      v-else-if="rows.length === 0"
      :title="emptyTitle"
      :message="emptyMessage"
      compact
    >
      <template v-if="$slots['empty-action']" #action><slot name="empty-action" /></template>
    </BaseEmptyState>

    <template v-else>
      <!-- Desktop: a real table, because tabular data belongs in one -->
      <table class="table">
        <thead>
          <tr>
            <th
              v-for="column in columns"
              :key="column.key"
              :class="[`is-${column.align ?? 'left'}`, { 'is-hidden-mobile': column.hideOnMobile }]"
              :style="column.width ? { width: column.width } : undefined"
            >
              {{ column.label }}
            </th>
          </tr>
        </thead>

        <tbody>
          <tr
            v-for="row in rows"
            :key="row[rowKey]"
            :class="{ 'is-clickable': isRowClickable(row) }"
            @click="isRowClickable(row) && $emit('row-click', row)"
          >
            <td
              v-for="column in columns"
              :key="column.key"
              :class="[`is-${column.align ?? 'left'}`, { 'is-hidden-mobile': column.hideOnMobile }]"
            >
              <slot :name="`cell-${column.key}`" :row="row" :value="valueOf(row, column.key)">
                {{ valueOf(row, column.key) }}
              </slot>
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Mobile: the same rows as stacked cards; a 6-column table is unusable at 360px -->
      <ul class="cards">
        <li
          v-for="row in rows"
          :key="row[rowKey]"
          class="cards__item"
          :class="{ 'is-clickable': isRowClickable(row) }"
          @click="isRowClickable(row) && $emit('row-click', row)"
        >
          <div v-for="column in columns" :key="column.key" class="cards__row">
            <span class="cards__label">{{ column.label }}</span>
            <span class="cards__value">
              <slot :name="`cell-${column.key}`" :row="row" :value="valueOf(row, column.key)">
                {{ valueOf(row, column.key) }}
              </slot>
            </span>
          </div>
        </li>
      </ul>
    </template>
  </div>
</template>

<style scoped lang="scss" src="./BaseTable.scss"></style>
