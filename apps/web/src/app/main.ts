import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { useAuthStore } from '@/features/auth'
import App from './App.vue'
import { router } from './router'
import './styles/global.css'

const app = createApp(App)
app.use(createPinia())
app.use(router)

const authStore = useAuthStore()
await authStore.init()

app.mount('#app')
