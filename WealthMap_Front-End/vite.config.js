import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  css: {
    preprocessorOptions: {
      scss: {
        // Mixins are injected into every <style lang="scss"> block, so components
        // never repeat an @use line. Tokens are plain CSS custom properties and
        // are already global via main.scss.
        additionalData: '@use "mixins" as *;\n',
        loadPaths: [fileURLToPath(new URL('./src/assets/styles', import.meta.url))]
      }
    }
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5015',
        changeOrigin: true
      }
    }
  }
})