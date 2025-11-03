using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WhatsAppBot.Models;
using WhatsAppBot.Services.Interfaces;
using WhatsAppBot.Data.DTOs;
using WhatsAppBot.Configuration;
using System.Threading.Tasks;

namespace WhatsAppBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WhatsAppController : ControllerBase
    {
        private readonly IWhatsAppService _whatsAppService;
        private readonly IWebhookService _webhookService;
        private readonly WhatsAppSettings _whatsAppSettings;
        private readonly SecuritySettings _securitySettings;
        private readonly ILogger<WhatsAppController> _logger;

        public WhatsAppController(
            IWhatsAppService whatsAppService,
            IWebhookService webhookService,
            IOptions<WhatsAppSettings> whatsAppSettings,
            IOptions<SecuritySettings> securitySettings,
            ILogger<WhatsAppController> logger)
        {
            _whatsAppService = whatsAppService;
            _webhookService = webhookService;
            _whatsAppSettings = whatsAppSettings.Value;
            _securitySettings = securitySettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Enviar mensaje simple de texto por WhatsApp.
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDTO dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var message = new WhatsAppMessage
                {
                    To = dto.To,
                    Body = dto.Body
                };

                var success = await _whatsAppService.SendMessageAsync(message);

                var result = new SendMessageResultDto
                {
                    Success = success,
                    Message = success ? "Mensaje enviado" : "Error al enviar mensaje"
                };

                if (success)
                    return Ok(result);

                // Si SendMessageAsync devuelve false en tu implementación, considera más info
                return StatusCode(500, result);
            }
            catch (ArgumentException aex)
            {
                // errores de validación manual específicos
                _logger.LogWarning(aex, "Error de validación al enviar mensaje");
                return BadRequest(new SendMessageResultDto { Success = false, Message = aex.Message });
            }
            catch (System.Exception ex)
            {
                // Loguear la excepción (el servicio ya lo hace), devolver ProblemDetails
                _logger.LogError(ex, "Error al enviar mensaje");
                var pd = new ProblemDetails
                {
                    Title = "Error al enviar mensaje",
                    Detail = ex.Message,
                    Status = 500
                };
                return StatusCode(500, pd);
            }
        }

        /// <summary>
        /// Webhook de verificación para Meta/Facebook.
        /// </summary>
        [HttpGet("webhook")]
        public IActionResult VerifyWebhook(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.challenge")] string challenge,
            [FromQuery(Name = "hub.verify_token")] string verifyToken)
        {
            // ✅ Usar token de configuración segura en lugar de constante hardcodeada
            if (mode == "subscribe" && verifyToken == _whatsAppSettings.VerifyToken)
            {
                _logger.LogInformation("✅ Webhook verificado correctamente");
                return Ok(challenge);
            }
            
            _logger.LogWarning("❌ Intento de verificación de webhook fallido. Token inválido.");
            return Unauthorized();
        }

        /// <summary>
        /// Webhook POST: recibe mensajes de WhatsApp desde Meta.
        /// </summary>
        [HttpPost("webhook")]
        public async Task<IActionResult> ReceiveMessage([FromBody] WebhookPayload payload)
        {
            if (payload == null)
            {
                _logger.LogWarning("Payload de webhook nulo");
                return BadRequest("Payload inválido");
            }

            // ✅ TODO: Implementar validación de firma HMAC
            // La validación de firma garantiza que el webhook viene de Meta
            /*
            var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
            if (!ValidateWebhookSignature(signature, payloadRaw, _securitySettings.AppSecret))
            {
                _logger.LogWarning("⚠️ Firma de webhook inválida - posible intento de suplantación");
                return Unauthorized("Firma inválida");
            }
            */

            try
            {
                await _webhookService.ProcessIncomingMessageAsync(payload);
                return Ok();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error procesando webhook");
                var pd = new ProblemDetails 
                { 
                    Title = "Error procesando webhook", 
                    Detail = ex.Message, 
                    Status = 500 
                };
                return StatusCode(500, pd);
            }
        }

        // ✅ TODO: Implementar validación de firma
        // Método auxiliar para validar la firma HMAC del webhook
        /*
        private bool ValidateWebhookSignature(string? signature, string payload, string appSecret)
        {
            if (string.IsNullOrEmpty(signature))
                return false;

            try
            {
                var expectedSignature = ComputeHmacSha256(payload, appSecret);
                return signature.Equals($"sha256={expectedSignature}", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando firma de webhook");
                return false;
            }
        }

        private string ComputeHmacSha256(string data, string secret)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
        */
    }
}
