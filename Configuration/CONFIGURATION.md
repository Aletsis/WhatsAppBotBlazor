# 🔐 Guía de Configuración Segura - WhatsApp Bot

## ⚠️ IMPORTANTE: Seguridad de Credenciales

Este proyecto utiliza **configuración segura** para proteger credenciales sensibles. **NUNCA** commitees archivos con tokens o contraseñas reales al repositorio.

---

## 🏠 Desarrollo Local

### Opción 1: User Secrets (⭐ RECOMENDADO)

Los User Secrets almacenan configuración sensible fuera del proyecto, evitando exposición accidental.

#### Paso 1: Inicializar User Secrets

Abre PowerShell en la raíz del proyecto y ejecuta:

```powershell
dotnet user-secrets init --project WhatsAppBot.csproj
```
Esto agregará un `UserSecretsId` a tu archivo `.csproj`

#### Paso 2:Configurar Valores Sensibles

Ejecuta los siguientes comandos reemplazando los valores con tus credenciales reales:

```
# Token de WhatsApp Business API
dotnet user-secrets set "WhatsApp:Token" "TU_TOKEN_DE_WHATSAPP"

# ID del número de teléfono de WhatsApp Business
dotnet user-secrets set "WhatsApp:PhoneNumberId" "TU_PHONE_NUMBER_ID"

# Token de verificación del webhook (crea uno fuerte y aleatorio)
dotnet user-secrets set "WhatsApp:VerifyToken" "token-aleatorio-minimo-20-caracteres"

# Cadena de conexión a SQL Server
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=WhatsAppBotDB;User Id=TU_USUARIO;Password=TU_PASSWORD;TrustServerCertificate=True;"

# App Secret de Meta (para validar webhooks)
dotnet user-secrets set "Security:AppSecret" "TU_APP_SECRET_DE_META"

# Contraseña del usuario administrador
dotnet user-secrets set "AdminUser:Password" "TuPasswordSegura123!"
```

#### Paso 3: Verificar Configuración

```
dotnet user-secrets list
```

Deberías ver todos tus secretos configurados (sin exponer los valores completos).

---

### Opción 2: Variables de entorno

si prefieres usar variables de entorno:

#### En windows (PowerShell):

```
# Cargar desde archivo .env
Get-Content .env | ForEach-Object {
    if ($_ -match '^([^=]+)=(.*)$') {
        [Environment]::SetEnvironmentVariable($matches[1], $matches[2], "Process")
    }
}
```

#### Configurar manualmente:

```
$env:WHATSAPP__TOKEN = "tu_token"
$env:WHATSAPP__PHONENUMBERID = "tu_phone_id"
$env:WHATSAPP__VERIFYTOKEN = "tu_verify_token"
$env:CONNECTIONSTRINGS__DEFAULTCONNECTION = "tu_connection_string"
$env:SECURITY__APPSECRET = "tu_app_secret"
$env:ADMINUSER__PASSWORD = "tu_password_admin"
```

**Nota:** Los `__`(doble guion bajo) representan la jerarquia de configuración.

---

## 🚀 Entornos de Producción

### Azure App Service

1. Ve al Portal de Azure
2. Selecciona tu App Service
3. Ve a Configuration → Application settings
4. Agrega cada configuración:

```
WhatsApp:Token = valor_secreto
WhatsApp:PhoneNumberId = valor
WhatsApp:VerifyToken = valor
ConnectionStrings:DefaultConnection = cadena_conexion
Security:AppSecret = app_secret
AdminUser:Password = password
```

### Azure Key Vault (Recomendado para Producción)

```
// En Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### Docker
Usa archivo `.env` o pasa variables al contenedor:

```
docker run -d \
  -e WHATSAPP__TOKEN="token" \
  -e WHATSAPP__PHONENUMBERID="phone_id" \
  -e WHATSAPP__VERIFYTOKEN="verify_token" \
  -e CONNECTIONSTRINGS__DEFAULTCONNECTION="connection_string" \
  -e SECURITY__APPSECRET="app_secret" \
  -e ADMINUSER__PASSWORD="password" \
  tu-imagen:latest
```

O usar `docker-compose.yml`:

```
version: '3.8'
services:
  whatsapp-bot:
    image: tu-imagen:latest
    environment:
      - WHATSAPP__TOKEN=${WHATSAPP_TOKEN}
      - WHATSAPP__PHONENUMBERID=${WHATSAPP_PHONENUMBERID}
      - WHATSAPP__VERIFYTOKEN=${WHATSAPP_VERIFYTOKEN}
      - CONNECTIONSTRINGS__DEFAULTCONNECTION=${DB_CONNECTION}
      - SECURITY__APPSECRET=${APP_SECRET}
      - ADMINUSER__PASSWORD=${ADMIN_PASSWORD}
    env_file:
      - .env.production
```

### IIS

Configura en `web.config` usando `<environmentVariables>` o establece variables de sistema en el servidor.

---

## 🔑 Obtener Credenciales de WhatsApp Business API

### 1. Token de Acceso (WhatsApp:Token)
1. Ve a Meta for Developers
2. Selecciona tu aplicación
3. Ve a WhatsApp → API Setup
4. Copia el Temporary access token o genera uno permanente

### 2. hone Number ID (WhatsApp:PhoneNumberId)
1. En la misma página de API Setup
2. Busca Phone number ID debajo de tu número de teléfono
3. Copia el ID

### 3. Verify Token (WhatsApp:VerifyToken)
Es un token que TÚ creas. Debe ser:

* Aleatorio y único
* Mínimo 20 caracteres
* Combinar letras, números y símbolos

Ejemplo de generacion segura:

```
# PowerShell: Generar token aleatorio
-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | ForEach-Object {[char]$_})
```

### 4. App Secret (Security:AppSecret)

1. En tu aplicación de Meta for Developers
2. Ve a Settings → Basic
3. Copia el App Secret

---

## 📋 Archivos de Referencia

`appsettings.template-json`
Archivo de plantilla con todas las configuraciones necesarias (sin valores reales). Úsalo como referencia.

`appsettings.json`
Contiene solo estructura vacía. Los valores reales se cargan desde User Secrets o variables de entorno.

`.env.example`
Plantilla para variables de entorno. Copia como .env y completa con valores reales.

---

## ✅ Checklist de Seguridad

Antes de deployar a producción:

 - [] Todos los secretos están en User Secrets o variables de entorno
 - [] appsettings.json NO contiene valores sensibles
 - [] .gitignore excluye appsettings.json y archivos .env
 - [] Los tokens son fuertes y aleatorios (mínimo 20 caracteres)
 - [] AppSecret configurado para validar webhooks
 - [] Contraseña de administrador cumple requisitos de seguridad
 - [] Tokens de WhatsApp tienen permisos mínimos necesarios
 - [] Se configuró rotación periódica de tokens

 ---

 ## 🆘 Solución de Problemas

#### Error: "El Token de WhatsApp es obligatorio"

Significa que la configuración no se está cargando. Verifica:

1. User Secrets configurados correctamente: `dotnet user-secrets list`
2. Variables de entorno establecidas en el sistema
3. En Azure: Application Settings configuradas correctamente

#### La aplicación no se conecta a la base de datos

Verifica:

1. `ConnectionStrings:DefaultConnection` configurada
2. SQL Server está corriendo
3. Credenciales de la BD son correctas

#### Webhook no funciona
1. Verifica que `WhatsApp:VerifyToken` coincida con el configurado en Meta
2. Implementa validación de firma (descomentar código en `WhatsAppController.cs`)
3. Revisa logs para ver detalles del error

---

