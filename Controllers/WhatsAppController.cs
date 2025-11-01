using Microsoft.AspNetCore.Mvc;
using WhatsAppBot.Models;
using WhatsAppBot.Services.Interfaces;
using WhatsAppBot.Data.DTOs;
using System.Threading.Tasks;

namespace WhatsAppBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WhatsAppController : ControllerBase
    {
        private readonly IWhatsAppService _whatsAppService;
        private readonly IWebhookService _webhookService;

        public WhatsAppController(IWhatsAppService whatsAppService, IWebhookService webhookService)
        {
            _whatsAppService = whatsAppService;
            _webhookService = webhookService;
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
                return BadRequest(new SendMessageResultDto { Success = false, Message = aex.Message });
            }
            catch (System.Exception ex)
            {
                // Loguear la excepción (el servicio ya lo hace), devolver ProblemDetails
                var pd = new ProblemDetails
                {
                    Title = "Error al enviar mensaje",
                    Detail = ex.Message,
                    Status = 500
                };
                return StatusCode(500, pd);
            }
        }

        // Webhook de verificación. Mantengo la lógica actual (query params).
        [HttpGet("webhook")]
        public IActionResult VerifyWebhook(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.challenge")] string challenge,
            [FromQuery(Name = "hub.verify_token")] string verifyToken)
        {
            const string VERIFY_TOKEN = "qwerty"; // Reemplaza por secreto en producción
            if (mode == "subscribe" && verifyToken == VERIFY_TOKEN)
            {
                return Ok(challenge);
            }
            return BadRequest();
        }

        // Webhook POST: recibe payload de Meta. Se delega todo al servicio de webhook.
        [HttpPost("webhook")]
        public async Task<IActionResult> ReceiveMessage([FromBody] WebhookPayload payload)
        {
            if (payload == null)
                return BadRequest();

            try
            {
                await _webhookService.ProcessIncomingMessageAsync(payload);
                return Ok();
            }
            catch (System.Exception ex)
            {
                var pd = new ProblemDetails { Title = "Error procesando webhook", Detail = ex.Message, Status = 500 };
                return StatusCode(500, pd);
            }
        }
    }
}
