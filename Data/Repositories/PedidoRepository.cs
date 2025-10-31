using Microsoft.EntityFrameworkCore;
using WhatsAppBot.Models;
using WhatsAppBot.Data.Repositories.Interfaces;

namespace WhatsAppBot.Data.Repositories
{
    public class PedidoRepository : Repository<Pedido>, IPedidoRepository
    {
        public PedidoRepository(WhatsAppDbContext context) : base(context)
        {
        }

        public async Task<Pedido?> GetByFolioAsync(string folio)
        {
            return await _dbSet
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.Folio == folio);
        }

        public async Task<IEnumerable<Pedido>> GetByClienteIdAsync(int clienteId)
        {
            return await _dbSet
                .Include(p => p.Cliente)
                .Where(p => p.ClienteId == clienteId)
                .ToListAsync();
        }

        public async Task<Pedido?> GetLastOrderByPhoneAsync(string telefono)
        {
            return await _dbSet
                .Include(p => p.Cliente)
                .Where(p => p.Cliente.Telefono == telefono)
                .OrderByDescending(p => p.FechaPedido)
                .FirstOrDefaultAsync();
        }
    }
}