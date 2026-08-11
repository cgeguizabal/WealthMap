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

<style scoped lang="scss">
.table-wrap { width: 100%; }

.table-state {
  display: grid;
  place-items: center;
  padding: var(--sp-10);
  color: var(--text-muted);
}

/* ── Table (>= 768px) ─────────────────────── */
.table {
  width: 100%;
  font-size: var(--fs-base);

  th {
    padding: var(--sp-3) var(--sp-4);
    text-align: left;
    font-size: var(--fs-xs);
    font-weight: var(--fw-semibold);
    text-transform: uppercase;
    letter-spacing: 0.04em;
    color: var(--text-muted);
    border-bottom: var(--border);
    white-space: nowrap;
  }

  td {
    padding: var(--sp-3) var(--sp-4);
    border-bottom: var(--border-subtle);
    vertical-align: middle;
  }

  tbody tr:last-child td { border-bottom: none; }
  tbody tr:nth-child(even) { background: var(--canvas-alt); }

  .is-right { text-align: right; }
  .is-left { text-align: left; }

  .is-clickable {
    cursor: pointer;
    &:hover { background: var(--canvas-alt); }
  }
}

.cards { display: none; }

@media (max-width: 767px) {
  .table { display: none; }

  .cards {
    display: flex;
    flex-direction: column;
    gap: var(--sp-3);
    padding: var(--sp-3);
  }

  .cards__item {
    display: flex;
    flex-direction: column;
    gap: var(--sp-2);

    padding: var(--sp-3);
    border: var(--border);
    border-radius: var(--radius);
    background: var(--surface);
    box-shadow: var(--shadow-sm);

    &.is-clickable { cursor: pointer; }
  }

  .cards__row {
    display: flex;
    align-items: baseline;
    justify-content: space-between;
    gap: var(--sp-3);
  }

  .cards__label {
    font-size: var(--fs-xs);
    text-transform: uppercase;
    letter-spacing: 0.04em;
    color: var(--text-muted);
    flex: none;
  }

  .cards__value {
    text-align: right;
    min-width: 0;
  }

  .is-hidden-mobile { display: none; }
}
</style>
