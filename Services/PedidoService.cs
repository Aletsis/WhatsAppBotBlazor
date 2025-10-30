using Microsoft.EntityFrameworkCore;
using WhatsAppBot.Data;
using WhatsAppBot.Models;
using WhatsAppBot.Services.Interfaces;

namespace WhatsAppBot.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly WhatsAppDbContext _context;

        public PedidoService(WhatsAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Pedido>> ObtenerPedidosAsync()
        {
            return await _context.Pedidos.OrderByDescending(p => p.FechaPedido).ToListAsync();
        }

        public async Task<Pedido?> ObtenerPedidoPorIdAsync(int id)
        {
            return await _context.Pedidos.FindAsync(id);
        }

        public async Task AgregarPedidoAsync(Pedido pedido)
        {
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarEstadoAsync(int id, string nuevoEstado)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return;

            pedido.Estado = nuevoEstado;
            await _context.SaveChangesAsync();
        }

        public async Task<Pedido> CrearAsync(Pedido pedido)
        {
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
            return pedido;
        }

        public async Task ActualizarAsync(Pedido pedido)
        {
            _context.Pedidos.Update(pedido);
            await _context.SaveChangesAsync();
        }

        public async Task<Pedido?> ObtenerPorFolioAsync(string folio)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.Folio == folio);
        }

        public async Task<Pedido?> ObtenerUltimoPedidoAsync(string telefonoCliente)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Where(p => p.Cliente.Telefono == telefonoCliente)
                .OrderByDescending(p => p.FechaPedido)
                .FirstOrDefaultAsync();
        }
        public string GenerarFolio()
        {
            string prefijo = "PED";
            string random = new Random().Next(1000, 9999).ToString();
            return $"{prefijo}{random}";
        }
    }
}
