<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useCartStore } from '../stores/cart'

const cart = useCartStore()
const error = ref('')
const success = ref(null)
const loading = ref(false)
const router = useRouter()

async function checkout() {
  error.value = ''
  loading.value = true
  try {
    success.value = await cart.checkout()
  } catch (err) {
    error.value = err.response?.data?.message || 'No se pudo completar la compra.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="cart-page">
    <h1>Carrito</h1>
    <p v-if="success" class="success">
      ¡Compra registrada! N° {{ success.saleNumber }} - Total: ${{ success.total.toFixed(2) }}.
      Revisá tu <router-link to="/orders">historial de compras</router-link>.
    </p>
    <template v-else>
      <p v-if="cart.items.length === 0">Tu carrito está vacío.</p>
      <table v-else>
        <thead>
          <tr>
            <th>Producto</th>
            <th>Cantidad</th>
            <th>Precio</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in cart.items" :key="item.productId">
            <td>{{ item.name }}</td>
            <td>{{ item.quantity }}</td>
            <td>${{ (item.price * item.quantity).toFixed(2) }}</td>
            <td><button @click="cart.removeItem(item.productId)">Quitar</button></td>
          </tr>
        </tbody>
      </table>
      <p v-if="cart.items.length > 0" class="total">Total: ${{ cart.total.toFixed(2) }}</p>
      <p v-if="error" class="error">{{ error }}</p>
      <button v-if="cart.items.length > 0" :disabled="loading" @click="checkout">Confirmar compra</button>
    </template>
  </div>
</template>
