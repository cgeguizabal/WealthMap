import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { VitePWA } from 'vite-plugin-pwa'

export default defineConfig({
  plugins: [
    vue(),
    VitePWA({
      registerType: 'autoUpdate',
      includeAssets: [
        'favicon-32.png',
        'favicon-64.png',
        'apple-touch-icon.png'
      ],

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
        // The light mark on an opaque white square. Both properties matter:
        // a non-square icon gets letterboxed by the launcher, and iOS renders
        // transparency in a home-screen icon as solid black — which would put
        // navy lettering on black.
        //
        // The maskable one carries far more padding because Android crops to a
        // circle of 80% diameter and keeps only what survives.
        icons: [
          { src: '/icon-192.png', sizes: '192x192', type: 'image/png', purpose: 'any' },
          { src: '/icon-512.png', sizes: '512x512', type: 'image/png', purpose: 'any' },
          { src: '/icon-maskable-512.png', sizes: '512x512', type: 'image/png', purpose: 'maskable' }
        ]
      },

      workbox: {
        // png is here for the app icons: an installed PWA that cannot draw its
        // own icon offline is a poor look, and they are a few KB each.
        globPatterns: ['**/*.{js,css,html,svg,png,woff2}'],

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
