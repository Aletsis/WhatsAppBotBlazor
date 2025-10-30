using System;
using System.Collections.Generic;

namespace WhatsAppBot.Models
{
    public class Cliente
    {
        public int ClienteId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public bool Activo { get; set; } = true;

        // Relación: Un cliente puede tener varios pedidos
        public ICollection<Pedido>? Pedidos { get; set; }
    }
}
