using WhatsAppBot.Components;
using MudBlazor.Services;
using WhatsAppBot.Services.Interfaces;
using WhatsAppBot.Services;
using WhatsAppBot.Data;
using Microsoft.EntityFrameworkCore;
using WhatsAppBot.Data.Repositories.Interfaces;
using WhatsAppBot.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using WhatsAppBot.Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ✅ Configurar Options Pattern con validación en startup
builder.Services.Configure<WhatsAppSettings>(
    builder.Configuration.GetSection("WhatsApp"));

builder.Services.Configure<SecuritySettings>(
    builder.Configuration.GetSection("Security"));

builder.Services.Configure<AdminUserSettings>(
    builder.Configuration.GetSection("AdminUser"));

// ✅ Validar configuración crítica al inicio
builder.Services.AddOptions<WhatsAppSettings>()
    .Bind(builder.Configuration.GetSection("WhatsApp"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<SecuritySettings>()
    .Bind(builder.Configuration.GetSection("Security"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<AdminUserSettings>()
    .Bind(builder.Configuration.GetSection("AdminUser"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ✅ Servicios básicos con renderizado interactivo habilitado
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(1);
        options.HandshakeTimeout = TimeSpan.FromSeconds(30);
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    });

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(1);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

builder.Services.AddMudServices();
builder.Services.AddHttpClient();

// Cache necesario para WebhookService
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();

// ✅ DbContext con configuración mejorada
builder.Services.AddDbContext<WhatsAppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableServiceProviderCaching();
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
});

// ✅ Identity configurado correctamente
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<WhatsAppDbContext>()
.AddDefaultTokenProviders();

// ✅ Configuración de cookies mejorada para Blazor Server
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/admin/login";
    options.LogoutPath = "/admin/logout";
    options.AccessDeniedPath = "/admin/accessdenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Autorización
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"));
});

// ✅ Servicios en orden correcto
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IMensajeRepository, MensajeRepository>();
builder.Services.AddScoped<IEstadoConversacionRepository, EstadoConversacionRepository>();

builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IConversacionService, ConversacionService>();
builder.Services.AddScoped<IHistoryMessageService, HistoryMessageService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();

builder.Services.AddControllers();

var app = builder.Build();

