import { api } from './api'
import { LoginRequest, CreateUserRequest, LoginResponse, UserInfo, AdminResponse } from '@/types/auth'

export const authService = {
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await api.post('/api/auth/login', credentials)
    return response.data
  },

  createUser: async (userData: CreateUserRequest) => {
    const response = await api.post('/api/auth/create-user', userData)
    return response.data
  },

  getUserInfo: async (): Promise<UserInfo> => {
    const response = await api.get('/api/usuario')
    return response.data
  },

  testAdminEndpoint: async (): Promise<AdminResponse> => {
    const response = await api.get('/api/admin')
    return response.data
  },

  initiateGoogleLogin: () => {
    const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5062'
    window.location.href = `${apiBaseUrl}/api/auth/external/google-login`
  }
}