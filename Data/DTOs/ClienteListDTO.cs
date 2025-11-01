namespace WhatsAppBot.Data.DTOs
{
    public class ClienteListDTO
    {
        public int ClienteId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }
        public int TotalPedidos { get; set; }
        public DateTime? UltimoPedido { get; set; }
        public string EstadoDisplay => Activo ? "Activo" : "Inactivo";
        public string FechaRegistroDisplay => FechaRegistro.ToString("dd/MM/yyyy");
        public string UltimoPedidoDisplay => UltimoPedido?.ToString("dd/MM/yyyy") ?? "Sin pedidos";
    }

    public class ClientePagedResultDTO
    {
        public List<ClienteListDTO> Clientes { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public class ClienteSearchFilterDTO
    {
        public string? SearchTerm { get; set; }
        public bool? SoloActivos { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ClienteEstadisticasDTO
    {
        public int TotalClientes { get; set; }
        public int ClientesActivos { get; set; }
        public int ClientesInactivos { get; set; }
        public int ClientesNuevosEsteMes { get; set; }
        public int ClientesConPedidos { get; set; }
        public double PorcentajeActivos => TotalClientes > 0 ? (double)ClientesActivos / TotalClientes * 100 : 0;
        public double PorcentajeConPedidos => TotalClientes > 0 ? (double)ClientesConPedidos / TotalClientes * 100 : 0;
    }

    public class ClienteCreateDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public bool Activo { get; set; } = true;
    }

    public class ClienteUpdateDTO
    {
        public int ClienteId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
    }
}