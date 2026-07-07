<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const form = ref({ userName: '', email: '', password: '', document: '' })
const error = ref('')
const success = ref(false)
const loading = ref(false)
const auth = useAuthStore()
const router = useRouter()

async function submit() {
  error.value = ''
  loading.value = true
  try {
    await auth.register(form.value)
    success.value = true
    setTimeout(() => router.push('/login'), 1200)
  } catch (err) {
    error.value = err.response?.data?.errors?.join(', ') || err.response?.data?.message || 'No se pudo registrar.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="auth-page">
    <h1>Crear cuenta</h1>
    <form @submit.prevent="submit">
      <label>
        Usuario
        <input v-model="form.userName" required />
      </label>
      <label>
        Correo
        <input v-model="form.email" type="email" required />
      </label>
      <label>
        Documento
        <input v-model="form.document" required />
      </label>
      <label>
        Contraseña
        <input v-model="form.password" type="password" required />
      </label>
      <p v-if="error" class="error">{{ error }}</p>
      <p v-if="success" class="success">¡Cuenta creada! Redirigiendo a login...</p>
      <button type="submit" :disabled="loading">Registrarme</button>
    </form>
    <p>¿Ya tenés cuenta? <router-link to="/login">Iniciá sesión</router-link></p>
  </div>
</template>
