import { useMutation } from '@tanstack/react-query'
import { toast } from 'react-toastify'
import { authService } from '@/services/authService'

export const useAdminTest = () => {
  return useMutation({
    mutationFn: authService.testAdminEndpoint,
    onSuccess: (data) => {
      toast.success(`✅ Acceso admin autorizado: ${data.mensaje}`)
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || 'No tienes permisos de administrador'
      toast.error(`❌ Acceso denegado: ${message}`)
    }
  })
}