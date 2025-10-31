namespace WhatsAppBot.Data.DTOs
{
    public class PedidoDTO
    {
        public int PedidoId { get; set; }
        public string Folio { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteTelefono { get; set; } = string.Empty;
        public DateTime FechaPedido { get; set; }
        public string DetallePedido { get; set; } = string.Empty;
        public string? FormaPago { get; set; }
        public string? DireccionEntrega { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
