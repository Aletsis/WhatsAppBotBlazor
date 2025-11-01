using Microsoft.EntityFrameworkCore;
using WhatsAppBot.Models;
using WhatsAppBot.Data.Repositories.Interfaces;

namespace WhatsAppBot.Data.Repositories
{
    public class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        public ClienteRepository(WhatsAppDbContext context) : base(context) { }

        public async Task<Cliente?> GetByPhoneAsync(string telefono)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Telefono == telefono);
        }
        public async Task<(IEnumerable<Cliente> Clientes, int TotalCount)> GetPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _dbSet.Include(c => c.Pedidos).AsQueryable();

            // Aplicar filtro de búsqueda si existe
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => 
                    c.Nombre.Contains(searchTerm) || 
                    c.Telefono.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            
            var clientes = await query
                .OrderByDescending(c => c.FechaRegistro)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (clientes, totalCount);
        }

        public async Task<IEnumerable<Cliente>> GetActiveClientsAsync()
        {
            return await _dbSet
                .Where(c => c.Activo)
                .Include(c => c.Pedidos)
                .OrderByDescending(c => c.FechaRegistro)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cliente>> GetClientsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(c => c.FechaRegistro >= startDate && c.FechaRegistro <= endDate)
                .Include(c => c.Pedidos)
                .OrderByDescending(c => c.FechaRegistro)
                .ToListAsync();
        }

        public async Task<bool> ExistsPhoneAsync(string telefono, int? excludeClienteId = null)
        {
            var query = _dbSet.Where(c => c.Telefono == telefono);
            
            if (excludeClienteId.HasValue)
            {
                query = query.Where(c => c.ClienteId != excludeClienteId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> GetTotalActiveCountAsync()
        {
            return await _dbSet.CountAsync(c => c.Activo);
        }

        public async Task<IEnumerable<Cliente>> SearchByNameOrPhoneAsync(string searchTerm)
        {
            return await _dbSet
                .Where(c => c.Nombre.Contains(searchTerm) || c.Telefono.Contains(searchTerm))
                .Include(c => c.Pedidos)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }
    }
}