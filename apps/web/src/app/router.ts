import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import AppLayout from './layouts/AppLayout.vue'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: AppLayout,
    children: [
      { path: '', name: 'home', component: () => import('@/pages/home').then((m) => m.HomePage) },
      { path: 'deck', name: 'deck', component: () => import('@/pages/deck').then((m) => m.DeckPage) },
      { path: 'train', name: 'train', component: () => import('@/pages/train').then((m) => m.TrainPage) },
      { path: 'add', name: 'add', component: () => import('@/pages/add-card').then((m) => m.AddCardPage) },
      {
        path: 'card/:id',
        name: 'card-detail',
        component: () => import('@/pages/card-detail').then((m) => m.CardDetailPage),
      },
      { path: 'stats', name: 'stats', component: () => import('@/pages/stats').then((m) => m.StatsPage) },
      {
        path: 'settings',
        name: 'settings',
        component: () => import('@/pages/settings').then((m) => m.SettingsPage),
      },
      { path: 'import', name: 'import', component: () => import('@/pages/import').then((m) => m.ImportPage) },
      { path: 'login', name: 'login', component: () => import('@/pages/login').then((m) => m.LoginPage) },
      {
        path: 'link-telegram',
        name: 'link-telegram',
        component: () => import('@/pages/link-telegram').then((m) => m.LinkTelegramPage),
      },
    ],
  },
]

export const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior() {
    return { top: 0 }
  },
})
