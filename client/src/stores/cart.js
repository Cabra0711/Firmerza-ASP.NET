import { defineStore } from 'pinia'
import api from '../api/client'

export const useCartStore = defineStore('cart', {
  state: () => ({
    items: [],
  }),
  getters: {
    total: (state) => state.items.reduce((sum, item) => sum + item.price * item.quantity, 0),
    itemCount: (state) => state.items.reduce((sum, item) => sum + item.quantity, 0),
  },
  actions: {
    addItem(product) {
      const existing = this.items.find((i) => i.productId === product.id)
      if (existing) {
        existing.quantity++
      } else {
        this.items.push({ productId: product.id, name: product.name, price: product.price, quantity: 1 })
      }
    },
    removeItem(productId) {
      this.items = this.items.filter((i) => i.productId !== productId)
    },
    clear() {
      this.items = []
    },
    async checkout() {
      const payload = {
        items: this.items.map((i) => ({ productId: i.productId, quantity: i.quantity })),
      }
      const { data } = await api.post('/api/ventas', payload)
      this.clear()
      return data
    },
  },
})
