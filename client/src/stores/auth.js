import { defineStore } from 'pinia'
import api from '../api/client'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('firmeza_token') || null,
    username: localStorage.getItem('firmeza_username') || null,
    role: localStorage.getItem('firmeza_role') || null,
  }),
  getters: {
    isAuthenticated: (state) => !!state.token,
  },
  actions: {
    async login(username, password) {
      const { data } = await api.post('/api/auth/login', { username, password })
      this.token = data.token
      this.username = data.username
      this.role = data.role
      localStorage.setItem('firmeza_token', data.token)
      localStorage.setItem('firmeza_username', data.username)
      localStorage.setItem('firmeza_role', data.role)
    },
    async register(payload) {
      await api.post('/api/auth/register', payload)
    },
    logout() {
      this.token = null
      this.username = null
      this.role = null
      localStorage.removeItem('firmeza_token')
      localStorage.removeItem('firmeza_username')
      localStorage.removeItem('firmeza_role')
    },
  },
})
