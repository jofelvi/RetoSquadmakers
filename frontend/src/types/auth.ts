export interface LoginRequest {
  email: string
  password: string
}

export interface CreateUserRequest {
  email: string
  nombre: string
  password: string
  rol: 'User' | 'Admin'
}

export interface LoginResponse {
  token: string
  email: string
  nombre: string
  rol: string
}

export interface UserInfo {
  id: number
  nombre: string
  email: string
  rol: string
  fechaCreacion: string
}

export interface User {
  token: string
  email: string
  nombre: string
  rol: string
}

export interface AdminResponse {
  mensaje: string
  fecha: string
}