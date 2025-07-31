import { useQuery } from '@tanstack/react-query'
import { toast } from 'react-toastify'
import { authService } from '@/services/authService'
import { useAuthStore } from '@/store/authStore'

export const useUserInfo = () => {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated)

  return useQuery({
    queryKey: ['userInfo'],
    queryFn: authService.getUserInfo,
    enabled: isAuthenticated,
    onError: (error: any) => {
      const message = error.response?.data?.message || 'Error al obtener información del usuario'
      toast.error(message)
    }
  })
}