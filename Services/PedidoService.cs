using WhatsAppBot.Models;
using WhatsAppBot.Services.Interfaces;
using WhatsAppBot.Data.Repositories.Interfaces;

namespace WhatsAppBot.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly IUnitOfWork _uow;

        public PedidoService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<Pedido>> ObtenerPedidosAsync()
        {
            var list = (await _uow.Pedidos.GetAllAsync()).ToList();
            return list.OrderByDescending(p => p.FechaPedido).ToList();
        }

        public async Task<Pedido?> ObtenerPedidoPorIdAsync(int id)
        {
            return await _uow.Pedidos.GetByIdAsync(id);
        }

        public async Task AgregarPedidoAsync(Pedido pedido)
        {
            await _uow.Pedidos.AddAsync(pedido);
            await _uow.CompleteAsync();
        }

        public async Task ActualizarEstadoAsync(int id, string nuevoEstado)
        {
            var pedido = await _uow.Pedidos.GetByIdAsync(id);
            if (pedido == null) return;

            pedido.Estado = nuevoEstado;
            await _uow.Pedidos.UpdateAsync(pedido);
            await _uow.CompleteAsync();
        }

        public async Task<Pedido> CrearAsync(Pedido pedido)
        {
            await _uow.Pedidos.AddAsync(pedido);
            await _uow.CompleteAsync();
            return pedido;
        }

        public async Task ActualizarAsync(Pedido pedido)
        {
            await _uow.Pedidos.UpdateAsync(pedido);
            await _uow.CompleteAsync();
        }

        public async Task<Pedido?> ObtenerPorFolioAsync(string folio)
        {
            return await _uow.Pedidos.GetByFolioAsync(folio);
        }

        public async Task<Pedido?> ObtenerUltimoPedidoAsync(string telefonoCliente)
        {
            return await _uow.Pedidos.GetLastOrderByPhoneAsync(telefonoCliente);
        }
        public string GenerarFolio()
        {
            string prefijo = "PED";
            string random = new Random().Next(1000, 9999).ToString();
            return $"{prefijo}{random}";
        }
    }
}
