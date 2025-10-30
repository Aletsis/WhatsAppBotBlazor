using WhatsAppBot.Models;

namespace WhatsAppBot.Services.Interfaces;

public interface IWebhookService
{
    Task ProcessIncomingMessageAsync(WebhookPayload payload);
}
