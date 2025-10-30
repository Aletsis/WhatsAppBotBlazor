using System;

namespace WhatsAppBot.Models
{
    public class MensajeWhatsApp
    {
        public int MensajeId { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string? MensajeTexto { get; set; }
        public string DireccionConversacion { get; set; } = "entrada"; // entrada/salida
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string? EstadoConversacion { get; set; }
    }
}
