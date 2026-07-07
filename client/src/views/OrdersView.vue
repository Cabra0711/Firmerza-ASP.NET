<script setup>
import { ref, onMounted } from 'vue'
import api from '../api/client'

const sales = ref([])
const loading = ref(true)

onMounted(async () => {
  try {
    const { data } = await api.get('/api/ventas/mis-compras')
    sales.value = data
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="orders-page">
    <h1>Mi historial de compras</h1>
    <p v-if="loading">Cargando...</p>
    <p v-else-if="sales.length === 0">Todavía no tenés compras registradas.</p>
    <div v-else class="orders-list">
      <div v-for="sale in sales" :key="sale.id" class="order-card">
        <h3>{{ sale.saleNumber }} - {{ new Date(sale.saleDate).toLocaleDateString() }}</h3>
        <p>Estado: {{ sale.status }}</p>
        <ul>
          <li v-for="detail in sale.details" :key="detail.productId">
            {{ detail.productName }} x{{ detail.quantity }} - ${{ detail.subTotal.toFixed(2) }}
          </li>
        </ul>
        <p class="total">Total: ${{ sale.total.toFixed(2) }}</p>
      </div>
    </div>
  </div>
</template>
