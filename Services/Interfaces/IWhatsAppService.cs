using WhatsAppBot.Models;

namespace WhatsAppBot.Services.Interfaces;

public interface IWhatsAppService
{
    Task<bool> SendMessageAsync(WhatsAppMessage message);
    Task<bool> SendInteractiveMessageAsync(string to, string body, string[] buttons);
}
