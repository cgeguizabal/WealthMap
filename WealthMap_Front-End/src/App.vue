<script setup>
import { RouterView } from 'vue-router'
import BaseToast from '@/components/base/BaseToast.vue'
import BaseConfirmDialog from '@/components/base/BaseConfirmDialog.vue'
</script>

<template>
  <RouterView />

  <!-- Global singletons: any component raises these through the UI store -->
  <BaseToast />
  <BaseConfirmDialog />
</template>
