using WhatsAppBot.Models;

namespace WhatsAppBot.Services.Interfaces
{
    public interface IClienteService
    {
        Task<Cliente?> ObtenerPorNumeroAsync(string numero);
        Task<Cliente> CrearAsync(Cliente cliente);
        Task ActualizarAsync(Cliente cliente);
    }
}
