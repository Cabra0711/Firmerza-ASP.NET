<script setup>
import { useAuthStore } from './stores/auth'
import { useCartStore } from './stores/cart'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const cart = useCartStore()
const router = useRouter()

function logout() {
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <header class="topbar">
    <router-link to="/" class="brand">Firmeza</router-link>
    <nav>
      <router-link to="/">Catálogo</router-link>
      <router-link to="/cart">Carrito ({{ cart.itemCount }})</router-link>
      <router-link v-if="auth.isAuthenticated" to="/orders">Mis compras</router-link>
      <span v-if="auth.isAuthenticated">
        {{ auth.username }}
        <button @click="logout">Salir</button>
      </span>
      <router-link v-else to="/login">Iniciar sesión</router-link>
    </nav>
  </header>
  <main>
    <router-view />
  </main>
</template>

<style>
body {
  margin: 0;
  font-family: system-ui, sans-serif;
  background: #0f172a;
  color: #e2e8f0;
}
.topbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 2rem;
  background: #1e293b;
}
.topbar .brand {
  font-weight: bold;
  font-size: 1.25rem;
  color: #818cf8;
  text-decoration: none;
}
.topbar nav {
  display: flex;
  gap: 1.5rem;
  align-items: center;
}
.topbar nav a {
  color: #e2e8f0;
  text-decoration: none;
}
main {
  padding: 2rem;
  max-width: 1100px;
  margin: 0 auto;
}
.error {
  color: #f87171;
}
.success {
  color: #34d399;
}
.product-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 1rem;
}
.product-card, .order-card {
  background: #1e293b;
  padding: 1rem;
  border-radius: 0.75rem;
}
.filters {
  display: flex;
  gap: 1rem;
  margin-bottom: 1.5rem;
}
form {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  max-width: 320px;
}
table {
  width: 100%;
  border-collapse: collapse;
}
td, th {
  padding: 0.5rem;
  text-align: left;
  border-bottom: 1px solid #334155;
}
</style>
