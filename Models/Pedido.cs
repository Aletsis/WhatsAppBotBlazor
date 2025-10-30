using System;

namespace WhatsAppBot.Models
{
    public class Pedido
    {
        public int PedidoId { get; set; }
        public int ClienteId { get; set; }
        public DateTime FechaPedido { get; set; } = DateTime.Now;
        public string DetallePedido { get; set; } = string.Empty;
        public string? FormaPago { get; set; }
        public string? DireccionEntrega { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public string Folio { get; set; } = string.Empty;

        // Navegación
        public Cliente? Cliente { get; set; }
    }
}
