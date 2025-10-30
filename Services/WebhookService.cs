using Microsoft.EntityFrameworkCore;
using WhatsAppBot.Data;
using WhatsAppBot.Models;
using WhatsAppBot.Services.Interfaces;

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

    public WebhookService(
        IWhatsAppService whatsAppService,
        IHistoryMessageService historyMessageService,
        IConversacionService conversacionService,
        IClienteService clienteService,
        IPedidoService pedidoService,
        IMessageService messageService,
        ILogger<WebhookService> logger)
    {
        _whatsAppService = whatsAppService;
        _historyMessageService = historyMessageService;
        _conversacionService = conversacionService;
        _clienteService = clienteService;
        _pedidoService = pedidoService;
        _messageService = messageService;
        _logger = logger;
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
            if (message.Type != "text" && message.Type != "interactive")
                return;
            
            string telefono = message.From;
            string texto = message.Text?.Body?.Trim()
                ?? message.Interactive?.Button_Reply?.Title
                ?? message.Interactive?.List_Reply?.Title
                ?? "";
            string transformed = telefono.Remove(2, 1);
            telefono = transformed;
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
                        var mensageRespuesta = new WhatsAppMessage
                        {
                            To = telefono,
                            Body = $"Perfecto {cliente.Nombre}, ¿cuál será tu pedido?\nPuedes escribir varios productos en un solo mensaje."
                        };

                        await _whatsAppService.SendMessageAsync(mensageRespuesta);
                        await _conversacionService.ActualizarEstadoAsync(telefono, "SolicitandoPedido");
                        // Registrar el mensaje enviado
                        await _historyMessageService.GuardarMensajeEnHistorial(new MensajeWhatsApp
                        {
                            Telefono = telefono,
                            MensajeTexto = mensageRespuesta.Body,
                            DireccionConversacion = "salida"
                        });
                    }
                    else
                    {
                        _logger.LogInformation("Cliente no encontrado al solicitar pedido, solicitando nombre.");
                        var mensageRespuesta2 = new WhatsAppMessage
                        {
                            To = telefono,
                            Body = "Antes de hacer tu pedido, necesito tu nombre completo 🧾"
                        };

                        await _whatsAppService.SendMessageAsync(mensageRespuesta2);

                        await _conversacionService.ActualizarEstadoAsync(telefono, "RegistroNombre");
                        await _historyMessageService.GuardarMensajeEnHistorial(new MensajeWhatsApp
                        {
                            Telefono = telefono,
                            MensajeTexto = mensageRespuesta2.Body,
                            DireccionConversacion = "salida"
                        });
                    }
                }
                else
                {
                    var mensageRespuesta3 = new WhatsAppMessage
                    {
                        To = telefono,
                        Body = "Lo siento no entendi tu respuesta."
                    };
                    await _whatsAppService.SendMessageAsync(mensageRespuesta3);
                    await _historyMessageService.GuardarMensajeEnHistorial(new MensajeWhatsApp
                    {
                        Telefono = telefono,
                        MensajeTexto = mensageRespuesta3.Body,
                        DireccionConversacion = "salida"
                    });
                    await MostrarMenuPrincipal(telefono);
                }
                break;

            case "RegistroNombre":
                var nuevoCliente = new Cliente { Telefono = telefono, Nombre = texto };
                await _clienteService.CrearAsync(nuevoCliente);
                var mensageRespuesta4 = new WhatsAppMessage
                {
                    To = telefono,
                    Body = "Gracias 👍 Ahora, por favor escribe tu dirección completa (calle, número, colonia, ciudad)."
                };
                await _whatsAppService.SendMessageAsync(mensageRespuesta4);
                await _conversacionService.ActualizarEstadoAsync(telefono, "RegistroDireccion");
                await _historyMessageService.GuardarMensajeEnHistorial(new MensajeWhatsApp
                {
                    Telefono = telefono,
                    MensajeTexto = mensageRespuesta4.Body,
                    DireccionConversacion = "salida"
                });
                break;

            case "RegistroDireccion":
                var clienteExistente = await _clienteService.ObtenerPorNumeroAsync(telefono);
                if (clienteExistente == null)
                {
                    _logger.LogWarning("RegistroDireccion: no existe cliente para el teléfono {telefono}. Solicitando nombre.", telefono);
                    var pedirNombre = new WhatsAppMessage
                    {
                        To = telefono,
                        Body = "No pude encontrar tu registro. Por favor dime tu nombre completo."
                    };
                    await _whatsAppService.SendMessageAsync(pedirNombre);
                    await _conversacionService.ActualizarEstadoAsync(telefono, "RegistroNombre");
                    await _historyMessageService.GuardarMensajeEnHistorial(new MensajeWhatsApp
                    {
                        Telefono = telefono,
                        MensajeTexto = pedirNombre.Body,
                        DireccionConversacion = "salida"
                    });
                    return;
                }

                clienteExistente.Direccion = texto;
                await _clienteService.ActualizarAsync(clienteExistente);
                var mensageRespuesta5 = new WhatsAppMessage
                {
                    To = telefono,
                    Body = $"Perfecto {clienteExistente.Nombre}. Ahora, ¿cuál será tu pedido?"
                };
                await _whatsAppService.SendMessageAsync(mensageRespuesta5);
                await _conversacionService.ActualizarEstadoAsync(telefono, "SolicitandoPedido");
                await _historyMessageService.GuardarMensajeEnHistorial(new MensajeWhatsApp
                {
                    Telefono = telefono,
                    MensajeTexto = mensageRespuesta5.Body,
                    DireccionConversacion = "salida"
                });
                break;

            case "SolicitandoPedido":
                var clientePedido = await _clienteService.ObtenerPorNumeroAsync(telefono);
                if (clientePedido == null)
                {
                    _logger.LogWarning("SolicitandoPedido: no existe cliente para {telefono}. Solicitando nombre.", telefono);
                    var pedirNombre2 = new WhatsAppMessage
                    {
                        To = telefono,
                        Body = "No encuentro tu registro. Por favor envía tu nombre completo para crear tu cuenta."
                    };
                    await _whatsAppService.SendMessageAsync(pedirNombre2);
                    await _conversacionService.ActualizarEstadoAsync(telefono, "RegistroNombre");
                    await _historyMessageService.GuardarMensajeEnHistorial(new MensajeWhatsApp
                    {
                        Telefono = telefono,
                        MensajeTexto = pedirNombre2.Body,
                        DireccionConversacion = "salida"
                    });
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
                    var sinPedido2 = new WhatsAppMessage
                    {
                        To = telefono,
                        Body = "No pude encontrar tu pedido. Por favor inicia un nuevo pedido con la opción \"Hacer pedido\"."
                    };
                    await _whatsAppService.SendMessageAsync(sinPedido2);
                    await _historyMessageService.GuardarMensajeEnHistorial(new MensajeWhatsApp
                    {
                        Telefono = telefono,
                        MensajeTexto = sinPedido2.Body,
                        DireccionConversacion = "salida"
                    });
                    await _conversacionService.ActualizarEstadoAsync(telefono, "MenuPrincipal");
                    return;
                }

                if (texto.Contains("confirmar", StringComparison.OrdinalIgnoreCase))
                {
                    pedidoConf.Folio = _pedidoService.GenerarFolio();
                    pedidoConf.Estado = "En espera";
                    await _pedidoService.ActualizarAsync(pedidoConf);

                    var mensajeRespuesta6 = new WhatsAppMessage
                    {
                        To = telefono,
                        Body = $"✅ Gracias por tu pedido, {pedidoConf.Cliente?.Nombre}.\nTu folio es *{pedidoConf.Folio}*.\nTu pedido está en espera de ser surtido."
                    };
                    await _whatsAppService.SendMessageAsync(mensajeRespuesta6);
                    await _historyMessageService.GuardarMensajeEnHistorial(new MensajeWhatsApp
                    {
                        Telefono = telefono,
                        MensajeTexto = mensajeRespuesta6.Body,
                        DireccionConversacion = "salida"
                    });
                    await _conversacionService.ActualizarEstadoAsync(telefono, "Finalizado");
                }
                else
                {
                    var mensajeRespuesta8 = new WhatsAppMessage
                    {
                        To = telefono,
                        Body = "Por favor envíame la dirección corregida:"
                    };
                    await _whatsAppService.SendMessageAsync(mensajeRespuesta8);
                    await _historyMessageService.GuardarMensajeEnHistorial(new MensajeWhatsApp
                    {
                        Telefono = telefono,
                        MensajeTexto = mensajeRespuesta8.Body,
                        DireccionConversacion = "salida"
                    });
                    await _conversacionService.ActualizarEstadoAsync(telefono, "ActualizarDireccion");
                }
                break;

            case "ActualizarDireccion":
                var clienteActual = await _clienteService.ObtenerPorNumeroAsync(telefono);
                if (clienteActual == null)
                {
                    _logger.LogWarning("ActualizarDireccion: no existe cliente para {telefono}.", telefono);
                    var pedirNombre3 = new WhatsAppMessage
                    {
                        To = telefono,
                        Body = "No pude encontrar tu registro. Por favor envía tu nombre completo para crear tu cuenta."
                    };
                    await _whatsAppService.SendMessageAsync(pedirNombre3);
                    await _conversacionService.ActualizarEstadoAsync(telefono, "RegistroNombre");
                    await _historyMessageService.GuardarMensajeEnHistorial(new MensajeWhatsApp
                    {
                        Telefono = telefono,
                        MensajeTexto = pedirNombre3.Body,
                        DireccionConversacion = "salida"
                    });
                    return;
                }

                clienteActual.Direccion = texto;
                await _clienteService.ActualizarAsync(clienteActual);

                var pedidoConfi = await _pedidoService.ObtenerUltimoPedidoAsync(telefono);

                if (pedidoConfi == null)
                {
                    _logger.LogWarning("ActualizarDireccion: no existe pedido para {telefono} al actualizar dirección.", telefono);
                    var sinPedido3 = new WhatsAppMessage
                    {
                        To = telefono,
                        Body = "No encontré el pedido asociado. Por favor inicia un nuevo pedido."
                    };
                    await _whatsAppService.SendMessageAsync(sinPedido3);
                    await _historyMessageService.GuardarMensajeEnHistorial(new MensajeWhatsApp
                    {
                        Telefono = telefono,
                        MensajeTexto = sinPedido3.Body,
                        DireccionConversacion = "salida"
                    });
                    await _conversacionService.ActualizarEstadoAsync(telefono, "MenuPrincipal");
                    return;
                }
                
                pedidoConfi.Folio = _pedidoService.GenerarFolio();
                pedidoConfi.Estado = "En espera";
                await _pedidoService.ActualizarAsync(pedidoConfi);
                var mensajeRespuesta7 = new WhatsAppMessage
                {
                    To = telefono,
                    Body = $"✅ Gracias por tu pedido, {pedidoConfi.Cliente?.Nombre}.\nTu folio es *{pedidoConfi.Folio}*.\nTu pedido está en espera de ser surtido."
                };
                await _whatsAppService.SendMessageAsync(mensajeRespuesta7);
                    await _conversacionService.ActualizarEstadoAsync(telefono, "Finalizado");
                await _historyMessageService.GuardarMensajeEnHistorial(new MensajeWhatsApp
                {
                    Telefono = telefono,
                    MensajeTexto = mensajeRespuesta7.Body,
                    DireccionConversacion = "salida"
                });
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
}
