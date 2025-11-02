using WhatsAppBot.Components;
using MudBlazor.Services;
using WhatsAppBot.Services.Interfaces;
using WhatsAppBot.Services;
using WhatsAppBot.Data;
using Microsoft.EntityFrameworkCore;
using WhatsAppBot.Data.Repositories.Interfaces;
using WhatsAppBot.Data.Repositories;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

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

// ✅ UNA SOLA inicialización con logging detallado
try 
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<WhatsAppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    Console.WriteLine("🔧 === INICIALIZACIÓN DEL SISTEMA ===");
    
    // Verificar conexión
    Console.WriteLine("🔧 Verificando conexión a base de datos...");
    var canConnect = await context.Database.CanConnectAsync();
    Console.WriteLine($"✅ Conexión a BD: {canConnect}");
    
    if (canConnect)
    {
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("✅ Base de datos lista");
    }
    else
    {
        throw new Exception("No se puede conectar a la base de datos");
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
    
    // Crear/verificar usuario admin
    Console.WriteLine("🔧 Verificando usuario admin...");
    var adminUser = await userManager.FindByEmailAsync("admin@whatsappbot.com");
    
    if (adminUser == null)
    {
        Console.WriteLine("🔧 Creando usuario admin...");
        adminUser = new IdentityUser
        {
            UserName = "admin@whatsappbot.com",
            Email = "admin@whatsappbot.com",
            EmailConfirmed = true,
            LockoutEnabled = false,
            PhoneNumberConfirmed = true
        };
        
        var createResult = await userManager.CreateAsync(adminUser, "Admin123!");
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
        var passwordCheck = await userManager.CheckPasswordAsync(adminUser, "Admin123!");
        Console.WriteLine($"✅ Contraseña válida: {passwordCheck}");
        
        if (!passwordCheck)
        {
            Console.WriteLine("🔧 Actualizando contraseña...");
            var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            var resetResult = await userManager.ResetPasswordAsync(adminUser, token, "Admin123!");
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
    Console.WriteLine("📋 Credenciales: admin@whatsappbot.com / Admin123!");
    Console.WriteLine("🌐 Login URL: /admin/login");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error crítico en inicialización:");
    Console.WriteLine($"   Mensaje: {ex.Message}");
    Console.WriteLine($"   Tipo: {ex.GetType().Name}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"   Inner: {ex.InnerException.Message}");
    }
    Console.WriteLine($"   Stack: {ex.StackTrace}");
    
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
Console.WriteLine("📋 Login: admin@whatsappbot.com");
Console.WriteLine("🔑 Password: Admin123!");
Console.WriteLine("🌐 URL: /admin/login");
Console.WriteLine("📊 Dashboard: /admin/dashboard");
Console.WriteLine("🚀 ===============================");

app.Run();