// ✅ UNA SOLA inicialización con logging detallado y diagnóstico mejorado
try 
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<WhatsAppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var adminSettings = scope.ServiceProvider.GetRequiredService<IOptions<AdminUserSettings>>().Value;
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    
    Console.WriteLine("🔧 === INICIALIZACIÓN DEL SISTEMA ===");
    
    // ✅ DIAGNÓSTICO: Verificar que la configuración se cargó correctamente
    Console.WriteLine("🔍 === DIAGNÓSTICO DE CONFIGURACIÓN ===");
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        Console.WriteLine("❌ CRÍTICO: ConnectionString 'DefaultConnection' no está configurada");
        Console.WriteLine("💡 Solución:");
        Console.WriteLine("   1. Ejecuta: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"Server=localhost;Database=WhatsAppBotDB;User Id=Pruebas;Password=Fina2017;TrustServerCertificate=True;\"");
        Console.WriteLine("   2. O verifica que appsettings.json tenga la estructura correcta");
        throw new InvalidOperationException("ConnectionString no configurada. Verifica User Secrets o appsettings.json");
    }
    
    // Mostrar conexión sin password (para seguridad)
    var safeConnectionString = System.Text.RegularExpressions.Regex.Replace(
        connectionString, 
        @"Password=([^;]+)", 
        "Password=***");
    Console.WriteLine($"📋 Connection String: {safeConnectionString}");
    
    // Verificar otras configuraciones críticas
    var whatsAppSettings = scope.ServiceProvider.GetRequiredService<IOptions<WhatsAppSettings>>().Value;
    Console.WriteLine($"📱 WhatsApp Token: {(string.IsNullOrEmpty(whatsAppSettings.Token) ? "❌ NO CONFIGURADO" : "✅ Configurado")}");
    Console.WriteLine($"📱 WhatsApp PhoneNumberId: {(string.IsNullOrEmpty(whatsAppSettings.PhoneNumberId) ? "❌ NO CONFIGURADO" : "✅ Configurado")}");
    Console.WriteLine($"📱 WhatsApp VerifyToken: {(string.IsNullOrEmpty(whatsAppSettings.VerifyToken) ? "❌ NO CONFIGURADO" : "✅ Configurado")}");
    Console.WriteLine($"👤 Admin Email: {adminSettings.Email}");
    Console.WriteLine($"👤 Admin Password: {(string.IsNullOrEmpty(adminSettings.Password) ? "❌ NO CONFIGURADO" : "✅ Configurado")}");
    Console.WriteLine("🔍 === FIN DIAGNÓSTICO ===\n");
    
    // Verificar conexión a base de datos
    Console.WriteLine("🔧 Verificando conexión a base de datos...");
    
    try
    {
        var canConnect = await context.Database.CanConnectAsync();
        Console.WriteLine($"✅ Conexión a BD: {canConnect}");
        
        if (!canConnect)
        {
            Console.WriteLine("❌ No se pudo conectar a la base de datos");
            Console.WriteLine("💡 Posibles causas:");
            Console.WriteLine("   1. SQL Server no está corriendo");
            Console.WriteLine("   2. Credenciales incorrectas en ConnectionString");
            Console.WriteLine("   3. Nombre de servidor incorrecto");
            Console.WriteLine("   4. Base de datos no existe y no tiene permisos para crearla");
            Console.WriteLine("\n🔧 Verifica:");
            Console.WriteLine("   - Ejecuta 'sqlcmd -S localhost -U Pruebas -P Fina2017' para probar la conexión");
            Console.WriteLine("   - O verifica que SQL Server esté corriendo");
            
            throw new InvalidOperationException("No se puede conectar a la base de datos. Verifica que SQL Server esté corriendo y las credenciales sean correctas.");
        }
        
        // ✅ CORRECCIÓN: Aplicar migraciones en lugar de EnsureCreated
        Console.WriteLine("🔧 Verificando estructura de base de datos...");
        
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        
        Console.WriteLine($"📊 Migraciones aplicadas: {appliedMigrations.Count()}");
        Console.WriteLine($"📊 Migraciones pendientes: {pendingMigrations.Count()}");
        
        if (pendingMigrations.Any())
        {
            Console.WriteLine($"🔄 Aplicando {pendingMigrations.Count()} migraciones...");
            foreach (var migration in pendingMigrations)
            {
                Console.WriteLine($"   - {migration}");
            }
            
            await context.Database.MigrateAsync();
            Console.WriteLine("✅ Migraciones aplicadas correctamente");
        }
        else if (!appliedMigrations.Any())
        {
            Console.WriteLine("⚠️ No hay migraciones aplicadas. Aplicando todas las migraciones...");
            await context.Database.MigrateAsync();
            Console.WriteLine("✅ Base de datos inicializada con migraciones");
        }
        else
        {
            Console.WriteLine("✅ Base de datos ya está actualizada");
        }
    }
    catch (Exception dbEx)
    {
        Console.WriteLine($"❌ Error de base de datos: {dbEx.Message}");
        if (dbEx.InnerException != null)
        {
            Console.WriteLine($"   Detalle: {dbEx.InnerException.Message}");
        }
        throw;
    }
    
    // Crear rol Admin
    Console.WriteLine("🔧 Verificando rol Admin...");
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        var roleResult = await roleManager.CreateAsync(new IdentityRole("Admin"));
        Console.WriteLine($"✅ Rol Admin creado: {roleResult.Succeeded}");
        
        if (!roleResult.Succeeded)
        {
            foreach (var error in roleResult.Errors)
            {
                Console.WriteLine($"❌ Error rol: {error.Description}");
            }
        }
    }
    else
    {
        Console.WriteLine("✅ Rol Admin ya existe");
    }
    
    // Crear/verificar usuario admin usando configuración segura
    Console.WriteLine("🔧 Verificando usuario admin...");
    var adminUser = await userManager.FindByEmailAsync(adminSettings.Email);
    
    if (adminUser == null)
    {
        Console.WriteLine("🔧 Creando usuario admin...");
        adminUser = new IdentityUser
        {
            UserName = adminSettings.Email,
            Email = adminSettings.Email,
            EmailConfirmed = true,
            LockoutEnabled = false,
            PhoneNumberConfirmed = true
        };
        
        var createResult = await userManager.CreateAsync(adminUser, adminSettings.Password);
        Console.WriteLine($"✅ Usuario creado: {createResult.Succeeded}");
        
        if (createResult.Succeeded)
        {
            var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine($"✅ Rol asignado: {roleResult.Succeeded}");
        }
        else
        {
            Console.WriteLine("❌ Errores al crear usuario:");
            foreach (var error in createResult.Errors)
            {
                Console.WriteLine($"   - {error.Code}: {error.Description}");
            }
        }
    }
    else
    {
        Console.WriteLine("✅ Usuario admin existe");
        
        // Verificaciones adicionales
        var passwordCheck = await userManager.CheckPasswordAsync(adminUser, adminSettings.Password);
        Console.WriteLine($"✅ Contraseña válida: {passwordCheck}");
        
        if (!passwordCheck)
        {
            Console.WriteLine("🔧 Actualizando contraseña...");
            var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            var resetResult = await userManager.ResetPasswordAsync(adminUser, token, adminSettings.Password);
            Console.WriteLine($"✅ Contraseña actualizada: {resetResult.Succeeded}");
        }
        
        var hasAdminRole = await userManager.IsInRoleAsync(adminUser, "Admin");
        Console.WriteLine($"✅ Tiene rol Admin: {hasAdminRole}");
        
        if (!hasAdminRole)
        {
            var addRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine($"✅ Rol Admin agregado: {addRoleResult.Succeeded}");
        }
    }
    
    Console.WriteLine("🚀 === INICIALIZACIÓN COMPLETADA ===");
    Console.WriteLine($"📋 Credenciales: {adminSettings.Email}");
    Console.WriteLine("🌐 Login URL: /admin/login");
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ ============================================");
    Console.WriteLine($"❌ ERROR CRÍTICO EN INICIALIZACIÓN");
    Console.WriteLine($"❌ ============================================");
    Console.WriteLine($"   Mensaje: {ex.Message}");
    Console.WriteLine($"   Tipo: {ex.GetType().Name}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"   Inner Exception: {ex.InnerException.Message}");
    }
    
    Console.WriteLine($"\n📋 Stack Trace:");
    Console.WriteLine(ex.StackTrace);
    
    Console.WriteLine($"\n💡 PASOS PARA SOLUCIONAR:");
    Console.WriteLine("   1. Verifica User Secrets: dotnet user-secrets list");
    Console.WriteLine("   2. Verifica SQL Server esté corriendo");
    Console.WriteLine("   3. Revisa ConnectionString y credenciales");
    Console.WriteLine("   4. Revisa el archivo CONFIGURATION.md para más detalles");
    Console.WriteLine($"❌ ============================================\n");
    
    throw; // Fallar rápido si hay problemas críticos
}

// ✅ Pipeline mejorado
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// ✅ Orden crítico para autenticación
app.UseAuthentication();
app.UseAuthorization();

// ✅ Mapeo con renderizado interactivo habilitado
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous(); // Permitir acceso anónimo para login

app.MapControllers();

// ✅ Información final de arranque
Console.WriteLine("🚀 ===============================");
Console.WriteLine("🚀 SISTEMA WHATSAPP BOT INICIADO");
Console.WriteLine("🚀 ===============================");
Console.WriteLine($"📋 Login: {app.Services.GetRequiredService<IOptions<AdminUserSettings>>().Value.Email}");
Console.WriteLine("🌐 URL: /admin/login");
Console.WriteLine("📊 Dashboard: /admin/dashboard");
Console.WriteLine("🚀 ===============================");

app.Run();
