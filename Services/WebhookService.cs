using Microsoft.EntityFrameworkCore;
using WhatsAppBot.Data;
using WhatsAppBot.Models;
using WhatsAppBot.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace WhatsAppBot.Services;

public class WebhookService : IWebhookService
{
    private readonly IWhatsAppService _whatsAppService;
    private readonly IHistoryMessageService _historyMessageService;
    private readonly IConversacionService _conversacionService;
    private readonly IClienteService _clienteService;
    private readonly IPedidoService _pedidoService;
    private readonly IMessageService _messageService;
    private readonly ILogger<WebhookService> _logger;
    private readonly IDistributedCache _cache;

    public WebhookService(
        IWhatsAppService whatsAppService,
        IHistoryMessageService historyMessageService,
        IConversacionService conversacionService,
        IClienteService clienteService,
        IPedidoService pedidoService,
        IMessageService messageService,
        ILogger<WebhookService> logger,
        IDistributedCache cache)
    {
        _whatsAppService = whatsAppService;
        _historyMessageService = historyMessageService;
        _conversacionService = conversacionService;
        _clienteService = clienteService;
        _pedidoService = pedidoService;
        _messageService = messageService;
        _logger = logger;
        _cache = cache;
    }

    public async Task ProcessIncomingMessageAsync(WebhookPayload payload)
    {
        try
        {
            _logger.LogInformation("Procesando mensaje entrante del webhook.");
            // Extraer el número y texto
            var entry = payload.Entry.FirstOrDefault();
            var change = entry?.Changes?.FirstOrDefault();
            var message = change?.Value?.Messages?.FirstOrDefault();

            _logger.LogInformation("Verificamos que payload contenga un mensaje o un numero y que sea tipo texto o interactivo.");
            if (message == null || string.IsNullOrEmpty(message.From))
                return;

            // Verificar duplicados
            if (await EsMensajeDuplicado(message.Id))
            {
                _logger.LogInformation($"Mensaje duplicado detectado, ID: {message.Id}");
                return;
            }
            
            if (message.Type != "text" && message.Type != "interactive")
                return;

            string texto = message.Text?.Body?.Trim()
                ?? message.Interactive?.Button_Reply?.Title
                ?? message.Interactive?.List_Reply?.Title
                ?? "";

            string telefono;
            try
            {
                telefono = NormalizarTelefono(message.From);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Número de teléfono inválido: {message.From}");
                return;
            }

            _logger.LogInformation($"📩 Mensaje recibido de {telefono}: {texto}");

            // Registrar el mensaje recibido
            _logger.LogInformation("Guardando mensaje entrante en el historial.");
            await _historyMessageService.GuardarMensajeEnHistorial(new MensajeWhatsApp
            {
                Telefono = telefono,
                MensajeTexto = texto,
                DireccionConversacion = "entrada"
            });

            // Obtener estado de conversación o iniciarla
            var estado = await _conversacionService.ObtenerOIniciarAsync(telefono);
            if (estado == null)
            {
                _logger.LogWarning("Estado de conversación nulo, iniciando estado por defecto.");
                await _conversacionService.ActualizarEstadoAsync(telefono, "Inicio");
                estado = await _conversacionService.ObtenerOIniciarAsync(telefono);
                if (estado == null)
                {
                    _logger.LogError("No se pudo iniciar estado de conversación.");
                    return;
                }
            }

            await ProcesarEstado(telefono, texto, estado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar el mensaje entrante.");
        }
    }
    private async Task ProcesarEstado(string telefono, string texto, EstadoConversacion estado)
    {
        string mensajeRespuesta;
        switch (estado.EstadoActual)
        {
            case "Inicio":
                await EnviarBienvenida(telefono);
                await _conversacionService.ActualizarEstadoAsync(telefono, "MenuPrincipal");
                break;

            case "Finalizado":
                await EnviarBienvenida(telefono);
                await _conversacionService.ActualizarEstadoAsync(telefono, "MenuPrincipal");
                break;

            case "MenuPrincipal":
                if (texto.Contains("pedido", StringComparison.OrdinalIgnoreCase))
                {
                    var cliente = await _clienteService.ObtenerPorNumeroAsync(telefono);
                    if (cliente != null)
                    {
                        mensajeRespuesta = $"Perfecto {cliente.Nombre}, ¿cuál será tu pedido?\nPuedes escribir varios productos en un solo mensaje.";
                        await EnviarYRegistrarMensaje(telefono, mensajeRespuesta, false);
                        await _conversacionService.ActualizarEstadoAsync(telefono, "SolicitandoPedido");
                    }
                    else
                    {
                        _logger.LogInformation("Cliente no encontrado al solicitar pedido, solicitando nombre.");
                        mensajeRespuesta = "Antes de hacer tu pedido, necesito tu nombre completo 🧾";
                        await EnviarYRegistrarMensaje(telefono, mensajeRespuesta, false);
                        await _conversacionService.ActualizarEstadoAsync(telefono, "RegistroNombre");
                    }
                }
                else
                {
                    mensajeRespuesta = "Lo siento no entendi tu respuesta.";
                    await EnviarYRegistrarMensaje(telefono, mensajeRespuesta, false);
                    await MostrarMenuPrincipal(telefono);
                }
                break;

            case "RegistroNombre":
                var nuevoCliente = new Cliente { Telefono = telefono, Nombre = texto };
                await _clienteService.CrearAsync(nuevoCliente);
                mensajeRespuesta = $"👍 ¡Gracias {texto}!\n Ahora, por favor escribe tu dirección completa (calle, número, colonia, ciudad).";
                await EnviarYRegistrarMensaje(telefono, mensajeRespuesta, false);
                await _conversacionService.ActualizarEstadoAsync(telefono, "RegistroDireccion");
                break;

            case "RegistroDireccion":
                var clienteExistente = await _clienteService.ObtenerPorNumeroAsync(telefono);
                if (clienteExistente == null)
                {
                    _logger.LogWarning("RegistroDireccion: no existe cliente para el teléfono {telefono}. Solicitando nombre.", telefono);
                    mensajeRespuesta = "No pude encontrar tu registro. Por favor dime tu nombre completo.";
                    await EnviarYRegistrarMensaje(telefono, mensajeRespuesta, false);
                    await _conversacionService.ActualizarEstadoAsync(telefono, "RegistroNombre");
                    return;
                }

                clienteExistente.Direccion = texto;
                await _clienteService.ActualizarAsync(clienteExistente);
                mensajeRespuesta = $"Perfecto {clienteExistente.Nombre}. Ahora, ¿cuál será tu pedido?";
                await _conversacionService.ActualizarEstadoAsync(telefono, "SolicitandoPedido");
                break;

            case "SolicitandoPedido":
                var clientePedido = await _clienteService.ObtenerPorNumeroAsync(telefono);
                if (clientePedido == null)
                {
                    _logger.LogWarning("SolicitandoPedido: no existe cliente para {telefono}. Solicitando nombre.", telefono);
                    mensajeRespuesta = "No encuentro tu registro. Por favor envía tu nombre completo para crear tu cuenta.";
                    await EnviarYRegistrarMensaje(telefono, mensajeRespuesta, false);
                    await _conversacionService.ActualizarEstadoAsync(telefono, "RegistroNombre");
                    return;
                }

                var pedido = new Pedido
                {
                    ClienteId = clientePedido.ClienteId,
                    DetallePedido = texto,
                    DireccionEntrega = clientePedido.Direccion,
                    Estado = "En espera"
                };
                await _pedidoService.AgregarPedidoAsync(pedido);

                await EnviarBotonesFormaPago(telefono);
                await _conversacionService.ActualizarEstadoAsync(telefono, "FormaPago");
                break;

            case "FormaPago":
                var pedidoActual = await _pedidoService.ObtenerUltimoPedidoAsync(telefono);
                if (pedidoActual == null)
                {
                    _logger.LogWarning("FormaPago: no existe pedido para {telefono}. Informando al usuario.", telefono);
                    var sinPedido = new WhatsAppMessage
                    {
                        To = telefono,
                        Body = "No tengo un pedido asociado. Por favor inicia un nuevo pedido escribiendo \"pedido\"."
                    };
                    await _whatsAppService.SendMessageAsync(sinPedido);
                    await _historyMessageService.GuardarMensajeEnHistorial(new MensajeWhatsApp
                    {
                        Telefono = telefono,
                        MensajeTexto = sinPedido.Body,
                        DireccionConversacion = "salida"
                    });
                    await _conversacionService.ActualizarEstadoAsync(telefono, "MenuPrincipal");
                    return;
                }

                pedidoActual.FormaPago = texto.Contains("tarjeta", StringComparison.OrdinalIgnoreCase) ? "Tarjeta" : "Efectivo";
                await _pedidoService.ActualizarAsync(pedidoActual);
                await EnviarBotonesConfirmarDireccion(telefono, pedidoActual.DireccionEntrega ?? "");
                await _conversacionService.ActualizarEstadoAsync(telefono, "ConfirmarDireccion");
                break;

            case "ConfirmarDireccion":
                var pedidoConf = await _pedidoService.ObtenerUltimoPedidoAsync(telefono);
                if (pedidoConf == null)
                {
                    _logger.LogWarning("ConfirmarDireccion: no existe pedido para {telefono}.", telefono);
                    mensajeRespuesta = "No pude encontrar tu pedido. Por favor inicia un nuevo pedido con la opción \"Hacer pedido\".";
                    await EnviarYRegistrarMensaje(telefono, mensajeRespuesta, false);
                    await _conversacionService.ActualizarEstadoAsync(telefono, "MenuPrincipal");
                    return;
                }

                if (texto.Contains("confirmar", StringComparison.OrdinalIgnoreCase))
                {
                    pedidoConf.Folio = _pedidoService.GenerarFolio();
                    pedidoConf.Estado = "En espera";
                    await _pedidoService.ActualizarAsync(pedidoConf);
                    mensajeRespuesta = $"✅ Gracias por tu pedido, {pedidoConf.Cliente?.Nombre}.\nTu folio es *{pedidoConf.Folio}*.\nTu pedido está en espera de ser surtido.";
                    await EnviarYRegistrarMensaje(telefono, mensajeRespuesta, false);
                    await _conversacionService.ActualizarEstadoAsync(telefono, "Finalizado");
                }
                else
                {
                    mensajeRespuesta = "Entendido, vamos a corregir la dirección.\n Por favor escribe tu dirección completa (calle, número, colonia, ciudad).";
                    await EnviarYRegistrarMensaje(telefono, mensajeRespuesta, false);
                    await _conversacionService.ActualizarEstadoAsync(telefono, "ActualizarDireccion");
                }
                break;

            case "ActualizarDireccion":
                var clienteActual = await _clienteService.ObtenerPorNumeroAsync(telefono);
                if (clienteActual == null)
                {
                    _logger.LogWarning("ActualizarDireccion: no existe cliente para {telefono}.", telefono);
                    mensajeRespuesta = "No pude encontrar tu registro. Por favor dime tu nombre completo.";
                    await EnviarYRegistrarMensaje(telefono, mensajeRespuesta, false);
                    await _conversacionService.ActualizarEstadoAsync(telefono, "RegistroNombre");
                    return;
                }

                clienteActual.Direccion = texto;
                await _clienteService.ActualizarAsync(clienteActual);

                var pedidoConfi = await _pedidoService.ObtenerUltimoPedidoAsync(telefono);

                if (pedidoConfi == null)
                {
                    _logger.LogWarning("ActualizarDireccion: no existe pedido para {telefono} al actualizar dirección.", telefono);
                    mensajeRespuesta = "No encontré el pedido asociado. Por favor inicia un nuevo pedido.";
                    await EnviarYRegistrarMensaje(telefono, mensajeRespuesta, false);
                    await _conversacionService.ActualizarEstadoAsync(telefono, "MenuPrincipal");
                    return;
                }
                
                pedidoConfi.Folio = _pedidoService.GenerarFolio();
                pedidoConfi.Estado = "En espera";
                await _pedidoService.ActualizarAsync(pedidoConfi);
                mensajeRespuesta = $"✅ Gracias por tu pedido, {pedidoConfi.Cliente?.Nombre}.\nTu folio es *{pedidoConfi.Folio}*.\nTu pedido está en espera de ser surtido.";
                await EnviarYRegistrarMensaje(telefono, mensajeRespuesta, false);
                await _conversacionService.ActualizarEstadoAsync(telefono, "Finalizado");
                break;
        }
    }
    private async Task EnviarBienvenida(string telefono)
    {
        var bienvenidaMensaje = _messageService.CrearMensajeBienvenida(telefono);
        await _whatsAppService.SendMessageAsync(bienvenidaMensaje);
        await _historyMessageService.GuardarMensajeEnHistorial(new MensajeWhatsApp
        {
            Telefono = telefono,
            MensajeTexto = bienvenidaMensaje.Body,
            DireccionConversacion = "salida"
        });

        await MostrarMenuPrincipal(telefono);
    }
    private async Task MostrarMenuPrincipal(string telefono)
    {
        await _whatsAppService.SendInteractiveMessageAsync(
            telefono,
            "¿Como te puedo ayudar?",
            new[] { "Hacer pedido", "Información" }
            );
    }
    private async Task EnviarBotonesFormaPago(string telefono)
    {
        await _whatsAppService.SendInteractiveMessageAsync(
            telefono,
            "¿Cuál será la forma de pago?",
            new[] { "💵 Efectivo", "💳 Tarjeta" }
        );
    }

    private async Task EnviarBotonesConfirmarDireccion(string telefono, string direccion)
    {
        await _whatsAppService.SendInteractiveMessageAsync(
            telefono,
            $"Por favor confirma tu dirección:\n📍 {direccion}",
            new[] { "✅ Confirmar", "✏️ Corregir" }
        );
    }
    private async Task EnviarYRegistrarMensaje(string telefono, string mensaje, bool esInteractivo = false, string[]? botones = null)
    {
        if (esInteractivo && botones != null)
        {
            await _whatsAppService.SendInteractiveMessageAsync(telefono, mensaje, botones);
        }
        else
        {
            await _whatsAppService.SendMessageAsync(new WhatsAppMessage
            {
                To = telefono,
                Body = mensaje
            });
        }

        await _historyMessageService.GuardarMensajeEnHistorial(new MensajeWhatsApp
        {
            Telefono = telefono,
            MensajeTexto = mensaje,
            DireccionConversacion = "salida"
        });
    }
    private string NormalizarTelefono(string telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono))
            throw new ArgumentException("El número de teléfono no puede estar vacío");

