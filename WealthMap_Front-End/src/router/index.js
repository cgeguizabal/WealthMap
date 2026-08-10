import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'

const routes = [
  {
    path: '/login',
    name: 'login',
    component: () => import('@/features/auth/views/LoginView.vue'),
    meta: { public: true, layout: 'blank' }
  },
  {
    path: '/register',
    name: 'register',
    component: () => import('@/features/auth/views/RegisterView.vue'),
    meta: { public: true, layout: 'blank' }
  },
  {
    path: '/',
    name: 'dashboard',
    component: () => import('@/features/dashboard/views/DashboardView.vue')
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: () => import('@/features/shared/NotFoundView.vue'),
    meta: { public: true }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior: () => ({ top: 0 })
})

router.beforeEach((to) => {
  const auth = useAuthStore()

  // Not logged in, route needs auth → login, remembering where they wanted to go
  if (!to.meta.public && !auth.isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  // Already logged in, visiting login/register → send home
  if (to.meta.public && auth.isAuthenticated && to.name !== 'not-found') {
    return { name: 'dashboard' }
  }

  return true
})

export default router