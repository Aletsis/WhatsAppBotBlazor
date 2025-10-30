using Microsoft.AspNetCore.Mvc;
using WhatsAppBot.Models;
using WhatsAppBot.Services.Interfaces;

namespace WhatsAppBot.Controllers;

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

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] WhatsAppMessage message)
    {
        var success = await _whatsAppService.SendMessageAsync(message);
        return success ? Ok("Mensaje enviado") : StatusCode(500, "Error al enviar mensaje");
    }

    // Webhook de Meta
    [HttpGet("webhook")]
    public IActionResult VerifyWebhook(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.challenge")] string challenge,
        [FromQuery(Name = "hub.verify_token")] string verifyToken)
    {
        const string VERIFY_TOKEN = "qwerty";//Modificar el token de acceso
        if (mode == "subscribe" && verifyToken == VERIFY_TOKEN)
        {
            Console.WriteLine("Webhook verificado correctamente por Meta.");
            return Ok(challenge);
        }
        Console.WriteLine("Error: Token de verificacion incorrecto o modo incorrecto.");
        return BadRequest();
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> ReceiveMessage([FromBody] WebhookPayload payload)
    {
        await _webhookService.ProcessIncomingMessageAsync(payload);
        return Ok();
    }
}
