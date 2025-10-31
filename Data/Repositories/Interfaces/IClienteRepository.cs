using WhatsAppBot.Models;

namespace WhatsAppBot.Data.Repositories.Interfaces
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<Cliente?> GetByPhoneAsync(string telefono);
    }
}
