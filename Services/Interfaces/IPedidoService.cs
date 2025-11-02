using WhatsAppBot.Models;
using WhatsAppBot.Data.DTOs;

namespace WhatsAppBot.Services.Interfaces
{
    public interface IPedidoService
    {
        // Métodos existentes
        Task<List<Pedido>> ObtenerPedidosAsync();
        Task<Pedido?> ObtenerPedidoPorIdAsync(int id);
        Task AgregarPedidoAsync(Pedido pedido);
        Task ActualizarEstadoAsync(int id, string nuevoEstado);
        Task<Pedido> CrearAsync(Pedido pedido);
        Task ActualizarAsync(Pedido pedido);
        Task<Pedido?> ObtenerPorFolioAsync(string folio);
        Task<Pedido?> ObtenerUltimoPedidoAsync(string telefonoCliente);
        string GenerarFolio();
        Task<ReportePedidosDTO> GenerarReporteAsync(ReporteFiltrosDTO filtros);
        Task<List<VentasPorDiaDTO>> ObtenerVentasPorDiaAsync(DateTime fechaDesde, DateTime fechaHasta);
        Task<List<VentasPorEstadoDTO>> ObtenerVentasPorEstadoAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null);
        Task<List<ClienteTopDTO>> ObtenerClientesTopAsync(int cantidad = 10, DateTime? fechaDesde = null, DateTime? fechaHasta = null);
        Task<List<ProductoPopularDTO>> ObtenerProductosPopularesAsync(int cantidad = 10, DateTime? fechaDesde = null, DateTime? fechaHasta = null);
        Task<TendenciasDTO> ObtenerTendenciasAsync();
        Task<ResumenVentasDTO> ObtenerResumenVentasAsync(DateTime fechaDesde, DateTime fechaHasta);
        Task<List<Pedido>> ObtenerPedidosFiltradosAsync(ReporteFiltrosDTO filtros);
        Task<byte[]> ExportarReporteAsync(ReporteFiltrosDTO filtros, string formato = "PDF");
    }
}
