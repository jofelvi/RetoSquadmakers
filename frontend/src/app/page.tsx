'use client'

import { useState } from 'react'
import { useAuthStore } from '@/store/authStore'
import { useLogin } from '@/hooks/useLogin'
import { useCreateUser } from '@/hooks/useCreateUser'
import { useUserInfo } from '@/hooks/useUserInfo'
import { useAdminTest } from '@/hooks/useAdminTest'
import { authService } from '@/services/authService'
import { LoginRequest, CreateUserRequest } from '@/types/auth'

export default function Home() {
  const { user, isAuthenticated, logout } = useAuthStore()
  const [loginForm, setLoginForm] = useState<LoginRequest>({ 
    email: 'admin@test.com', 
    password: 'admin123' 
  })
  const [createForm, setCreateForm] = useState<CreateUserRequest>({ 
    email: '', 
    nombre: '', 
    password: '', 
    rol: 'User' 
  })

  const loginMutation = useLogin()
  const createUserMutation = useCreateUser()
  const { data: userInfo, refetch: refetchUserInfo, isLoading: isLoadingUserInfo } = useUserInfo()
  const adminTestMutation = useAdminTest()

  const handleLogin = (e: React.FormEvent) => {
    e.preventDefault()
    loginMutation.mutate(loginForm)
  }

  const handleCreateUser = (e: React.FormEvent) => {
    e.preventDefault()
    createUserMutation.mutate(createForm, {
      onSuccess: () => {
        setCreateForm({ email: '', nombre: '', password: '', rol: 'User' })
      }
    })
  }

  const handleGoogleLogin = () => {
    authService.initiateGoogleLogin()
  }

  const handleGetUserInfo = () => {
    refetchUserInfo()
  }

  const handleTestAdmin = () => {
    adminTestMutation.mutate()
  }

  const handleLogout = () => {
    logout()
  }

  const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5062'

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-4xl mx-auto px-4">
        <div className="bg-white rounded-lg shadow-lg p-8">
          <h1 className="text-3xl font-bold text-center text-gray-800 mb-2">
            🎭 RetoSquadmakers API
          </h1>
          <p className="text-center text-gray-600 mb-8">
            Sistema de Autenticación con OAuth 2.0 y JWT
          </p>

          {!isAuthenticated ? (
            <div className="grid md:grid-cols-2 gap-8">
              {/* OAuth Login */}
              <div className="border rounded-lg p-6">
                <h2 className="text-xl font-semibold mb-4 flex items-center">
                  🔐 Autenticación OAuth
                </h2>
                <p className="text-gray-600 mb-4">
                  Inicia sesión con tu cuenta de Google:
                </p>
                <button
                  onClick={handleGoogleLogin}
                  className="w-full bg-red-600 hover:bg-red-700 text-white font-bold py-3 px-4 rounded-lg transition duration-200"
                >
                  📧 Iniciar sesión con Google
                </button>
              </div>

              {/* Local Login */}
              <div className="border rounded-lg p-6">
                <h2 className="text-xl font-semibold mb-4 flex items-center">
                  🔑 Autenticación Local
                </h2>
                <form onSubmit={handleLogin} className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Email:
                    </label>
                    <input
                      type="email"
                      value={loginForm.email}
                      onChange={(e) => setLoginForm({ ...loginForm, email: e.target.value })}
                      className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Contraseña:
                    </label>
                    <input
                      type="password"
                      value={loginForm.password}
                      onChange={(e) => setLoginForm({ ...loginForm, password: e.target.value })}
                      className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      required
                    />
                  </div>
                  <button
                    type="submit"
                    disabled={loginMutation.isPending}
                    className="w-full bg-blue-600 hover:bg-blue-700 text-white font-bold py-3 px-4 rounded-lg transition duration-200 disabled:opacity-50"
                  >
                    {loginMutation.isPending ? '⏳ Cargando...' : '🚀 Iniciar Sesión'}
                  </button>
                </form>
              </div>

              {/* Create User */}
              <div className="border rounded-lg p-6 md:col-span-2">
                <h2 className="text-xl font-semibold mb-4 flex items-center">
                  👤 Crear Usuario
                </h2>
                <form onSubmit={handleCreateUser} className="grid md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Email:
                    </label>
                    <input
                      type="email"
                      value={createForm.email}
                      onChange={(e) => setCreateForm({ ...createForm, email: e.target.value })}
                      className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Nombre:
                    </label>
                    <input
                      type="text"
                      value={createForm.nombre}
                      onChange={(e) => setCreateForm({ ...createForm, nombre: e.target.value })}
                      className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Contraseña:
                    </label>
                    <input
                      type="password"
                      value={createForm.password}
                      onChange={(e) => setCreateForm({ ...createForm, password: e.target.value })}
                      className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Rol:
                    </label>
                    <select
                      value={createForm.rol}
                      onChange={(e) => setCreateForm({ ...createForm, rol: e.target.value as 'User' | 'Admin' })}
                      className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    >
                      <option value="User">Usuario</option>
                      <option value="Admin">Administrador</option>
                    </select>
                  </div>
                  <button
                    type="submit"
                    disabled={createUserMutation.isPending}
                    className="md:col-span-2 bg-green-600 hover:bg-green-700 text-white font-bold py-3 px-4 rounded-lg transition duration-200 disabled:opacity-50"
                  >
                    {createUserMutation.isPending ? '⏳ Creando...' : '✨ Crear Usuario'}
                  </button>
                </form>
              </div>
            </div>
          ) : (
            <div className="space-y-6">
              {/* User Info */}
              <div className="bg-green-50 border border-green-200 rounded-lg p-6">
                <h2 className="text-xl font-semibold text-green-800 mb-4">
                  ✅ Sesión Activa
                </h2>
                <div className="grid md:grid-cols-2 gap-4">
                  <div>
                    <p><strong>Usuario:</strong> {user?.nombre}</p>
                    <p><strong>Email:</strong> {user?.email}</p>
                    <p><strong>Rol:</strong> {user?.rol}</p>
                  </div>
                  <div>
                    <p><strong>Token JWT:</strong></p>
                    <div className="bg-gray-100 p-2 rounded text-xs font-mono break-all">
                      {user?.token.substring(0, 50)}...
                    </div>
                  </div>
                </div>
              </div>

              {/* API Testing */}
              <div className="border rounded-lg p-6">
                <h2 className="text-xl font-semibold mb-4">🧪 Probar API</h2>
                <div className="flex flex-wrap gap-4 mb-4">
                  <button
                    onClick={handleGetUserInfo}
                    disabled={isLoadingUserInfo}
                    className="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded-lg transition duration-200 disabled:opacity-50"
                  >
                    👤 Obtener Info Usuario
                  </button>
                  <button
                    onClick={handleTestAdmin}
                    disabled={adminTestMutation.isPending}
                    className="bg-purple-600 hover:bg-purple-700 text-white font-bold py-2 px-4 rounded-lg transition duration-200 disabled:opacity-50"
                  >
                    👑 Endpoint Admin
                  </button>
                  <a
                    href={`${apiBaseUrl}/swagger`}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="bg-gray-600 hover:bg-gray-700 text-white font-bold py-2 px-4 rounded-lg transition duration-200"
                  >
                    📖 Swagger UI
                  </a>
                  <button
                    onClick={handleLogout}
                    className="bg-red-600 hover:bg-red-700 text-white font-bold py-2 px-4 rounded-lg transition duration-200"
                  >
                    🚪 Cerrar Sesión
                  </button>
                </div>

                {userInfo && (
                  <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                    <h3 className="font-semibold text-blue-800 mb-2">Información del Usuario:</h3>
                    <div className="grid md:grid-cols-2 gap-2 text-sm">
                      <p><strong>ID:</strong> {userInfo.id}</p>
                      <p><strong>Nombre:</strong> {userInfo.nombre}</p>
                      <p><strong>Email:</strong> {userInfo.email}</p>
                      <p><strong>Rol:</strong> {userInfo.rol}</p>
                      <p><strong>Fecha Creación:</strong> {new Date(userInfo.fechaCreacion).toLocaleString()}</p>
                    </div>
                  </div>
                )}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
