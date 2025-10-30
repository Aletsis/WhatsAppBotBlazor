using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WhatsAppBot.Models;
using WhatsAppBot.Services.Interfaces;

namespace WhatsAppBot.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public WhatsAppService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<bool> SendMessageAsync(WhatsAppMessage message)
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

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SendInteractiveMessageAsync(string to, string body, string[] buttons)
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

            return response.IsSuccessStatusCode;
        }
    }
}
