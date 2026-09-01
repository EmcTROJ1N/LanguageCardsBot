import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { useAuthStore } from '@/features/auth'
import App from './App.vue'
import { router } from './router'
import './styles/global.css'

const app = createApp(App)
app.use(createPinia())

const authStore = useAuthStore()
await authStore.init()

app.use(router)
app.mount('#app')
