using WhatsAppBot.Models;
using WhatsAppBot.Services.Interfaces;
using WhatsAppBot.Data.Repositories.Interfaces;

namespace WhatsAppBot.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IUnitOfWork _uow;

        public ClienteService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<Cliente?> ObtenerPorNumeroAsync(string numero)
        {
            return await _uow.Clientes.GetByPhoneAsync(numero);
        }

        public async Task<Cliente> CrearAsync(Cliente cliente)
        {
            await _uow.Clientes.AddAsync(cliente);
            await _uow.CompleteAsync();
            return cliente;
        }

        public async Task ActualizarAsync(Cliente cliente)
        {
            await _uow.Clientes.UpdateAsync(cliente);
            await _uow.CompleteAsync();
        }
    }
}
