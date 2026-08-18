import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { VitePWA } from 'vite-plugin-pwa'

export default defineConfig({
  plugins: [
    vue(),
    VitePWA({
      registerType: 'autoUpdate',
      includeAssets: ['favicon.svg', 'icon.svg', 'icon-maskable.svg'],

      manifest: {
        name: 'WealthMap',
        short_name: 'WealthMap',
        description: 'What you have, what you owe, and what is safe to spend.',
        theme_color: '#201F1D',
        background_color: '#F3F2EE',
        display: 'standalone',
        orientation: 'portrait-primary',
        start_url: '/',
        scope: '/',
        categories: ['finance', 'productivity'],
        icons: [
          { src: '/icon.svg', sizes: 'any', type: 'image/svg+xml', purpose: 'any' },
          { src: '/icon-maskable.svg', sizes: 'any', type: 'image/svg+xml', purpose: 'maskable' }
        ]
      },

      workbox: {
        globPatterns: ['**/*.{js,css,html,svg,woff2}'],

        // The shell is served from cache so the app opens offline; any deep link
        // falls back to index.html and the router takes it from there.
        navigateFallback: '/index.html',
        navigateFallbackDenylist: [/^\/api\//],

        runtimeCaching: [
          {
            // Financial figures are never served from cache. A stale balance is
            // worse than no balance, so offline requests fail and the UI says so.
            urlPattern: ({ url }) => url.pathname.startsWith('/api/'),
            handler: 'NetworkOnly'
          },
          {
            urlPattern: ({ url }) => url.origin === 'https://fonts.googleapis.com',
            handler: 'StaleWhileRevalidate',
            options: { cacheName: 'google-fonts-stylesheets' }
          },
          {
            urlPattern: ({ url }) => url.origin === 'https://fonts.gstatic.com',
            handler: 'CacheFirst',
            options: {
              cacheName: 'google-fonts-files',
              expiration: { maxEntries: 12, maxAgeSeconds: 60 * 60 * 24 * 365 },
              cacheableResponse: { statuses: [0, 200] }
            }
          }
        ]
      },

      devOptions: { enabled: false }
    })
  ],

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

    fs: {
      // The legal pages import docs/legal/*.md directly, which sits above the
      // Vite root. One copy of that text exists, and it is the one a lawyer will
      // eventually mark up — a duplicate inside src/ would drift the moment
      // either was edited. The build resolves these fine on its own; only the
      // dev server needs telling.
      allow: ['..']
    },

    proxy: {
      '/api': {
        target: 'http://localhost:5015',
        changeOrigin: true
      }
    }
  }
})
