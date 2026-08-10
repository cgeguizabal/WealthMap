<script setup>
import { RouterLink } from 'vue-router'
</script>

<template>
  <div class="notfound">
    <h1>404</h1>
    <p>That page doesn't exist.</p>
    <RouterLink to="/">Back to dashboard</RouterLink>
  </div>
</template>

<style scoped>
.notfound {
  min-height: 100vh;
  display: grid;
  place-content: center;
  text-align: center;
  gap: var(--sp-4);
}
</style>