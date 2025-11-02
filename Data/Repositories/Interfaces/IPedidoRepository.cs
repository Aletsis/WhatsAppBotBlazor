using WhatsAppBot.Models;

namespace WhatsAppBot.Data.Repositories.Interfaces
{
    public interface IPedidoRepository : IRepository<Pedido>
    {
        Task<Pedido?> GetByFolioAsync(string folio);
        Task<IEnumerable<Pedido>> GetByClienteIdAsync(int clienteId);
        Task<Pedido?> GetLastOrderByPhoneAsync(string telefono);
        Task<IEnumerable<Pedido>> GetByDateRangeAsync(DateTime fechaDesde, DateTime fechaHasta);
        Task<IEnumerable<Pedido>> GetByEstadoAsync(string estado);
        Task<IEnumerable<Pedido>> GetByFormaPagoAsync(string formaPago);
        Task<IEnumerable<Pedido>> GetWithFiltersAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? estado, string? formaPago, int? clienteId);
        Task<Dictionary<string, int>> GetEstadisticasPorEstadoAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null);
        Task<Dictionary<string, int>> GetEstadisticasPorFormaPagoAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null);
        Task<List<(int ClienteId, string ClienteNombre, string ClienteTelefono, int CantidadPedidos, DateTime UltimoPedido)>> GetClientesMasFrecuentesAsync(int cantidad = 10, DateTime? fechaDesde = null, DateTime? fechaHasta = null);
        Task<int> GetCountByDateRangeAsync(DateTime fechaDesde, DateTime fechaHasta);
        Task<List<(DateTime Fecha, int Cantidad)>> GetPedidosPorDiaAsync(DateTime fechaDesde, DateTime fechaHasta);
    }
}
