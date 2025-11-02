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
        public async Task<IEnumerable<Pedido>> GetByDateRangeAsync(DateTime fechaDesde, DateTime fechaHasta)
        {
            return await _dbSet
                .Include(p => p.Cliente)
                .Where(p => p.FechaPedido >= fechaDesde && p.FechaPedido <= fechaHasta)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pedido>> GetByEstadoAsync(string estado)
        {
            return await _dbSet
                .Include(p => p.Cliente)
                .Where(p => p.Estado == estado)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pedido>> GetByFormaPagoAsync(string formaPago)
        {
            return await _dbSet
                .Include(p => p.Cliente)
                .Where(p => p.FormaPago == formaPago)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pedido>> GetWithFiltersAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? estado, string? formaPago, int? clienteId)
        {
            var query = _dbSet.Include(p => p.Cliente).AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(p => p.FechaPedido >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(p => p.FechaPedido <= fechaHasta.Value);

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(p => p.Estado == estado);

            if (!string.IsNullOrEmpty(formaPago))
                query = query.Where(p => p.FormaPago == formaPago);

            if (clienteId.HasValue)
                query = query.Where(p => p.ClienteId == clienteId.Value);

            return await query.OrderByDescending(p => p.FechaPedido).ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetEstadisticasPorEstadoAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var query = _dbSet.AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(p => p.FechaPedido >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(p => p.FechaPedido <= fechaHasta.Value);

            return await query
                .GroupBy(p => p.Estado)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetEstadisticasPorFormaPagoAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var query = _dbSet.AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(p => p.FechaPedido >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(p => p.FechaPedido <= fechaHasta.Value);

            return await query
                .Where(p => !string.IsNullOrEmpty(p.FormaPago))
                .GroupBy(p => p.FormaPago)
                .ToDictionaryAsync(g => g.Key!, g => g.Count());
        }

        public async Task<List<(int ClienteId, string ClienteNombre, string ClienteTelefono, int CantidadPedidos, DateTime UltimoPedido)>> GetClientesMasFrecuentesAsync(int cantidad = 10, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var query = _dbSet.Include(p => p.Cliente).AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(p => p.FechaPedido >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(p => p.FechaPedido <= fechaHasta.Value);

            return await query
                .GroupBy(p => new { p.ClienteId, p.Cliente!.Nombre, p.Cliente.Telefono })
                .Select(g => new
                {
                    ClienteId = g.Key.ClienteId,
                    ClienteNombre = g.Key.Nombre,
                    ClienteTelefono = g.Key.Telefono,
                    CantidadPedidos = g.Count(),
                    UltimoPedido = g.Max(p => p.FechaPedido)
                })
                .OrderByDescending(x => x.CantidadPedidos)
                .Take(cantidad)
                .Select(x => ValueTuple.Create(x.ClienteId, x.ClienteNombre, x.ClienteTelefono, x.CantidadPedidos, x.UltimoPedido))
                .ToListAsync();
        }

        public async Task<int> GetCountByDateRangeAsync(DateTime fechaDesde, DateTime fechaHasta)
        {
            return await _dbSet
                .CountAsync(p => p.FechaPedido >= fechaDesde && p.FechaPedido <= fechaHasta);
        }

        public async Task<List<(DateTime Fecha, int Cantidad)>> GetPedidosPorDiaAsync(DateTime fechaDesde, DateTime fechaHasta)
        {
            return await _dbSet
                .Where(p => p.FechaPedido >= fechaDesde && p.FechaPedido <= fechaHasta)
                .GroupBy(p => p.FechaPedido.Date)
                .Select(g => new { Fecha = g.Key, Cantidad = g.Count() })
                .OrderBy(x => x.Fecha)
                .Select(x => ValueTuple.Create(x.Fecha, x.Cantidad))
                .ToListAsync();
        }
    }
}