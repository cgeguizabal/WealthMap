import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import { reportMissingKeys } from './i18n'
import { applyTheme, readStoredTheme } from './config/theme'
import './assets/styles/main.scss'

/**
 * Before anything renders, and before Pinia exists.
 *
 * The theme store applies the same value when it is created, but a store is only
 * created when a component first uses it — which is after mount. Waiting for
 * that means one frame of light theme on every load for a dark-mode user, and a
 * white flash is the most conspicuous way to get this wrong.
 */
applyTheme(readStoredTheme())

const app = createApp(App)

/**
 * Last line of defence. BaseErrorBoundary renders the fallback UI; this makes
 * sure nothing is swallowed silently, and is where a reporting service would go.
 */
app.config.errorHandler = (error, _instance, info) => {
  console.error(`[WealthMap] ${info}`, error)
}

// Pinia before the router: the navigation guard calls useAuthStore.
app.use(createPinia())
app.use(router)

// Dev-only: warns if the two locale files have drifted apart.
reportMissingKeys()

app.mount('#app')
