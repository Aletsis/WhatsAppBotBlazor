using WhatsAppBot.Models;

namespace WhatsAppBot.Services.Interfaces
{
    public interface IPedidoService
    {
        Task<List<Pedido>> ObtenerPedidosAsync();
        Task<Pedido?> ObtenerPedidoPorIdAsync(int id);
        Task AgregarPedidoAsync(Pedido pedido);
        Task ActualizarEstadoAsync(int id, string nuevoEstado);
        Task<Pedido> CrearAsync(Pedido pedido);
        Task ActualizarAsync(Pedido pedido);
        Task<Pedido?> ObtenerPorFolioAsync(string folio);
        Task<Pedido?> ObtenerUltimoPedidoAsync(string telefonoCliente);
        string GenerarFolio();
    }
}
