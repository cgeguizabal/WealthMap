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
    path: '/accounts',
    name: 'accounts',
    component: () => import('@/features/accounts/views/AccountsView.vue')
  },
  {
    path: '/accounts/:id',
    name: 'account-detail',
    component: () => import('@/features/accounts/views/AccountDetailView.vue')
  },
  {
    path: '/credit-cards',
    name: 'credit-cards',
    component: () => import('@/features/creditCards/views/CreditCardsView.vue')
  },
  {
    path: '/credit-cards/:id',
    name: 'credit-card-detail',
    component: () => import('@/features/creditCards/views/CreditCardDetailView.vue')
  },
  {
    path: '/payments',
    name: 'payments',
    component: () => import('@/features/payments/views/PaymentsView.vue')
  },
  {
    path: '/purchases',
    name: 'purchases',
    component: () => import('@/features/purchases/views/PurchasesView.vue')
  },
  {
    path: '/stores',
    name: 'stores',
    component: () => import('@/features/stores/views/StoresView.vue')
  },
  {
    path: '/installments',
    name: 'installments',
    component: () => import('@/features/installments/views/InstallmentsView.vue')
  },
  {
    path: '/installments/:id',
    name: 'installment-detail',
    component: () => import('@/features/installments/views/InstallmentDetailView.vue')
  },
  {
    path: '/debts',
    name: 'debts',
    component: () => import('@/features/debts/views/DebtsView.vue')
  },
  {
    path: '/debts/:id',
    name: 'debt-detail',
    component: () => import('@/features/debts/views/DebtDetailView.vue')
  },
  {
    path: '/goals',
    name: 'goals',
    component: () => import('@/features/goals/views/GoalsView.vue')
  },
  {
    path: '/job',
    name: 'job',
    component: () => import('@/features/job/views/JobView.vue')
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