<script setup>
import { ref, computed, onMounted } from 'vue'
import { motion } from 'motion-v'
import { fadeUp } from '@/composables/useMotionSafe'
import { storesApi } from '@/api/stores.api'
import { useAsync } from '@/composables/useAsync'

import PageHeader from '@/components/layout/PageHeader.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseIcon from '@/components/base/BaseIcon.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseEmptyState from '@/components/base/BaseEmptyState.vue'
import CardGridSkeleton from '@/features/shared/components/CardGridSkeleton.vue'

import StoreFormModal from '../components/StoreFormModal.vue'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const { data: stores, loading, error, run: loadStores } = useAsync(storesApi.list, { initialData: [] })

const formOpen = ref(false)
const editing = ref(null)
const search = ref('')

const filtered = computed(() => {
  const term = search.value.trim().toLowerCase()
  if (!term) return stores.value ?? []

  return (stores.value ?? []).filter((store) =>
    store.name.toLowerCase().includes(term) || store.category?.toLowerCase().includes(term)
  )
})

function openCreate() {
  editing.value = null
  formOpen.value = true
}

function openEdit(store) {
  editing.value = store
  formOpen.value = true
}

onMounted(loadStores)
</script>

<template>
  <div>
    <PageHeader
      :title="t('stores.title')"
      :subtitle="t('stores.subtitle')"
    >
      <template #actions>
        <BaseButton variant="primary" @click="openCreate">
          <template #icon><BaseIcon name="plus" :size="15" /></template>
          New store
        </BaseButton>
      </template>
    </PageHeader>

    <div class="search">
      <BaseInput v-model="search" :placeholder="t('stores.searchPlaceholder')">
        <template #prefix><BaseIcon name="search" :size="16" /></template>
      </BaseInput>
    </div>

    <CardGridSkeleton v-if="loading && !stores?.length" />

    <BaseEmptyState
      v-else-if="error"
      icon="alert"
      :title="t('stores.loadFailed')"
      :message="error.message"
    >
      <template #action><BaseButton variant="primary" @click="loadStores">{{ t('common.tryAgain') }}</BaseButton></template>
    </BaseEmptyState>

    <BaseEmptyState
      v-else-if="!stores?.length"
      icon="store"
      :title="t('stores.emptyTitle')"
      :message="t('stores.emptyMessage')"
    >
      <template #action><BaseButton variant="primary" @click="openCreate">{{ t('stores.addFirst') }}</BaseButton></template>
    </BaseEmptyState>

    <BaseEmptyState
      v-else-if="!filtered.length"
      icon="search"
      :title="t('stores.noMatches')"
      :message="`Nothing in the catalogue matches “${search}”.`"
      compact
    />

    <motion.div
      v-else
      class="grid"
      v-bind="fadeUp()"
    >
      <article v-for="store in filtered" :key="store.id" class="store">
        <div class="store__head">
          <div class="store__identity">
            <!-- Logos are user-supplied URLs, so a broken one must not break the card -->
            <img
              v-if="store.logoUrl"
              :src="store.logoUrl"
              :alt="''"
              class="store__logo"
              @error="(e) => (e.target.style.display = 'none')"
            />
            <span v-else class="store__logo store__logo--fallback">
              {{ store.name.charAt(0).toUpperCase() }}
            </span>

            <div class="store__text">
              <h3 class="store__name">{{ store.name }}</h3>
              <BaseBadge size="sm">{{ store.category }}</BaseBadge>
            </div>
          </div>

          <BaseButton
            v-if="store.isMine"
            size="sm"
            variant="ghost"
            :title="t('common.edit')"
            @click="openEdit(store)"
          >
            <template #icon><BaseIcon name="pencil" :size="14" /></template>
            <span class="sr-only">{{ t('common.edit') }}</span>
          </BaseButton>
        </div>

        <p v-if="store.description" class="store__description">{{ store.description }}</p>

        <span v-if="store.isMine" class="store__mine">{{ t('stores.addedByYou') }}</span>
      </article>
    </motion.div>

    <StoreFormModal v-model="formOpen" :store="editing" @saved="loadStores" />
  </div>
</template>

<style scoped lang="scss">
.search { max-width: 360px; margin-bottom: var(--sp-5); }

.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: var(--sp-4);
}

.store {
  display: flex;
  flex-direction: column;
  gap: var(--sp-3);

  padding: var(--sp-4);
  background: var(--surface);
  border: var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
}

.store__head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--sp-2);
}

.store__identity {
  display: flex;
  align-items: center;
  gap: var(--sp-3);
  min-width: 0;
}

.store__logo {
  width: 36px;
  height: 36px;
  flex: none;
  object-fit: contain;

  border: var(--border-subtle);
  border-radius: var(--radius-sm);
  background: var(--canvas-alt);
}

.store__logo--fallback {
  display: grid;
  place-items: center;
  font-size: var(--fs-md);
  font-weight: var(--fw-semibold);
  color: var(--text-muted);
}

.store__text { min-width: 0; display: flex; flex-direction: column; gap: var(--sp-1); align-items: flex-start; }

.store__name {
  font-size: var(--fs-base);
  font-weight: var(--fw-semibold);
  @include truncate;
}

.store__description { font-size: var(--fs-sm); color: var(--text-muted); line-height: 1.5; }

.store__mine {
  margin-top: auto;
  font-size: var(--fs-xs);
  color: var(--text-subtle);
}


@media (max-width: 640px) {
  .grid { grid-template-columns: 1fr; }
  .search { max-width: none; }
}
</style>
