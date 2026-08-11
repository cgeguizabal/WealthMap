import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import { reportMissingKeys } from './i18n'
import './assets/styles/main.scss'

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
