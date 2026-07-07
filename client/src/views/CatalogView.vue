<script setup>
import { ref, computed, onMounted } from 'vue'
import api from '../api/client'
import { useCartStore } from '../stores/cart'

const products = ref([])
const search = ref('')
const category = ref('')
const loading = ref(true)
const cart = useCartStore()

const categories = computed(() => [...new Set(products.value.map((p) => p.category))])

const filtered = computed(() =>
  products.value.filter((p) => {
    const matchesSearch = p.name.toLowerCase().includes(search.value.toLowerCase())
    const matchesCategory = !category.value || p.category === category.value
    return matchesSearch && matchesCategory
  }),
)

onMounted(async () => {
  try {
    const { data } = await api.get('/api/productos')
    products.value = data
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="catalog-page">
    <h1>Catálogo Firmeza</h1>
    <div class="filters">
      <input v-model="search" placeholder="Buscar productos..." />
      <select v-model="category">
        <option value="">Todas las categorías</option>
        <option v-for="c in categories" :key="c" :value="c">{{ c }}</option>
      </select>
    </div>
    <p v-if="loading">Cargando productos...</p>
    <div v-else class="product-grid">
      <div v-for="product in filtered" :key="product.id" class="product-card">
        <h3>{{ product.name }}</h3>
        <p>{{ product.description }}</p>
        <p class="price">${{ product.price.toFixed(2) }}</p>
        <p class="stock">Stock: {{ product.quantity }} ({{ product.status }})</p>
        <button :disabled="product.quantity === 0" @click="cart.addItem(product)">Agregar al carrito</button>
      </div>
      <p v-if="filtered.length === 0">No hay productos que coincidan con la búsqueda.</p>
    </div>
  </div>
</template>
