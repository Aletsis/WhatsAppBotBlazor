using WhatsAppBot.Components;
using MudBlazor.Services;
using WhatsAppBot.Services.Interfaces;
using WhatsAppBot.Services;
using WhatsAppBot.Data;
using Microsoft.EntityFrameworkCore;
using WhatsAppBot.Hubs;
using WhatsAppBot.Data.Repositories.Interfaces;
using WhatsAppBot.Data.Repositories;


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
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddControllers();

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapHub<PedidosHub>("/pedidoshub");
app.MapControllers(); // Necesario para el Webhook


app.Run();
