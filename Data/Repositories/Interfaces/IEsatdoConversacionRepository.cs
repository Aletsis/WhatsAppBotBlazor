using WhatsAppBot.Models;

namespace WhatsAppBot.Data.Repositories.Interfaces
{
    public interface IEstadoConversacionRepository : IRepository<EstadoConversacion>
    {
        Task<EstadoConversacion?> GetByPhoneAsync(string telefono);
        Task<EstadoConversacion> CreateOrUpdateAsync(string telefono, string estado);
    }
}