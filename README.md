# WhatsApp Bot con Blazor Server 🤖

## Descripción
Sistema completo de gestión de pedidos automatizado con integración a WhatsApp Business API. Desarrollado con ASP.NET Core 8, Blazor Server y MudBlazor para crear una experiencia de usuario moderna y funcional.

## 🎯 Características Principales

### ✨ Funcionalidades Core
- **Automatización Inteligente**: Respuestas automáticas y gestión de estados de conversación
- **Dashboard en Tiempo Real**: Métricas y estadísticas actualizadas con SignalR
- **Panel Administrativo**: Control completo de pedidos, clientes y conversaciones
- **Landing Page Profesional**: Interfaz pública con estadísticas en tiempo real
- **Gestión de Conversaciones**: Historial completo y envío directo de mensajes
- **Sistema de Pedidos**: Procesamiento automático con estados personalizables

### 🛡️ Seguridad y Arquitectura
- **Autenticación Identity**: Sistema de usuarios con roles administrativos
- **Patrón Repository**: Separación de responsabilidades y testabilidad
- **Unit of Work**: Transacciones consistentes en base de datos
- **Retry Policies**: Manejo robusto de errores con Polly
- **Cache Distribuido**: Redis para optimización de rendimiento

## 📁 Estructura del Proyecto

```
WhatsAppBot/
├── Components/ # Componentes Blazor
│ ├── Layout/
│ │ ├── AdminLayout.razor # Layout administrativo
│ │ └── MainLayout.razor # Layout público
│ └── Pages/
│ ├── Home.razor # Landing page pública
│ ├── Admin/
│ │ ├── Dashboard.razor # Panel de métricas
│ │ ├── ConversationHistory.razor
│ │ ├── DirectMessage.razor
│ │ └── Login.razor
│ └── Pedidos/
│ └── Pedidos.razor # Vista pública de pedidos
├── Controllers/
│ └── WhatsAppController.cs # API para webhooks
├── Data/
│ ├── WhatsAppDbContext.cs # Contexto EF Core
│ ├── DTOs/ # Objetos de transferencia
│ └── Repositories/ # Patrón Repository
├── Models/ # Entidades de dominio
├── Services/ # Lógica de negocio
├── Hubs/
│ └── PedidosHub.cs # SignalR para tiempo real
└── Migrations/ # Migraciones EF Core
```

## 🚀 Guía de Instalación

### Requisitos Previos
- **.NET 8.0 SDK** o superior
- **SQL Server** (LocalDB o instancia completa)
- **Redis Server** (local o remoto)
- **Cuenta WhatsApp Business API** (Meta for Developers)
- **Visual Studio 2022** o VS Code (recomendado)

### Configuración Paso a Paso

#### 1. Clonar el Repositorio
```bash
git clone https://github.com/Aletsis/WhatsAppBotBlazor.git
cd WhatsAppBotBlazor
```

### 2. Configurar Base de Datos

```bash
# Restaurar paquetes NuGet
dotnet restore

# Crear base de datos
dotnet ef database update
```

### 3. Configurar `appsettings.json`

```bash
{
  "WhatsApp": {
    "Token": "TU_WHATSAPP_ACCESS_TOKEN",
    "PhoneNumberId": "TU_PHONE_NUMBER_ID"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WhatsAppBotDB;Trusted_Connection=true;TrustServerCertificate=true;",
    "Redis": "localhost:6379"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### 4. Configurar WhatsApp Business API
1. Crear aplicación en Meta for Developers
2. Agregar producto "WhatsApp Business API"
3. Obtener Access Token y Phone Number ID
4. Configurar webhook URL: https://tu-dominio.com/api/whatsapp/webhook
5. Configurar verify token: qwerty (cambiar en producción)

### 5. Instalar y Configurar Redis

```bash
# Windows (con Chocolatey)
choco install redis-64

# macOS (con Homebrew)
brew install redis

# Ubuntu
sudo apt install redis-server
```

### 6. Ejecutar la Aplicacion

```bash
dotnet run
```

La aplicación estará disponible en:

- Sitio público: https://localhost:5001
- Panel admin: https://localhost:5001/admin