﻿using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WhatsAppBot.Models;
using WhatsAppBot.Services.Interfaces;
using Polly;
using Polly.Retry;

namespace WhatsAppBot.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly AsyncRetryPolicy<bool> _retryPolicy;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<WhatsAppService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;

            _retryPolicy = Policy<bool>
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    3, // Número de reintentos
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Espera exponencial
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            "Error al enviar mensaje a WhatsApp. Reintento {RetryCount} después de {TimeSpan}s.",
                            retryCount,
                            timeSpan.TotalSeconds,
                            exception);
                    }
                );
        }

        public async Task<bool> SendMessageAsync(WhatsAppMessage message)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    string? token = _config["WhatsApp:Token"];
                    string? phoneNumberId = _config["WhatsApp:PhoneNumberId"];

                    if (string.IsNullOrWhiteSpace(token))
                        throw new InvalidOperationException("El token de WhatsApp no está configurado. Revisa appsettings.json");

                    if (string.IsNullOrWhiteSpace(phoneNumberId))
                        throw new InvalidOperationException("El PhoneNumberId de WhatsApp no está configurado. Revisa appsettings.json");

                    var url = $"https://graph.facebook.com/v22.0/{phoneNumberId}/messages";

                    var http = _httpClientFactory.CreateClient();
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var payload = new
                    {
                        messaging_product = "whatsapp",
                        to = message.To,
                        type = "text",
                        text = new { body = message.Body }
                    };

                    var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                    Console.WriteLine($"📤 Enviando mensaje a: {url}");

                    var response = await http.PostAsync(url, content);

                    var responseText = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"📥 Respuesta de Meta: {response.StatusCode} - {responseText}");

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar mensaje de WhatsApp");
                    throw; //Relanzamos la excepcion para que la maneje la politica de reintentos
                }
            });
        }

        public async Task<bool> SendInteractiveMessageAsync(string to, string body, string[] buttons)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    string? token = _config["WhatsApp:Token"];
                    string? phoneNumberId = _config["WhatsApp:PhoneNumberId"];

                    if (string.IsNullOrWhiteSpace(token))
                        throw new InvalidOperationException("El token de WhatsApp no está configurado. Revisa appsettings.json");

                    if (string.IsNullOrWhiteSpace(phoneNumberId))
                        throw new InvalidOperationException("El PhoneNumberId de WhatsApp no está configurado. Revisa appsettings.json");

                    var url = $"https://graph.facebook.com/v22.0/{phoneNumberId}/messages";

                    var http = _httpClientFactory.CreateClient();
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var payload = new
                    {
                        messaging_product = "whatsapp",
                        to = to,
                        type = "interactive",
                        interactive = new
                        {
                            type = "button",
                            body = new { text = body },
                            action = new
                            {
                                buttons = buttons.Select((b, i) => new
                                {
                                    type = "reply",
                                    reply = new { id = $"btn_{i + 1}", title = b }
                                })
                            }
                        }
                    };

                    var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                    Console.WriteLine($"📤 Enviando mensaje interactivo a: {url}");

                    var response = await http.PostAsync(url, content);
                    var responseText = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"📥 Respuesta de Meta (interactivo): {response.StatusCode} - {responseText}");

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar mensaje interactivo de WhatsApp");
                    throw; //Relanzamos la excepcion para que la maneje la politica de reintentos
                }
            });
            
        }
    }
}
