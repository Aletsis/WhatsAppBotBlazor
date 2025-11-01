namespace WhatsAppBot.Data.DTOs
{
    public class DashboardMetricsDTO
    {
        public int PedidosHoy { get; set; }
        public double PedidosTrend { get; set; }
        public int ConversacionesActivas { get; set; }
        public int ClientesNuevos { get; set; }
        public int MensajesEnviados { get; set; }
        public int MensajesRecibidos { get; set; }
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }

    public class EstadoPedidoMetricDTO
    {
        public string Estado { get; set; } = "";
        public int Cantidad { get; set; }
        public double Porcentaje { get; set; }
        public string Color { get; set; } = "";
    }

    public class PedidoRecenteDTO
    {
        public int PedidoId { get; set; }
        public string Folio { get; set; } = "";
        public string ClienteNombre { get; set; } = "";
        public string ClienteTelefono { get; set; } = "";
        public string Estado { get; set; } = "";
        public DateTime FechaPedido { get; set; }
        public string DetallePedido { get; set; } = "";
    }

    public class ConversacionActivaDTO
    {
        public string Telefono { get; set; } = "";
        public DateTime UltimaActividad { get; set; }
        public int MensajesHoy { get; set; }
        public string EstadoConversacion { get; set; } = "";
    }
}