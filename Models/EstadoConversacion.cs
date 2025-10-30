using System;

namespace WhatsAppBot.Models
{
    public class EstadoConversacion
    {
        public string Telefono { get; set; } = string.Empty;
        public string EstadoActual { get; set; } = "Inicio";
        public DateTime UltimaActualizacion { get; set; } = DateTime.Now;
    }
}