        // Eliminar todos los caracteres no numéricos
        var numerosLimpios = new string(telefono.Where(char.IsDigit).ToArray());

        // Validar longitud (asumiendo formato mexicano: 12 dígitos incluyendo 52)
        if (numerosLimpios.Length < 10)
            throw new ArgumentException($"Longitud de teléfono inválida: {numerosLimpios.Length} dígitos");

        // Si comienza con 521, remover el 1
        if (numerosLimpios.StartsWith("521"))
        {
            numerosLimpios = "52" + numerosLimpios.Substring(3);
        }
        // Si no comienza con 52, añadirlo
        else if (!numerosLimpios.StartsWith("52"))
        {
            numerosLimpios = "52" + numerosLimpios;
        }

        // Validar longitud final (debe ser 12 dígitos: 52 + 10 dígitos)
        if (numerosLimpios.Length != 12)
            throw new ArgumentException($"Longitud de teléfono final inválida: {numerosLimpios.Length} dígitos");

        return numerosLimpios;
    }
    private async Task<bool> EsMensajeDuplicado(string messageId)
    {
        if (string.IsNullOrEmpty(messageId))
            return false;

        var cacheKey = $"msg_{messageId}";
        var existente = await _cache.GetStringAsync(cacheKey);
        
        if (existente != null)
            return true;

        // Guardar el ID por 24 horas para prevenir duplicados
        await _cache.SetStringAsync(
            cacheKey,
            "1",
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            });

        return false;
    }
}
