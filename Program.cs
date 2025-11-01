using WhatsAppBot.Components;
using MudBlazor.Services;
using WhatsAppBot.Services.Interfaces;
using WhatsAppBot.Services;
using WhatsAppBot.Data;
using Microsoft.EntityFrameworkCore;
using WhatsAppBot.Hubs;
using WhatsAppBot.Data.Repositories.Interfaces;
using WhatsAppBot.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//Agregar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddFilter((category, level) =>
{
    if (category == "Microsoft.EntityFrameworkCore.Database.Command" && 
        level == LogLevel.Information)
    {
        return false; // No registrar estos logs
    }
    return true;
});

// Agregar servicios
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddRazorPages();

builder.Services.AddSignalR();

builder.Services.AddMudServices();
builder.Services.AddHttpClient();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "WhatsAppBot_";
});

builder.Services.AddDbContext<WhatsAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Inyectar servicios propios
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IConversacionService, ConversacionService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IHistoryMessageService, HistoryMessageService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IMensajeRepository, MensajeRepository>();
builder.Services.AddScoped<IEstadoConversacionRepository, EstadoConversacionRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddControllers();

// Configuración de Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<WhatsAppDbContext>()
.AddDefaultTokenProviders();

// Política de autorización para el panel admin
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Administrator"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// Middleware de autenticación
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapHub<PedidosHub>("/pedidoshub");
app.MapControllers(); // Necesario para el Webhook


app.Run();
