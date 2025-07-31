'use client'

import { useEffect } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { useAuthStore } from '@/store/authStore'
import { toast } from 'react-toastify'

export default function CallbackPage() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const setUser = useAuthStore((state) => state.setUser)

  useEffect(() => {
    const token = searchParams.get('token')
    const email = searchParams.get('email')
    const nombre = searchParams.get('nombre')
    const rol = searchParams.get('rol')

    if (token && email && nombre && rol) {
      const user = { token, email, nombre, rol }
      localStorage.setItem('token', token)
      setUser(user)
      toast.success(`¡Bienvenido ${nombre}! Autenticación OAuth exitosa`)
      router.replace('/')
    } else {
      toast.error('Error en la autenticación OAuth')
      router.replace('/')
    }
  }, [searchParams, setUser, router])

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center">
      <div className="bg-white rounded-lg shadow-lg p-8 text-center">
        <h1 className="text-2xl font-bold text-gray-800 mb-4">
          Procesando autenticación...
        </h1>
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
        <p className="text-gray-600 mt-4">
          Redirigiendo...
        </p>
      </div>
    </div>
  )
}