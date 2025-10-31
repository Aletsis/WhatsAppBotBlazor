# WhatsAppBot 🤖

## Bot de WhatsApp integrado con Blazor que permite gestionar pedidos y comunicación automatizada. Proyecto en desarrollo.

### Fases del Proyecto 📅
#### Fase 1 - Completada ✅
- Arquitectura base del proyecto
- Integración con WhatsApp Business API
- Interfaz web básica con Blazor y MudBlazor
 - Sistema de base de datos y modelos
#### Fase 2 - En Desarrollo 🚧
- Gestión de conversaciones automatizadas
- Sistema de pedidos en tiempo real
- Panel de administración
- Caché distribuida con Redis
#### Fase 3 - Pendiente 📋
- Reportes y analíticas
- Integración con sistemas de pago
- Optimización y escalabilidad
- Tests automatizados
#### Características Actuales ✨
* Interfaz web construida con Blazor y MudBlazor
* Integración con la API de WhatsApp
* Sistema de pedidos en tiempo real con SignalR
* Base de datos SQL Server con Entity Framework Core
* Caché distribuida con Redis
* Manejo de conversaciones automatizadas
* Gestión de clientes y pedidos
#### Requisitos Previos 📋
* .NET 8.0
* SQL Server
* Redis
* Cuenta de WhatsApp Business API
#### Instalación 🚀
1. Clona el repositorio:
```
git clone https://github.com/Aletsis/WhatsAppBot.git
```
2. Configura la cadena de conexión en `appsettings.json`:
```
{
  "ConnectionStrings": {
    "DefaultConnection": "tu-conexion-sql-server",
    "Redis": "tu-conexion-redis"
  }
}
```
3. Ejecuta las migraciones:
```
dotnet ef database update
```
4. Inicia la aplicacion
```
dotnet run
```

#### Estructura del Proyecto 📁
`Components` - Componentes Blazor y páginas\
`Controllers` - Controladores API\
`Data` - Contexto y configuración de EF Core\
`Models` - Modelos de datos\
`Services` - Servicios de negocio\
`Hubs` - Hubs de SignalR
#### Tecnologías Utilizadas 🛠️
- ASP.NET Core 8
- Blazor Server
- Entity Framework Core
- SignalR
- MudBlazor
- Redis
- SQL Server
- Polly
#### Estado Actual ⚠️
Este proyecto está actualmente en desarrollo activo. Algunas características pueden no estar completamente implementadas o pueden cambiar significativamente.

#### Contribuir 🤝
Las contribuciones son bienvenidas. Por favor, abre un issue primero para discutir los cambios que te gustaría realizar.

#### Licencia 📄
MIT
