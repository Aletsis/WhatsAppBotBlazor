using WhatsAppBot.Models;

namespace WhatsAppBot.Data.Repositories.Interfaces
{
    public interface IPedidoRepository : IRepository<Pedido>
    {
        Task<Pedido?> GetByFolioAsync(string folio);
        Task<IEnumerable<Pedido>> GetByClienteIdAsync(int clienteId);
        Task<Pedido?> GetLastOrderByPhoneAsync(string telefono);
    }
}
