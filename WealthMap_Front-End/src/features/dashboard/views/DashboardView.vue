<script setup>
// Placeholder — the real dashboard is built in step 4. It exists now because the
// router imports it, and every commit must build.
import { useAuthStore } from '@/stores/auth.store'

const auth = useAuthStore()
</script>

<template>
  <div class="placeholder">
    <h1>Signed in as {{ auth.user?.fullName }}</h1>
    <p>The dashboard is built in a later step.</p>
  </div>
</template>

<style scoped lang="scss">
.placeholder {
  display: flex;
  flex-direction: column;
  gap: var(--sp-2);
  padding: var(--sp-10);
}
</style>
