# RetoSquadmakers - API de Chistes con Autenticación y Notificaciones

Este proyecto implementa una API REST completa para la gestión de chistes con autenticación JWT/OAuth2, sistema de notificaciones y arquitectura hexagonal.

## 📋 Tabla de Contenidos

- [Descripción General](#descripción-general)
- [Ejercicios Implementados](#ejercicios-implementados)
- [Configuración del Proyecto](#configuración-del-proyecto)
- [Base de Datos](#base-de-datos)
- [Ejecución](#ejecución)
- [Pruebas y Testing](#pruebas-y-testing)
- [API Endpoints](#api-endpoints)
- [Arquitectura](#arquitectura)

## 🚀 Descripción General

El proyecto RetoSquadmakers es una API REST desarrollada en .NET 8 que implementa tres ejercicios principales:

1. **Sistema de Autenticación** - JWT y OAuth2 con Google
2. **Gestión de Chistes** - CRUD completo con filtros avanzados
3. **Sistema de Notificaciones** - Notificaciones multi-canal con templates

## 📚 Ejercicios Implementados

### EJERCICIO 1 - Sistema de Autenticación JWT y OAuth2

**Descripción**: Sistema completo de autenticación que soporta login tradicional con JWT y autenticación social con Google OAuth2.

**Funcionalidades**:
- ✅ Registro y login de usuarios
- ✅ Autenticación JWT con tokens seguros
- ✅ Integración con Google OAuth2
- ✅ Gestión de roles (User/Admin)
- ✅ Middleware de autorización
- ✅ Validación de tokens y refresh tokens

**Cómo probarlo**:
1. **Registro de usuario**:
   ```bash
   POST http://localhost:5062/api/auth/register
   {
     "nombre": "Juan Pérez",
     "email": "juan@example.com",
     "password": "MiPassword123!"
   }
   ```

2. **Login tradicional**:
   ```bash
   POST http://localhost:5062/api/auth/login
   {
     "email": "juan@example.com",
     "password": "MiPassword123!"
   }
   ```

3. **Login con Google**:
   - Visitar: `http://localhost:5062/api/auth/google`
   - Autorizar con tu cuenta de Google
   - Recibirás el JWT token

### EJERCICIO 2 - Gestión de Chistes

**Descripción**: Sistema completo de gestión de chistes con CRUD, filtros avanzados, categorización por temáticas y arquitectura hexagonal.

**Funcionalidades**:
- ✅ CRUD completo de chistes
- ✅ Sistema de temáticas (categorías)
- ✅ Filtros avanzados (por autor, temática, contenido, etc.)
- ✅ Chistes aleatorios locales
- ✅ Validaciones de dominio
- ✅ Arquitectura hexagonal completa

**Cómo probarlo**:
1. **Crear un chiste** (requiere autenticación):
   ```bash
   POST http://localhost:5062/api/chistes
   Authorization: Bearer {tu-jwt-token}
   {
     "texto": "¿Por qué los programadores prefieren el modo oscuro? Porque la luz atrae a los bugs!",
     "tematicaIds": [1, 2]
   }
   ```

2. **Obtener chistes con filtros**:
   ```bash
   GET http://localhost:5062/api/chistes?contiene=programador&autorId=1&tematicaId=2
   ```

3. **Chistes aleatorios**:
   ```bash
   GET http://localhost:5062/api/chistes/random/5
   ```

4. **Gestionar temáticas**:
   ```bash
   GET http://localhost:5062/api/chistes/tematicas
   POST http://localhost:5062/api/chistes/tematicas
   {
     "nombre": "Programación"
   }
   ```

### EJERCICIO 3 - Sistema de Notificaciones

**Descripción**: Sistema completo de notificaciones multi-canal con templates, cola de procesamiento en background, preferencias de usuario y integración con eventos del sistema.

**Funcionalidades**:
- ✅ Notificaciones Email (Gmail SMTP)
- ✅ Notificaciones SMS (simuladas)
- ✅ Notificaciones Push (simuladas)
- ✅ Sistema de templates con variables
- ✅ Cola de procesamiento en background
- ✅ Preferencias de usuario por tipo de notificación
- ✅ Integración automática con eventos (nuevo chiste)
- ✅ API REST completa para gestión
- ✅ Inyección de dependencias

**Cómo probarlo**:
1. **Enviar notificación individual**:
   ```bash
   POST http://localhost:5062/api/notification/send
   Authorization: Bearer {tu-jwt-token}
   {
     "type": "Email",
     "subject": "Test Notification",
     "content": "Esta es una notificación de prueba",
     "priority": "Normal"
   }
   ```

2. **Enviar notificaciones masivas** (solo Admin):
   ```bash
   POST http://localhost:5062/api/notification/send-bulk
   Authorization: Bearer {admin-jwt-token}
   {
     "userIds": [1, 2, 3],
     "type": "Email",
     "subject": "Notificación Masiva",
     "content": "Mensaje para todos los usuarios",
     "priority": "High"
   }
   ```

3. **Gestionar preferencias**:
   ```bash
   GET http://localhost:5062/api/notification/preferences
   PUT http://localhost:5062/api/notification/preferences
   {
     "notificationType": "Email",
     "eventType": "chiste_created",
     "isEnabled": true
   }
   ```

4. **Ver historial de notificaciones**:
   ```bash
   GET http://localhost:5062/api/notification/history?page=1&pageSize=10&status=Sent
   ```

## ⚙️ Configuración del Proyecto

### Requisitos Previos

- **.NET 8.0 SDK** - [Descargar aquí](https://dotnet.microsoft.com/download/dotnet/8.0)
- **PostgreSQL** - [Descargar aquí](https://www.postgresql.org/download/)
- **Git** - Para clonar el repositorio
- **Cuenta de Google** - Para OAuth2 (opcional)
- **Cuenta de Gmail** - Para SMTP (opcional)

### Variables de Entorno

Crear un archivo `appsettings.Development.json` con:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=retoSquadmakers;Username=tu_usuario;Password=tu_password"
  },
  "JwtSettings": {
    "SecretKey": "tu-clave-secreta-muy-larga-y-segura-de-al-menos-256-bits",
    "Issuer": "RetoSquadmakers",
    "Audience": "RetoSquadmakersUsers",
    "ExpirationHours": 24
  },
  "Authentication": {
    "Google": {
      "ClientId": "tu-google-client-id.apps.googleusercontent.com",
      "ClientSecret": "tu-google-client-secret"
    }
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "tu-email@gmail.com",
    "Password": "tu-app-password",
    "FromEmail": "tu-email@gmail.com",
    "FromName": "RetoSquadmakers"
  }
}
```

### Configuración de Google OAuth2

1. Ir a [Google Cloud Console](https://console.cloud.google.com/)
2. Crear un nuevo proyecto o seleccionar uno existente
3. Habilitar la API de Google+
4. Crear credenciales OAuth 2.0
5. Agregar las URIs de redirección:
   - `http://localhost:5062/signin-google`
   - `https://localhost:7078/signin-google`

### Configuración de Gmail SMTP

1. Habilitar autenticación de 2 factores en tu cuenta Gmail
2. Generar una contraseña de aplicación
3. Usar esa contraseña en `EmailSettings.Password`

## 🗄️ Base de Datos

### Configuración de PostgreSQL

1. **Instalar PostgreSQL** y crear una base de datos:
   ```sql
   CREATE DATABASE retoSquadmakers;
   CREATE USER tu_usuario WITH PASSWORD 'tu_password';
   GRANT ALL PRIVILEGES ON DATABASE retoSquadmakers TO tu_usuario;
   ```

2. **Ejecutar migraciones**:
   ```bash
   dotnet ef database update
   ```

### Estructura de la Base de Datos

El sistema utiliza las siguientes tablas principales:

- **Usuarios** - Información de usuarios y roles
- **Chistes** - Chistes con autor y metadatos
- **Tematicas** - Categorías de chistes
- **ChisteTematicas** - Relación many-to-many entre chistes y temáticas
- **Notifications** - Registro de notificaciones enviadas
- **NotificationPreferences** - Preferencias de usuario
- **NotificationTemplates** - Templates de notificaciones

## 🏃‍♂️ Ejecución

### Opción 1: Ejecución en Primer Plano
```bash
# Clonar el repositorio
git clone <url-del-repositorio>
cd retoSquadmakers

# Restaurar dependencias
dotnet restore

# Aplicar migraciones
dotnet ef database update

# Ejecutar la aplicación
dotnet run
```

### Opción 2: Ejecución en Background (Recomendado)
```bash
# Ejecutar en background con logs
nohup dotnet run > /tmp/dotnet-app.log 2>&1 &

# Verificar que está corriendo
ps aux | grep dotnet | grep -v grep

# Ver logs en tiempo real
tail -f /tmp/dotnet-app.log

# Verificar que el puerto está escuchando
ss -tlnp | grep :5062

# Parar la aplicación
pkill -f "dotnet run"
```

### URLs de Acceso

- **API**: http://localhost:5062
- **HTTPS**: https://localhost:7078
- **Swagger UI**: http://localhost:5062/swagger (solo en desarrollo)

## 🧪 Pruebas y Testing

### Ejecutar Pruebas Unitarias

```bash
# Ejecutar todas las pruebas
dotnet test

# Ejecutar con coverage
dotnet test --collect:"XPlat Code Coverage"

# Ver coverage detallado
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage -reporttypes:Html
```

### Pruebas con Archivo HTTP

Usar el archivo `retoSquadmakers.http` con extensiones REST Client:

```http
### Registro de usuario
POST http://localhost:5062/api/auth/register
Content-Type: application/json

{
  "nombre": "Test User",
  "email": "test@example.com",
  "password": "TestPassword123!"
}

### Login
POST http://localhost:5062/api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "TestPassword123!"
}

### Crear chiste (usar token del login)
POST http://localhost:5062/api/chistes
Authorization: Bearer {token-aquí}
Content-Type: application/json

{
  "texto": "Mi primer chiste de prueba con más de 10 caracteres",
  "tematicaIds": []
}
```

## 📡 API Endpoints

### Autenticación
- `POST /api/auth/register` - Registro de usuario
- `POST /api/auth/login` - Login con email/password
- `GET /api/auth/google` - Login con Google OAuth2
- `GET /api/auth/user` - Información del usuario actual

### Chistes
- `GET /api/chistes` - Listar chistes con filtros
- `POST /api/chistes` - Crear chiste
- `GET /api/chistes/{id}` - Obtener chiste por ID
- `PUT /api/chistes/{id}` - Actualizar chiste
- `DELETE /api/chistes/{id}` - Eliminar chiste
- `GET /api/chistes/random/{count}` - Chistes aleatorios
- `GET /api/chistes/tematicas` - Listar temáticas
- `POST /api/chistes/tematicas` - Crear temática

### Notificaciones
- `POST /api/notification/send` - Enviar notificación
- `POST /api/notification/send-bulk` - Envío masivo (Admin)
- `GET /api/notification/history` - Historial personal
- `GET /api/notification/preferences` - Ver preferencias
- `PUT /api/notification/preferences` - Actualizar preferencias
- `GET /api/notification/templates` - Listar templates (Admin)
- `POST /api/notification/templates` - Crear template (Admin)

## 🏗️ Arquitectura

### Arquitectura Hexagonal (Clean Architecture)

```
├── Domain/                     # Núcleo del dominio
│   ├── Entities/              # Entidades de dominio
│   ├── Repositories/          # Interfaces de repositorios
│   ├── Services/              # Interfaces de servicios
│   └── ValueObjects/          # Objetos de valor
│
├── Application/               # Lógica de aplicación
│   ├── Services/              # Servicios de aplicación
│   ├── UseCases/              # Casos de uso
│   ├── DTOs/                  # Objetos de transferencia
│   └── EventHandlers/         # Manejadores de eventos
│
├── Infrastructure/            # Infraestructura
│   ├── Persistence/           # Repositorios y DbContext
│   ├── ExternalServices/      # Servicios externos
│   ├── BackgroundServices/    # Servicios en background
│   └── Security/              # JWT, OAuth2, etc.
│
├── Presentation/              # Capa de presentación
│   └── Controllers/           # Controladores HTTP
│
└── Controllers/               # Controladores adicionales
```

### Tecnologías Utilizadas

- **.NET 8** - Framework principal
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **PostgreSQL** - Base de datos
- **JWT** - Autenticación
- **Google OAuth2** - Autenticación social
- **Gmail SMTP** - Envío de emails
- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **Swagger/OpenAPI** - Documentación de API

## 🎯 Cobertura de Pruebas

Cobertura actual de pruebas unitarias:
- **Coverage Total**: ~41%
- **Domain Services**: 85%+
- **Repository Layer**: 75%+
- **Use Cases**: 90%+

## 🔧 Solución de Problemas

### Aplicación no inicia
1. Verificar que PostgreSQL esté ejecutándose
2. Verificar cadena de conexión en `appsettings.json`
3. Ejecutar `dotnet ef database update`
4. Verificar que el puerto 5062 esté libre

### Errores de autenticación
1. Verificar configuración de JWT en `appsettings.json`
2. Para Google OAuth2, verificar Client ID y Secret
3. Verificar URIs de redirección en Google Console

### Notificaciones no se envían
1. Verificar configuración SMTP de Gmail
2. Verificar que el servicio background esté ejecutándose
3. Revisar logs en `/tmp/dotnet-app.log`

## 📋 Instrucciones Específicas por Ejercicio

### Para probar EJERCICIO 1 (Autenticación):
1. Levantar la aplicación con `dotnet run`
2. Ir a `http://localhost:5062/swagger`
3. Probar el endpoint `/api/auth/register` para crear un usuario
4. Usar `/api/auth/login` para obtener el JWT token
5. Usar el token en los headers: `Authorization: Bearer {token}`

### Para probar EJERCICIO 2 (Gestión de Chistes):
1. Autenticarse primero (ver Ejercicio 1)
2. Crear temáticas con `POST /api/chistes/tematicas`
3. Crear chistes con `POST /api/chistes`
4. Probar filtros con `GET /api/chistes?contiene=texto&autorId=1`
5. Probar chistes aleatorios con `GET /api/chistes/random/5`

### Para probar EJERCICIO 3 (Notificaciones):
1. Configurar Gmail SMTP en `appsettings.Development.json`
2. Autenticarse como usuario normal
3. Enviar notificación con `POST /api/notification/send`
4. Ver historial con `GET /api/notification/history`
5. Gestionar preferencias con `GET/PUT /api/notification/preferences`
6. Para funciones de admin, necesitas crear un usuario con rol "Admin" en la BD

### Frontend (Opcional)
Este proyecto es solo backend API. Para un frontend, puedes:
1. Usar herramientas como Postman o Insomnia
2. Usar la documentación Swagger en `/swagger`
3. Usar el archivo `retoSquadmakers.http` con extensiones REST Client
4. Crear un frontend en React/Angular/Vue que consuma la API

## 📄 Licencia

Este proyecto es parte del RetoSquadmakers y está desarrollado con fines educativos.

---

**¡Desarrollado con ❤️  por jofelvi y mucho ☕ para RetoSquadmakers! sorry por la demora es que cuando le piden a un dev senior un proyecto asi nos ponemos en muchos escenarios y nos ponemos a pensar en todo asi que bueno les di un preview de lo que puedo hacer y ayudarles con el proyecto**

