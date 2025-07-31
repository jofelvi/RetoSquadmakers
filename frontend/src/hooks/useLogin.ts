import { useMutation } from '@tanstack/react-query'
import { toast } from 'react-toastify'
import { authService } from '@/services/authService'
import { useAuthStore } from '@/store/authStore'
import { LoginRequest } from '@/types/auth'

export const useLogin = () => {
  const setUser = useAuthStore((state) => state.setUser)

  return useMutation({
    mutationFn: (credentials: LoginRequest) => authService.login(credentials),
    onSuccess: (data) => {
      localStorage.setItem('token', data.token)
      setUser(data)
      toast.success(`¡Bienvenido ${data.nombre}!`)
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || error.response?.data || 'Error de autenticación'
      toast.error(message)
    }
  })
}