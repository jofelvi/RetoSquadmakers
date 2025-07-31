import { useMutation } from '@tanstack/react-query'
import { toast } from 'react-toastify'
import { authService } from '@/services/authService'
import { CreateUserRequest } from '@/types/auth'

export const useCreateUser = () => {
  return useMutation({
    mutationFn: (userData: CreateUserRequest) => authService.createUser(userData),
    onSuccess: (data) => {
      toast.success(`Usuario creado: ${data.usuario.nombre} (${data.usuario.rol})`)
    },
    onError: (error: any) => {
      const message = error.response?.data?.error || error.response?.data || 'Error al crear usuario'
      toast.error(message)
    }
  })
}