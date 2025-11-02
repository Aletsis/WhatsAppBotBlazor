namespace WhatsAppBot.Data.DTOs
{
    public class ReportePedidosDTO
    {
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public ResumenVentasDTO ResumenVentas { get; set; } = new();
        public List<VentasPorDiaDTO> VentasPorDia { get; set; } = new();
        public List<VentasPorEstadoDTO> VentasPorEstado { get; set; } = new();
        public List<VentasPorFormapagoDTO> VentasPorFormaPago { get; set; } = new();
        public List<ClienteTopDTO> ClientesTop { get; set; } = new();
        public List<ProductoPopularDTO> ProductosPopulares { get; set; } = new();
        public TendenciasDTO Tendencias { get; set; } = new();
    }

    public class ResumenVentasDTO
    {
        public int TotalPedidos { get; set; }
        public int PedidosCompletados { get; set; }
        public int PedidosPendientes { get; set; }
        public int PedidosCancelados { get; set; }
        public double TasaCompletitud => TotalPedidos > 0 ? (double)PedidosCompletados / TotalPedidos * 100 : 0;
        public double TasaCancelacion => TotalPedidos > 0 ? (double)PedidosCancelados / TotalPedidos * 100 : 0;
        public double PromedioClienteNuevo { get; set; }
        public double PromedioClienteRecurrente { get; set; }
    }

    public class VentasPorDiaDTO
    {
        public DateTime Fecha { get; set; }
        public int CantidadPedidos { get; set; }
        public int PedidosCompletados { get; set; }
        public int PedidosCancelados { get; set; }
        public string FechaDisplay => Fecha.ToString("dd/MM/yyyy");
        public string DiaSemana => Fecha.ToString("dddd", new System.Globalization.CultureInfo("es-ES"));
    }

    public class VentasPorEstadoDTO
    {
        public string Estado { get; set; } = "";
        public int Cantidad { get; set; }
        public double Porcentaje { get; set; }
        public string Color { get; set; } = "";
        public string IconoMaterial { get; set; } = "";
    }

    public class VentasPorFormapagoDTO
    {
        public string FormaPago { get; set; } = "";
        public int Cantidad { get; set; }
        public double Porcentaje { get; set; }
        public string Color { get; set; } = "";
    }

    public class ClienteTopDTO
    {
        public int ClienteId { get; set; }
        public string Nombre { get; set; } = "";
        public string Telefono { get; set; } = "";
        public int CantidadPedidos { get; set; }
        public DateTime UltimoPedido { get; set; }
        public string UltimoPedidoDisplay => UltimoPedido.ToString("dd/MM/yyyy");
        public string TipoCliente => CantidadPedidos >= 5 ? "VIP" : CantidadPedidos >= 3 ? "Frecuente" : "Regular";
    }

    public class ProductoPopularDTO
    {
        public string Producto { get; set; } = "";
        public int Menciones { get; set; }
        public double Porcentaje { get; set; }
        public List<string> ClientesFrecuentes { get; set; } = new();
    }

    public class TendenciasDTO
    {
        public double TendenciaSemanal { get; set; }
        public double TendenciaMensual { get; set; }
        public string MejorDiaSemana { get; set; } = "";
        public string HoraPico { get; set; } = "";
        public int CrecimientoClientesNuevos { get; set; }
        public string TendenciaSemanalDisplay => TendenciaSemanal >= 0 ? $"+{TendenciaSemanal:F1}%" : $"{TendenciaSemanal:F1}%";
        public string TendenciaMensualDisplay => TendenciaMensual >= 0 ? $"+{TendenciaMensual:F1}%" : $"{TendenciaMensual:F1}%";
    }

    public class ReporteFiltrosDTO
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? Estado { get; set; }
        public string? FormaPago { get; set; }
        public int? ClienteId { get; set; }
        public bool IncluirCancelados { get; set; } = true;
        public TipoReporte TipoReporte { get; set; } = TipoReporte.Semanal;
    }

    public enum TipoReporte
    {
        Diario,
        Semanal,
        Mensual,
        Trimestral,
        Anual,
        Personalizado
    }

    public class ReporteExportDTO
    {
        public string Titulo { get; set; } = "";
        public DateTime FechaGeneracion { get; set; } = DateTime.Now;
        public string UsuarioGenerador { get; set; } = "";
        public ReportePedidosDTO Datos { get; set; } = new();
        public string FormatoExport { get; set; } = "PDF"; // PDF, Excel, CSV
    }
}