using System.Collections.Generic;
using WhatsAppBot.Models;

namespace WhatsAppBot.Data.DTOs
{
    public class ConversationDetail
    {
        public string PhoneNumber { get; set; } = string.Empty;

        // Reutiliza la entidad MensajeWhatsApp para agilizar; si prefieres, puedes crear un MessageDto
        public List<MensajeWhatsApp> Messages { get; set; } = new();
    }
}